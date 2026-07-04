using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsNetTool.Tools.Arp;
using WindowsNetTool.Tools.DnsLookup;
using WindowsNetTool.Tools.Ping;
// The System.Net.NetworkInformation.Ping type must be aliased because the simple name "Ping"
// binds to the WindowsNetTool.Tools.Ping namespace.
using NetPing = System.Net.NetworkInformation.Ping;

namespace WindowsNetTool.Tools.IpScanner
{
	/// <summary>
	/// Discovers hosts on a subnet by pinging every address in it, wave after wave, until stopped,
	/// so hosts that fail to answer one wave are caught by a later one.  The system ARP table is
	/// merged into the results each wave, which fills in MAC addresses and also surfaces hosts
	/// that do not answer pings (shown with a ping time of "N/A").  Host names are resolved with
	/// reverse DNS, but private addresses are only ever looked up on a local DNS server, never a
	/// public resolver.
	/// </summary>
	public partial class IpScannerTool : UserControl, IRefreshOnActivate
	{
		/// <summary>
		/// Time to wait for each echo reply.  Shorter than the Ping tool's 4000 ms because scan
		/// speed on sparse subnets is bounded by (dead addresses / in-flight limit) * timeout, and
		/// hosts slower than this are still discovered by a later wave.
		/// </summary>
		private const int ScanPingTimeoutMs = 1500;

		/// <summary>Timeout for each reverse DNS query.</summary>
		private const int DnsTimeoutMs = 2000;

		/// <summary>How many reverse DNS lookups may run at once; kept low to be gentle on home routers.</summary>
		private const int MaxDnsConcurrency = 4;

		/// <summary>Pause between the end of one ping wave and the start of the next.</summary>
		private const int RescanDelayMs = 2000;

		/// <summary>Shortest allowed prefix length; /16 = 65,534 addresses is already a slow scan.</summary>
		private const int MinScanPrefix = 16;

		/// <summary>One discovered host.  Referenced by the ListViewItem's Tag, like ArpEntry in the ARP tool.</summary>
		private class ScanResult
		{
			public IPAddress IpAddress;
			public string IpText;
			/// <summary>The IP address as a big-endian number so entries sort in numeric address order.</summary>
			public uint IpSortKey;
			/// <summary>Latest successful round-trip time in ms, or -1 if the host has never answered a ping.</summary>
			public long PingRtt = -1;
			/// <summary>When the last successful ping reply arrived; MinValue if never.</summary>
			public DateTime LastReply = DateTime.MinValue;
			public string Hostname = "";
			public string MacAddress = "";
			public string MacDigits = "";
			/// <summary>True once a reverse DNS lookup has been queued; reset on timeout so a later wave retries.</summary>
			public bool DnsAttempted;
			public ListViewItem Item;
			public bool Dirty;
		}

		private class SortKey
		{
			public int Column;
			public bool Descending;
		}

		/// <summary>One entry in the subnet dropdown.</summary>
		private class SubnetChoice
		{
			public string Cidr;
			public string InterfaceName;
			public override string ToString()
			{
				return Cidr + "  (" + InterfaceName + ")";
			}
		}

		private bool running = false;
		/// <summary>Incremented whenever scanning starts or stops so callbacks from an earlier session are ignored.</summary>
		private int session = 0;
		private uint scanNetwork;
		private int scanPrefix;
		/// <summary>All discovered hosts of the current session, keyed by IpSortKey.</summary>
		private readonly Dictionary<uint, ScanResult> results = new Dictionary<uint, ScanResult>();
		/// <summary>Hosts discovered since the last flush, awaiting insertion into the list.</summary>
		private readonly List<ScanResult> pendingNew = new List<ScanResult>();
		/// <summary>Hosts whose displayed values changed since the last flush.</summary>
		private readonly List<ScanResult> dirtyResults = new List<ScanResult>();
		/// <summary>
		/// The column sort order chosen by clicking column headers, in priority order.  Empty
		/// until the first click; IP address order is the default and the tie-breaker.
		/// </summary>
		private readonly List<SortKey> sortKeys = new List<SortKey>();
		/// <summary>Limits how many reverse DNS lookups run concurrently.</summary>
		private readonly SemaphoreSlim dnsGate = new SemaphoreSlim(MaxDnsConcurrency);
		/// <summary>A system DNS server suitable for reverse-resolving private addresses (private or on-link), or null.</summary>
		private IPAddress dnsLocal;
		/// <summary>Any system DNS server, used for reverse-resolving public addresses, or null.</summary>
		private IPAddress dnsAny;
		/// <summary>Appended to the status line when reverse DNS had to be disabled for this scan.</summary>
		private string dnsNote = "";
		private int waveNumber, waveTotal, waveCompleted;

		public IpScannerTool()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignMode)
				PopulateSubnetList();
		}

		/// <summary>
		/// Rebuilds the subnet dropdown when the user returns to this tool, because interface
		/// addresses may have changed in the meantime (possibly via the IP Configuration tool).
		/// </summary>
		public void RefreshOnActivate()
		{
			PopulateSubnetList();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			// MainForm hides this control when the user switches to a different tool; scanning
			// should not continue in the background.
			if (!Visible && running)
				StopScan();
		}

		/// <summary>
		/// Fills the subnet dropdown with the IPv4 subnets of the machine's operational interfaces,
		/// listing subnets that have a default gateway first (those are usually the LAN the user
		/// wants to scan).  The user's typed text is preserved across refreshes.
		/// </summary>
		private void PopulateSubnetList()
		{
			string previousText = comboSubnet.Text;
			List<SubnetChoice> withGateway = new List<SubnetChoice>();
			List<SubnetChoice> withoutGateway = new List<SubnetChoice>();
			HashSet<string> seen = new HashSet<string>();
			try
			{
				foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
				{
					if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
						continue;
					IPInterfaceProperties props = nic.GetIPProperties();
					bool hasGateway = props.GatewayAddresses.Count > 0;
					foreach (UnicastIPAddressInformation ua in props.UnicastAddresses)
					{
						if (ua.Address.AddressFamily != AddressFamily.InterNetwork || ua.IPv4Mask == null)
							continue;
						int prefix = PrefixFromMask(ToUint(ua.IPv4Mask));
						// /31 and /32 (point-to-point and VPN addresses) are pointless to scan.
						if (prefix < 8 || prefix > 30)
							continue;
						string cidr = FromUint(ToUint(ua.Address) & MaskOf(prefix)).ToString() + "/" + prefix;
						if (!seen.Add(cidr + "|" + nic.Name))
							continue;
						(hasGateway ? withGateway : withoutGateway).Add(new SubnetChoice { Cidr = cidr, InterfaceName = nic.Name });
					}
				}
			}
			catch (NetworkInformationException)
			{
				// The dropdown stays empty; a subnet can still be typed in manually.
			}
			comboSubnet.BeginUpdate();
			comboSubnet.Items.Clear();
			foreach (SubnetChoice choice in withGateway)
				comboSubnet.Items.Add(choice);
			foreach (SubnetChoice choice in withoutGateway)
				comboSubnet.Items.Add(choice);
			comboSubnet.EndUpdate();
			if (previousText.Length > 0)
				comboSubnet.Text = previousText;
			else if (comboSubnet.Items.Count > 0)
				comboSubnet.SelectedIndex = 0;
		}

		private void btnStartStop_Click(object sender, EventArgs e)
		{
			if (running)
				StopScan();
			else
				StartScan();
		}

		private void comboSubnet_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
				if (!running)
					StartScan();
			}
		}

		/// <summary>
		/// The scan loop.  Each iteration merges the ARP table into the results, pings every
		/// address in the subnet, queues reverse DNS work, then pauses before rescanning.  The
		/// loop exits when the session counter changes (Stop pressed or the tool deactivated).
		/// </summary>
		private async void StartScan()
		{
			if (running)
				return;
			string text = comboSubnet.Text.Trim();
			int space = text.IndexOf(' ');
			if (space >= 0)
				text = text.Substring(0, space); // Drop the "(interface)" suffix of dropdown entries.
			uint network;
			int prefix;
			string error;
			if (!TryParseCidr(text, out network, out prefix, out error))
			{
				MessageBox.Show(this, error, "IP Scanner", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			if (prefix < MinScanPrefix)
			{
				MessageBox.Show(this, "Subnets larger than /" + MinScanPrefix + " are not supported.  A /" + prefix
					+ " subnet contains " + ((1L << (32 - prefix)) - 2).ToString("N0") + " addresses.",
					"IP Scanner", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			session++;
			int mySession = session;
			running = true;
			scanNetwork = network;
			scanPrefix = prefix;
			results.Clear();
			pendingNew.Clear();
			dirtyResults.Clear();
			listResults.Items.Clear();
			ChooseDnsServers();
			dnsNote = dnsLocal == null && IsPrivateIPv4(network) ? "   (reverse DNS unavailable: no local DNS server)" : "";
			waveNumber = 0;
			waveCompleted = 0;
			btnStartStop.Text = "Stop";
			comboSubnet.Enabled = false;
			timerFlush.Start();

			List<IPAddress> addresses = EnumerateAddresses(network, prefix);
			waveTotal = addresses.Count;
			while (session == mySession && !IsDisposed)
			{
				waveNumber++;
				waveCompleted = 0;
				await MergeArpEntriesAsync(mySession);
				if (session != mySession)
					break;
				await RunPingWave(addresses, mySession);
				if (session != mySession)
					break;
				// Queue reverse lookups for hosts found only via ARP, and retry ones that timed out.
				foreach (ScanResult result in results.Values)
					if (!result.DnsAttempted)
						BeginHostnameLookup(result, mySession);
				await Task.Delay(RescanDelayMs);
			}
		}

		private void StopScan()
		{
			if (!running)
				return;
			session++;
			running = false;
			timerFlush.Stop();
			FlushUpdates();
			btnStartStop.Text = "Start";
			comboSubnet.Enabled = true;
		}

		/// <summary>
		/// Pings every address once, keeping at most the configured number of pings in flight.
		/// The limit is read at the start of each wave, so adjusting the spinner while scanning
		/// takes effect on the next wave.  The semaphore self-paces the sends: each completed
		/// ping (reply or timeout) releases a slot for the next address.
		/// </summary>
		private async Task RunPingWave(List<IPAddress> addresses, int mySession)
		{
			int limit = (int)numInFlight.Value;
			SemaphoreSlim gate = new SemaphoreSlim(limit, limit);
			List<Task> tasks = new List<Task>(addresses.Count);
			foreach (IPAddress address in addresses)
			{
				await gate.WaitAsync();
				if (session != mySession)
				{
					gate.Release();
					break;
				}
				tasks.Add(PingOne(address, mySession, gate));
			}
			await Task.WhenAll(tasks);
		}

		private async Task PingOne(IPAddress address, int mySession, SemaphoreSlim gate)
		{
			try
			{
				// A Ping instance only supports one operation at a time, so each concurrent send
				// gets its own.
				using (NetPing ping = new NetPing())
				{
					PingReply reply = await ping.SendPingAsync(address, ScanPingTimeoutMs);
					if (session == mySession && !IsDisposed && reply.Status == IPStatus.Success)
						RecordPingReply(address, reply.RoundtripTime, mySession);
				}
			}
			catch
			{
				// Ping errors are routine while scanning (transient ICMP failures, unreachable
				// routes); the next wave retries every address anyway.
			}
			finally
			{
				if (session == mySession)
					waveCompleted++;
				gate.Release();
			}
		}

		private void RecordPingReply(IPAddress address, long rtt, int mySession)
		{
			ScanResult result = GetOrCreateResult(ToUint(address), address, mySession);
			result.PingRtt = rtt;
			result.LastReply = DateTime.Now;
			MarkDirty(result);
		}

		/// <summary>
		/// Merges the system ARP table into the results: MAC addresses for hosts already found by
		/// ping (pinging them created their ARP entries), plus hosts that never answer pings but
		/// appear in the ARP table because of other traffic.
		/// </summary>
		private async Task MergeArpEntriesAsync(int mySession)
		{
			List<ArpEntry> entries;
			try
			{
				entries = await Task.Run(() => IpHelperArp.GetArpEntries());
			}
			catch
			{
				return; // A failed ARP read should not stop the scan; ping results still flow.
			}
			if (session != mySession || IsDisposed)
				return;
			uint mask = MaskOf(scanPrefix);
			uint broadcast = scanNetwork | ~mask;
			foreach (ArpEntry entry in entries)
			{
				if ((entry.IpSortKey & mask) != scanNetwork)
					continue;
				if (scanPrefix <= 30 && (entry.IpSortKey == scanNetwork || entry.IpSortKey == broadcast))
					continue;
				ScanResult result = GetOrCreateResult(entry.IpSortKey, entry.IpAddress, mySession);
				if (entry.MacDigits.Length > 0 && entry.MacDigits != result.MacDigits)
				{
					result.MacAddress = entry.MacAddress;
					result.MacDigits = entry.MacDigits;
					MarkDirty(result);
				}
			}
		}

		private ScanResult GetOrCreateResult(uint key, IPAddress address, int mySession)
		{
			ScanResult result;
			if (!results.TryGetValue(key, out result))
			{
				result = new ScanResult { IpAddress = address, IpText = address.ToString(), IpSortKey = key };
				results.Add(key, result);
				pendingNew.Add(result);
				BeginHostnameLookup(result, mySession);
			}
			return result;
		}

		/// <summary>
		/// Resolves a host name via reverse DNS.  Private addresses are only ever queried against
		/// a local DNS server; if the system has none (e.g. its only resolver is a public one),
		/// the lookup is skipped entirely so private addresses never leak to public resolvers.
		/// </summary>
		private async void BeginHostnameLookup(ScanResult result, int mySession)
		{
			result.DnsAttempted = true;
			IPAddress server = IsPrivateIPv4(result.IpSortKey) ? dnsLocal : (dnsAny ?? dnsLocal);
			if (server == null)
				return;
			await dnsGate.WaitAsync();
			try
			{
				if (session != mySession || IsDisposed)
					return;
				IPAddress target = result.IpAddress;
				string host = await Task.Run(() => TryReverseLookup(server, target));
				if (session != mySession || IsDisposed)
					return;
				if (host == null)
				{
					// The DNS server did not respond; allow the next wave to retry.
					result.DnsAttempted = false;
					return;
				}
				if (host.Length > 0 && host != result.Hostname)
				{
					result.Hostname = host;
					MarkDirty(result);
				}
			}
			finally
			{
				dnsGate.Release();
			}
		}

		/// <summary>
		/// Returns the host name, "" when the address definitively has no name (so the lookup is
		/// not repeated), or null when the query timed out (so a later wave retries it).
		/// </summary>
		private static string TryReverseLookup(IPAddress server, IPAddress target)
		{
			try
			{
				DnsResponse response = DnsClient.Query(server, DnsClient.ReverseName(target), DnsRecordType.PTR, DnsTimeoutMs);
				foreach (DnsRecord record in response.Answers)
					if (record.Type == (ushort)DnsRecordType.PTR)
						return record.Data;
				return "";
			}
			catch (TimeoutException)
			{
				return null;
			}
			catch (IOException)
			{
				return null;
			}
			catch
			{
				return ""; // Malformed response, refused, etc.: do not retry every wave.
			}
		}

		/// <summary>
		/// Picks the DNS servers used for reverse lookups from the system's configured servers:
		/// dnsLocal is one that is a private or directly-attached (on-link) address, dnsAny is
		/// simply the first IPv4 server.  Either may be null.
		/// </summary>
		private void ChooseDnsServers()
		{
			dnsLocal = null;
			dnsAny = null;
			List<uint> onLinkNetworks = new List<uint>();
			List<uint> onLinkMasks = new List<uint>();
			try
			{
				NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
				foreach (NetworkInterface nic in nics)
				{
					if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
						continue;
					foreach (UnicastIPAddressInformation ua in nic.GetIPProperties().UnicastAddresses)
					{
						if (ua.Address.AddressFamily != AddressFamily.InterNetwork || ua.IPv4Mask == null)
							continue;
						int prefix = PrefixFromMask(ToUint(ua.IPv4Mask));
						if (prefix <= 0)
							continue;
						onLinkNetworks.Add(ToUint(ua.Address) & MaskOf(prefix));
						onLinkMasks.Add(MaskOf(prefix));
					}
				}
				foreach (NetworkInterface nic in nics)
				{
					if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
						continue;
					foreach (IPAddress dns in nic.GetIPProperties().DnsAddresses)
					{
						if (dns.AddressFamily != AddressFamily.InterNetwork)
							continue;
						uint address = ToUint(dns);
						if (dnsAny == null)
							dnsAny = dns;
						if (dnsLocal == null && (IsPrivateIPv4(address) || IsOnLink(address, onLinkNetworks, onLinkMasks)))
							dnsLocal = dns;
					}
				}
			}
			catch (NetworkInformationException)
			{
				// No DNS servers found; host name lookups are skipped for this scan.
			}
		}

		private static bool IsOnLink(uint address, List<uint> networks, List<uint> masks)
		{
			for (int i = 0; i < networks.Count; i++)
				if ((address & masks[i]) == networks[i])
					return true;
			return false;
		}

		/// <summary>
		/// True for addresses that public DNS servers cannot meaningfully reverse-resolve:
		/// RFC 1918 private ranges, loopback, link-local, and carrier-grade NAT (100.64/10).
		/// </summary>
		private static bool IsPrivateIPv4(uint address)
		{
			byte a = (byte)(address >> 24);
			byte b = (byte)(address >> 16);
			return a == 10
				|| (a == 172 && b >= 16 && b <= 31)
				|| (a == 192 && b == 168)
				|| (a == 169 && b == 254)
				|| (a == 100 && b >= 64 && b <= 127)
				|| a == 127;
		}

		#region List updating

		private void MarkDirty(ScanResult result)
		{
			if (result.Dirty)
				return;
			result.Dirty = true;
			dirtyResults.Add(result);
		}

		private void timerFlush_Tick(object sender, EventArgs e)
		{
			FlushUpdates();
		}

		/// <summary>
		/// Applies accumulated changes to the ListView.  Batching on a timer (rather than updating
		/// per ping reply) and rewriting only cells whose text actually changed keeps the list
		/// flicker-free and cheap even when hundreds of replies arrive per second.  Rows keep
		/// their positions while values change; new rows are inserted at their sorted position.
		/// </summary>
		private void FlushUpdates()
		{
			if (pendingNew.Count > 0)
			{
				listResults.BeginUpdate();
				foreach (ScanResult result in pendingNew)
				{
					result.Item = CreateItem(result);
					InsertSorted(result.Item);
				}
				pendingNew.Clear();
				listResults.EndUpdate();
			}
			if (dirtyResults.Count > 0)
			{
				foreach (ScanResult result in dirtyResults)
				{
					result.Dirty = false;
					if (result.Item != null)
						UpdateItemText(result);
				}
				dirtyResults.Clear();
			}
			UpdateStatus();
		}

		private static ListViewItem CreateItem(ScanResult result)
		{
			ListViewItem item = new ListViewItem(new string[]
			{
				result.IpText,
				PingText(result),
				result.Hostname,
				result.MacAddress,
				LastReplyText(result)
			});
			item.Tag = result;
			return item;
		}

		private void InsertSorted(ListViewItem item)
		{
			ScanResult result = (ScanResult)item.Tag;
			int lo = 0, hi = listResults.Items.Count;
			while (lo < hi)
			{
				int mid = (lo + hi) / 2;
				if (CompareResults((ScanResult)listResults.Items[mid].Tag, result) <= 0)
					lo = mid + 1;
				else
					hi = mid;
			}
			listResults.Items.Insert(lo, item);
		}

		private static void UpdateItemText(ScanResult result)
		{
			// Index 0 (the IP address) never changes.  Assigning identical text still triggers a
			// repaint, so each cell is compared first.
			SetSubItemText(result.Item.SubItems[1], PingText(result));
			SetSubItemText(result.Item.SubItems[2], result.Hostname);
			SetSubItemText(result.Item.SubItems[3], result.MacAddress);
			SetSubItemText(result.Item.SubItems[4], LastReplyText(result));
		}

		private static void SetSubItemText(ListViewItem.ListViewSubItem subItem, string text)
		{
			if (subItem.Text != text)
				subItem.Text = text;
		}

		private static string PingText(ScanResult result)
		{
			if (result.PingRtt < 0)
				return "N/A";
			return result.PingRtt <= 0 ? "<1 ms" : result.PingRtt + " ms";
		}

		private static string LastReplyText(ScanResult result)
		{
			return result.LastReply == DateTime.MinValue ? "" : result.LastReply.ToString("HH:mm:ss");
		}

		private void UpdateStatus()
		{
			string text;
			if (running)
			{
				if (waveCompleted < waveTotal)
					text = "Wave " + waveNumber + ": pinged " + waveCompleted + " of " + waveTotal
						+ " addresses, " + results.Count + " hosts found" + dnsNote;
				else
					text = "Wave " + waveNumber + " complete, " + results.Count + " hosts found, waiting to rescan" + dnsNote;
			}
			else
				text = results.Count == 0 ? "" : "Stopped, " + results.Count + " hosts found" + dnsNote;
			if (lblStatus.Text != text)
				lblStatus.Text = text;
		}

		#endregion

		#region Sorting

		private void listResults_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			SortKey existing = sortKeys.Find(k => k.Column == e.Column);
			if ((ModifierKeys & Keys.Shift) == Keys.Shift)
			{
				// Shift+click adds the column as a further sort level, or reverses it if it is
				// already one.
				if (existing != null)
					existing.Descending = !existing.Descending;
				else
					sortKeys.Add(new SortKey { Column = e.Column });
			}
			else
			{
				// A plain click sorts by just this column: ascending first, reversed when it is
				// already the primary sort column.
				bool descending = sortKeys.Count > 0 && sortKeys[0].Column == e.Column && !sortKeys[0].Descending;
				sortKeys.Clear();
				sortKeys.Add(new SortKey { Column = e.Column, Descending = descending });
			}
			RebuildList();
			UpdateSortArrows();
		}

		private void UpdateSortArrows()
		{
			for (int i = 0; i < listResults.Columns.Count; i++)
			{
				SortKey key = sortKeys.Find(k => k.Column == i);
				ListViewSortArrows.SetSortArrow(listResults, i, key == null ? (bool?)null : key.Descending);
			}
		}

		/// <summary>
		/// Fully re-sorts the list.  Only used when the user clicks a column header; during
		/// scanning, updates go through FlushUpdates so rows do not jump around while being read.
		/// </summary>
		private void RebuildList()
		{
			uint? selectedKey = listResults.SelectedItems.Count > 0
				? ((ScanResult)listResults.SelectedItems[0].Tag).IpSortKey : (uint?)null;
			List<ScanResult> all = new List<ScanResult>(results.Values);
			all.Sort(CompareResults);
			pendingNew.Clear();
			dirtyResults.Clear();
			listResults.BeginUpdate();
			listResults.Items.Clear();
			foreach (ScanResult result in all)
			{
				if (result.Item == null)
					result.Item = CreateItem(result);
				else if (result.Dirty)
					UpdateItemText(result);
				result.Dirty = false;
				listResults.Items.Add(result.Item);
				if (selectedKey.HasValue && result.IpSortKey == selectedKey.Value)
					result.Item.Selected = true;
			}
			listResults.EndUpdate();
		}

		/// <summary>
		/// Compares two hosts by each clicked sort column in priority order, falling back to
		/// numeric IP address order when all of them compare equal.
		/// </summary>
		private int CompareResults(ScanResult a, ScanResult b)
		{
			foreach (SortKey key in sortKeys)
			{
				int c = CompareByColumn(a, b, key.Column);
				if (c != 0)
					return key.Descending ? -c : c;
			}
			return a.IpSortKey.CompareTo(b.IpSortKey);
		}

		private static int CompareByColumn(ScanResult a, ScanResult b, int column)
		{
			switch (column)
			{
				case 0: // IP Address, numerically
					return a.IpSortKey.CompareTo(b.IpSortKey);
				case 1: // Ping Time; hosts that never answered ("N/A") sort last
					long rttA = a.PingRtt < 0 ? long.MaxValue : a.PingRtt;
					long rttB = b.PingRtt < 0 ? long.MaxValue : b.PingRtt;
					return rttA.CompareTo(rttB);
				case 2: // Host Name; unnamed hosts sort last
					return CompareTextEmptyLast(a.Hostname, b.Hostname);
				case 3: // MAC Address; hosts with no known MAC sort last
					return CompareTextEmptyLast(a.MacDigits, b.MacDigits);
				case 4: // Last Reply; hosts that never replied sort last
					if ((a.LastReply == DateTime.MinValue) != (b.LastReply == DateTime.MinValue))
						return a.LastReply == DateTime.MinValue ? 1 : -1;
					return a.LastReply.CompareTo(b.LastReply);
				default:
					return 0;
			}
		}

		private static int CompareTextEmptyLast(string a, string b)
		{
			bool emptyA = string.IsNullOrEmpty(a);
			bool emptyB = string.IsNullOrEmpty(b);
			if (emptyA || emptyB)
				return emptyA == emptyB ? 0 : (emptyA ? 1 : -1);
			return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
		}

		#endregion

		#region Ping tool link

		private void btnPing_Click(object sender, EventArgs e)
		{
			if (listResults.SelectedItems.Count == 0)
			{
				MessageBox.Show(this, "Select a host in the list first.", "No host selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			PingSelected();
		}

		private void listResults_ItemActivate(object sender, EventArgs e)
		{
			PingSelected();
		}

		private void PingSelected()
		{
			if (listResults.SelectedItems.Count == 0)
				return;
			ScanResult result = (ScanResult)listResults.SelectedItems[0].Tag;
			MainForm main = FindForm() as MainForm;
			if (main == null)
				return;
			// Activating the Ping tool hides this one, which stops the scan (OnVisibleChanged).
			PingTool ping = main.ActivateTool<PingTool>();
			if (ping != null)
				ping.StartPing(result.IpText);
		}

		#endregion

		#region Address arithmetic

		private static bool TryParseCidr(string text, out uint network, out int prefix, out string error)
		{
			network = 0;
			prefix = 0;
			int slash = text.IndexOf('/');
			if (slash < 0)
			{
				error = "Enter a subnet in CIDR notation, e.g. 192.168.1.0/24.";
				return false;
			}
			IPAddress ip;
			if (!IPAddress.TryParse(text.Substring(0, slash), out ip) || ip.AddressFamily != AddressFamily.InterNetwork)
			{
				error = "\"" + text.Substring(0, slash) + "\" is not a valid IPv4 address.";
				return false;
			}
			if (!int.TryParse(text.Substring(slash + 1), out prefix) || prefix < 0 || prefix > 32)
			{
				error = "The prefix length after the \"/\" must be a number from 0 to 32.";
				return false;
			}
			network = ToUint(ip) & MaskOf(prefix);
			error = null;
			return true;
		}

		/// <summary>
		/// Lists every host address in the subnet.  The network and broadcast addresses are
		/// excluded for ordinary subnets; /31 and /32 use all their addresses.
		/// </summary>
		private static List<IPAddress> EnumerateAddresses(uint network, int prefix)
		{
			uint broadcast = network | ~MaskOf(prefix);
			List<IPAddress> list = new List<IPAddress>();
			if (prefix == 32)
				list.Add(FromUint(network));
			else if (prefix == 31)
			{
				list.Add(FromUint(network));
				list.Add(FromUint(broadcast));
			}
			else
				for (uint value = network + 1; value < broadcast; value++)
					list.Add(FromUint(value));
			return list;
		}

		private static uint MaskOf(int prefix)
		{
			// The C# shift operator masks its count to 0-31, so a shift by 32 must be special-cased.
			return prefix == 0 ? 0u : uint.MaxValue << (32 - prefix);
		}

		/// <summary>Returns the prefix length of a subnet mask, or -1 if the mask is not contiguous.</summary>
		private static int PrefixFromMask(uint mask)
		{
			int prefix = 0;
			while ((mask & 0x80000000u) != 0)
			{
				prefix++;
				mask <<= 1;
			}
			return mask == 0 ? prefix : -1;
		}

		private static uint ToUint(IPAddress address)
		{
			byte[] bytes = address.GetAddressBytes();
			return (uint)(bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]);
		}

		private static IPAddress FromUint(uint value)
		{
			return new IPAddress(new byte[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value });
		}

		#endregion
	}
}
