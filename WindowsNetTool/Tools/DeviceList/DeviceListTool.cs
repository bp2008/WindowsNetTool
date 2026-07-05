using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsNetTool.Tools.Arp;
using WindowsNetTool.Tools.Ndp;
using WindowsNetTool.Tools.Ping;
// The System.Net.NetworkInformation.Ping type must be aliased because the simple name "Ping"
// binds to the WindowsNetTool.Tools.Ping namespace.
using NetPing = System.Net.NetworkInformation.Ping;

namespace WindowsNetTool.Tools.DeviceList
{
	/// <summary>
	/// Builds one row per physical device on a network by combining the discovery methods of the
	/// IP Scanner, ARP Viewer, and NDP Viewer: every IPv4 address in the subnet is pinged in
	/// repeating waves, the system's IPv4 (ARP) and IPv6 (NDP) neighbor caches are merged in each
	/// wave, and the results are correlated by MAC address so that all of a device's IPv4 and IPv6
	/// addresses, host names, and other details land in a single row.  The compact summary list
	/// pairs with a detail pane showing everything known about the selected device, which keeps
	/// the tool usable in a small viewport.
	/// </summary>
	public partial class DeviceListTool : UserControl, IRefreshOnActivate
	{
		/// <summary>Time to wait for each echo reply; matches the IP Scanner's choice.</summary>
		private const int ScanPingTimeoutMs = 1500;

		/// <summary>Timeout for each reverse DNS query.</summary>
		private const int DnsTimeoutMs = 2000;

		/// <summary>How many reverse DNS lookups may run at once; kept low to be gentle on home routers.</summary>
		private const int MaxDnsConcurrency = 4;

		/// <summary>Pause between the end of one ping wave and the start of the next.</summary>
		private const int RescanDelayMs = 2000;

		/// <summary>Shortest allowed prefix length; /16 = 65,534 addresses is already a slow scan.</summary>
		private const int MinScanPrefix = 16;

		/// <summary>How many pings may be in flight at once during a wave.</summary>
		private static int MaxPingsInFlight => MainForm.settings.IpScanner_MaxPingsInFlight;

		/// <summary>One address belonging to a device, with its reverse DNS bookkeeping.</summary>
		private class AddressEntry
		{
			public IPAddress Address;
			public string Text;
			/// <summary>True once a reverse DNS lookup has been queued; reset on timeout so a later wave retries.</summary>
			public bool DnsAttempted;
		}

		private class Ipv4Entry : AddressEntry
		{
			/// <summary>The address as a big-endian number so addresses sort in numeric order.</summary>
			public uint Key;
		}

		private class Ipv6Entry : AddressEntry
		{
			/// <summary>The 16 address bytes, for numeric sorting.</summary>
			public byte[] SortKey;
			/// <summary>The interface index used as the scope id when pinging link-local addresses.</summary>
			public uint ScopeInterfaceIndex;
		}

		/// <summary>
		/// One physical device, correlated across the discovery sources by MAC address.  Devices
		/// whose MAC is not (yet) known are keyed by their address and merged into the MAC-keyed
		/// device as soon as a neighbor table reveals the MAC.
		/// </summary>
		private class Device
		{
			/// <summary>The key under which the device is registered: "m:" + MacDigits, or "a:" + address text before the MAC is known.</summary>
			public string Key;
			public string MacAddress = "";
			public string MacDigits = "";
			/// <summary>The device's IPv4 addresses, kept in numeric order.</summary>
			public readonly List<Ipv4Entry> Ipv4 = new List<Ipv4Entry>();
			/// <summary>The device's IPv6 addresses, kept in numeric order (which lists global addresses before link-local ones).</summary>
			public readonly List<Ipv6Entry> Ipv6 = new List<Ipv6Entry>();
			/// <summary>All host names learned from reverse DNS (a device can have one per address).</summary>
			public readonly List<string> Hostnames = new List<string>();
			public string InterfaceName = "";
			/// <summary>Latest successful round-trip time in ms, or -1 if the device has never answered a ping.</summary>
			public long PingRtt = -1;
			/// <summary>When the last successful ping reply arrived; MinValue if never.</summary>
			public DateTime LastReply = DateTime.MinValue;
			/// <summary>True for the local machine's own network adapter.</summary>
			public bool IsThisPc;
			/// <summary>True when one of the device's addresses is a configured default gateway.</summary>
			public bool IsGateway;
			/// <summary>True when the device announced itself as a router in the NDP table.</summary>
			public bool IsRouter;
			/// <summary>Set when the device was merged into another; it must no longer be displayed.</summary>
			public bool Removed;
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
		/// <summary>All known devices of the current session, keyed by <see cref="Device.Key"/>.</summary>
		private readonly Dictionary<string, Device> devices = new Dictionary<string, Device>();
		/// <summary>Maps every known address (text form, no scope id) to the device that owns it.</summary>
		private readonly Dictionary<string, Device> deviceByAddress = new Dictionary<string, Device>();
		/// <summary>Devices discovered since the last flush, awaiting insertion into the list.</summary>
		private readonly List<Device> pendingNew = new List<Device>();
		/// <summary>Devices whose displayed values changed since the last flush.</summary>
		private readonly List<Device> dirtyDevices = new List<Device>();
		/// <summary>
		/// The column sort order chosen by clicking column headers, in priority order.  Empty
		/// until the first click; numeric IPv4 address order is the default and the tie-breaker.
		/// </summary>
		private readonly List<SortKey> sortKeys = new List<SortKey>();
		/// <summary>Limits how many reverse DNS lookups run concurrently.</summary>
		private readonly SemaphoreSlim dnsGate = new SemaphoreSlim(MaxDnsConcurrency);
		/// <summary>The DNS servers used for reverse lookups, chosen when a scan starts.</summary>
		private ReverseDns reverseDns = new ReverseDns();
		/// <summary>Appended to the status line when reverse DNS had to be disabled for this scan.</summary>
		private string dnsNote = "";
		/// <summary>The IPv6 interface indexes of the adapters attached to the scanned subnet; NDP entries elsewhere are ignored.</summary>
		private readonly HashSet<uint> ndpInterfaceIndexes = new HashSet<uint>();
		/// <summary>The default gateway addresses (text form, no scope id) of the adapters attached to the scanned subnet.</summary>
		private readonly HashSet<string> gatewayAddresses = new HashSet<string>();
		private int waveNumber, waveTotal, waveCompleted;

		public DeviceListTool()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignMode)
			{
				PopulateSubnetList();
				UpdateDetails();
			}
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
						int prefix = Ipv4Util.PrefixFromMask(Ipv4Util.ToUint(ua.IPv4Mask));
						// /31 and /32 (point-to-point and VPN addresses) are pointless to scan.
						if (prefix < 8 || prefix > 30)
							continue;
						string cidr = Ipv4Util.FromUint(Ipv4Util.ToUint(ua.Address) & Ipv4Util.MaskOf(prefix)).ToString() + "/" + prefix;
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
		/// The scan loop.  Each iteration merges the ARP and NDP tables into the device list,
		/// pings every IPv4 address in the subnet plus every known IPv6 address, queues reverse
		/// DNS work, then pauses before rescanning.  The loop exits when the session counter
		/// changes (Stop pressed or the tool deactivated).
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
			if (!Ipv4Util.TryParseCidr(text, out network, out prefix, out error))
			{
				MessageBox.Show(this, error, "Device List", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			if (prefix < MinScanPrefix)
			{
				MessageBox.Show(this, "Subnets larger than /" + MinScanPrefix + " are not supported.  A /" + prefix
					+ " subnet contains " + ((1L << (32 - prefix)) - 2).ToString("N0") + " addresses.",
					"Device List", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			session++;
			int mySession = session;
			running = true;
			scanNetwork = network;
			scanPrefix = prefix;
			devices.Clear();
			deviceByAddress.Clear();
			pendingNew.Clear();
			dirtyDevices.Clear();
			listDevices.Items.Clear();
			UpdateDetails();
			reverseDns = ReverseDns.ChooseServers();
			dnsNote = reverseDns.Local == null && Ipv4Util.IsPrivateIPv4(network) ? "   (reverse DNS unavailable: no local DNS server)" : "";
			waveNumber = 0;
			waveCompleted = 0;
			btnStartStop.Text = "Stop";
			comboSubnet.Enabled = false;
			timerFlush.Start();

			CollectScanInterfaces();

			List<IPAddress> ipv4Addresses = Ipv4Util.EnumerateAddresses(network, prefix);
			while (session == mySession && !IsDisposed)
			{
				waveNumber++;
				waveCompleted = 0;
				await MergeNeighborTablesAsync(mySession);
				if (session != mySession)
					break;
				List<IPAddress> targets = new List<IPAddress>(ipv4Addresses);
				CollectIpv6PingTargets(targets);
				waveTotal = targets.Count;
				await RunPingWave(targets, mySession);
				if (session != mySession)
					break;
				// Queue reverse lookups for addresses found via the neighbor tables, and retry
				// ones that timed out.
				foreach (Device device in devices.Values)
				{
					foreach (Ipv4Entry entry in device.Ipv4)
						if (!entry.DnsAttempted)
							BeginHostnameLookup(device, entry, mySession);
					foreach (Ipv6Entry entry in device.Ipv6)
						if (!entry.DnsAttempted)
							BeginHostnameLookup(device, entry, mySession);
				}
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
		/// Identifies the network adapters attached to the scanned subnet.  Their IPv6 interface
		/// indexes scope the NDP merge (an IPv6 neighbor on an unrelated adapter is not part of
		/// this network), their default gateways let the gateway device be labeled, and each
		/// adapter itself becomes a device row so the local machine appears in the list.
		/// </summary>
		private void CollectScanInterfaces()
		{
			ndpInterfaceIndexes.Clear();
			gatewayAddresses.Clear();
			uint mask = Ipv4Util.MaskOf(scanPrefix);
			try
			{
				foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
				{
					if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
						continue;
					IPInterfaceProperties props = nic.GetIPProperties();
					IPAddress addressInSubnet = null;
					foreach (UnicastIPAddressInformation ua in props.UnicastAddresses)
					{
						if (ua.Address.AddressFamily == AddressFamily.InterNetwork
							&& (Ipv4Util.ToUint(ua.Address) & mask) == scanNetwork)
						{
							addressInSubnet = ua.Address;
							break;
						}
					}
					if (addressInSubnet == null)
						continue;

					uint ipv6Index = 0;
					try
					{
						IPv6InterfaceProperties v6 = props.GetIPv6Properties();
						if (v6 != null)
						{
							ipv6Index = (uint)v6.Index;
							ndpInterfaceIndexes.Add(ipv6Index);
						}
					}
					catch (NetworkInformationException)
					{
						// The adapter has no IPv6 binding; there are no NDP entries to merge from it.
					}

					foreach (GatewayIPAddressInformation gw in props.GatewayAddresses)
						gatewayAddresses.Add(TextWithoutScope(gw.Address));

					AddLocalDevice(nic, addressInSubnet, ipv6Index);
				}
			}
			catch (NetworkInformationException)
			{
				// Interface enumeration failed; the scan proceeds on pings and neighbor tables alone.
			}
		}

		/// <summary>Adds the local machine's own adapter as a device, with all of its addresses.</summary>
		private void AddLocalDevice(NetworkInterface nic, IPAddress addressInSubnet, uint ipv6Index)
		{
			byte[] macBytes = nic.GetPhysicalAddress().GetAddressBytes();
			Device device = GetDevice(addressInSubnet.ToString(), FormatMac(macBytes, ""), FormatMac(macBytes, "-"));
			device.IsThisPc = true;
			if (device.InterfaceName.Length == 0)
				device.InterfaceName = nic.Name;
			AddHostname(device, Environment.MachineName);
			foreach (UnicastIPAddressInformation ua in nic.GetIPProperties().UnicastAddresses)
			{
				if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
					AddIpv4(device, ua.Address);
				else if (ua.Address.AddressFamily == AddressFamily.InterNetworkV6)
					AddIpv6(device, ua.Address, ipv6Index);
			}
			MarkDirty(device);
		}

		#region Device correlation

		/// <summary>
		/// Returns the device that owns the given address, creating or merging devices as needed.
		/// A device is keyed by its MAC address once one is known; an address-keyed device is
		/// promoted (or absorbed into the MAC's device) the moment a neighbor table associates its
		/// address with a MAC.
		/// </summary>
		private Device GetDevice(string ipText, string macDigits, string macAddress)
		{
			Device byAddress;
			deviceByAddress.TryGetValue(ipText, out byAddress);
			if (macDigits.Length == 0)
				return byAddress ?? CreateDevice("a:" + ipText);

			Device byMac;
			devices.TryGetValue("m:" + macDigits, out byMac);
			if (byMac == null && byAddress != null && byAddress.MacDigits.Length == 0)
			{
				// The MAC of a ping-discovered device is now known; promote it to a MAC key.
				devices.Remove(byAddress.Key);
				byAddress.Key = "m:" + macDigits;
				devices.Add(byAddress.Key, byAddress);
				byAddress.MacDigits = macDigits;
				byAddress.MacAddress = macAddress;
				MarkDirty(byAddress);
				return byAddress;
			}
			if (byMac == null)
			{
				byMac = CreateDevice("m:" + macDigits);
				byMac.MacDigits = macDigits;
				byMac.MacAddress = macAddress;
			}
			// The address was previously attributed to another device.  If that device has no MAC
			// of its own it was just an earlier sighting of this one, so merge it; if it has a
			// different MAC, the address has moved (e.g. DHCP reassignment) and only the address
			// changes hands.
			if (byAddress != null && byAddress != byMac)
			{
				if (byAddress.MacDigits.Length == 0)
					Absorb(byMac, byAddress);
				else
					RemoveAddress(byAddress, ipText);
			}
			return byMac;
		}

		private Device CreateDevice(string key)
		{
			Device device = new Device { Key = key };
			devices.Add(key, device);
			pendingNew.Add(device);
			return device;
		}

		/// <summary>Merges a MAC-less device into the device that turned out to own its addresses.</summary>
		private void Absorb(Device target, Device victim)
		{
			foreach (Ipv4Entry entry in victim.Ipv4)
			{
				deviceByAddress[entry.Text] = target;
				InsertIpv4(target, entry);
			}
			foreach (Ipv6Entry entry in victim.Ipv6)
			{
				deviceByAddress[entry.Text] = target;
				InsertIpv6(target, entry);
			}
			foreach (string name in victim.Hostnames)
				AddHostname(target, name);
			if (target.InterfaceName.Length == 0)
				target.InterfaceName = victim.InterfaceName;
			if (victim.LastReply > target.LastReply)
			{
				target.LastReply = victim.LastReply;
				target.PingRtt = victim.PingRtt;
			}
			target.IsThisPc |= victim.IsThisPc;
			target.IsGateway |= victim.IsGateway;
			target.IsRouter |= victim.IsRouter;

			devices.Remove(victim.Key);
			victim.Removed = true;
			pendingNew.Remove(victim);
			if (victim.Item != null && victim.Item.ListView != null)
				victim.Item.Remove();
			MarkDirty(target);
		}

		private void RemoveAddress(Device device, string ipText)
		{
			for (int i = 0; i < device.Ipv4.Count; i++)
				if (device.Ipv4[i].Text == ipText)
				{
					device.Ipv4.RemoveAt(i);
					MarkDirty(device);
					return;
				}
			for (int i = 0; i < device.Ipv6.Count; i++)
				if (device.Ipv6[i].Text == ipText)
				{
					device.Ipv6.RemoveAt(i);
					MarkDirty(device);
					return;
				}
		}

		private void AddIpv4(Device device, IPAddress address)
		{
			string text = address.ToString();
			Device previous;
			if (deviceByAddress.TryGetValue(text, out previous) && previous != device)
				RemoveAddress(previous, text);
			deviceByAddress[text] = device;
			InsertIpv4(device, new Ipv4Entry { Address = address, Text = text, Key = Ipv4Util.ToUint(address) });
			if (gatewayAddresses.Contains(text) && !device.IsGateway)
			{
				device.IsGateway = true;
				MarkDirty(device);
			}
		}

		private void AddIpv6(Device device, IPAddress address, uint scopeInterfaceIndex)
		{
			// Stored and displayed without a scope id; the scope (interface index) is kept
			// separately for pinging link-local addresses.
			byte[] bytes = address.GetAddressBytes();
			IPAddress plain = address.ScopeId != 0 ? new IPAddress(bytes) : address;
			string text = plain.ToString();
			Device previous;
			if (deviceByAddress.TryGetValue(text, out previous) && previous != device)
				RemoveAddress(previous, text);
			deviceByAddress[text] = device;
			InsertIpv6(device, new Ipv6Entry
			{
				Address = plain,
				Text = text,
				SortKey = bytes,
				ScopeInterfaceIndex = scopeInterfaceIndex,
				// Link-local addresses are never registered in DNS; skip their lookups.
				DnsAttempted = plain.IsIPv6LinkLocal
			});
			if (gatewayAddresses.Contains(text) && !device.IsGateway)
			{
				device.IsGateway = true;
				MarkDirty(device);
			}
		}

		private void InsertIpv4(Device device, Ipv4Entry entry)
		{
			for (int i = 0; i < device.Ipv4.Count; i++)
			{
				if (device.Ipv4[i].Key == entry.Key)
					return;
				if (device.Ipv4[i].Key > entry.Key)
				{
					device.Ipv4.Insert(i, entry);
					MarkDirty(device);
					return;
				}
			}
			device.Ipv4.Add(entry);
			MarkDirty(device);
		}

		private void InsertIpv6(Device device, Ipv6Entry entry)
		{
			for (int i = 0; i < device.Ipv6.Count; i++)
			{
				int c = NdpEntry.CompareSortKeys(device.Ipv6[i].SortKey, entry.SortKey);
				if (c == 0)
					return;
				if (c > 0)
				{
					device.Ipv6.Insert(i, entry);
					MarkDirty(device);
					return;
				}
			}
			device.Ipv6.Add(entry);
			MarkDirty(device);
		}

		private void AddHostname(Device device, string name)
		{
			if (name == null || name.Length == 0)
				return;
			foreach (string existing in device.Hostnames)
				if (string.Compare(existing, name, StringComparison.OrdinalIgnoreCase) == 0)
					return;
			device.Hostnames.Add(name);
			MarkDirty(device);
		}

		/// <summary>
		/// True for MAC addresses that identify a real device.  Group addresses (the multicast
		/// bit is set, which covers IPv4 multicast 01-00-5e, IPv6 multicast 33-33, and broadcast
		/// ff-ff-ff-ff-ff-ff) do not.
		/// </summary>
		private static bool IsRealDeviceMac(string macDigits)
		{
			if (macDigits.Length < 2)
				return false;
			int firstByte = Convert.ToInt32(macDigits.Substring(0, 2), 16);
			return (firstByte & 1) == 0;
		}

		private static string FormatMac(byte[] mac, string separator)
		{
			StringBuilder sb = new StringBuilder(mac.Length * 3);
			for (int i = 0; i < mac.Length; i++)
			{
				if (i > 0)
					sb.Append(separator);
				sb.Append(mac[i].ToString("x2"));
			}
			return sb.ToString();
		}

		#endregion

		#region Discovery: neighbor tables, pings, reverse DNS

		private async Task MergeNeighborTablesAsync(int mySession)
		{
			List<ArpEntry> arpEntries = null;
			List<NdpEntry> ndpEntries = null;
			try
			{
				arpEntries = await Task.Run(() => IpHelperArp.GetArpEntries());
			}
			catch
			{
				// A failed ARP read should not stop the scan; ping results still flow.
			}
			if (session != mySession || IsDisposed)
				return;
			try
			{
				ndpEntries = await Task.Run(() => IpHelperNdp.GetNdpEntries());
			}
			catch
			{
			}
			if (session != mySession || IsDisposed)
				return;
			if (arpEntries != null)
				MergeArpEntries(arpEntries);
			if (ndpEntries != null)
				MergeNdpEntries(ndpEntries);
		}

		/// <summary>
		/// Merges the IPv4 neighbor table: MAC addresses for devices already found by ping
		/// (pinging them created their ARP entries), plus devices that never answer pings but
		/// appear in the table because of other traffic.  Only entries within the scanned subnet
		/// are considered.
		/// </summary>
		private void MergeArpEntries(List<ArpEntry> entries)
		{
			uint mask = Ipv4Util.MaskOf(scanPrefix);
			uint broadcast = scanNetwork | ~mask;
			foreach (ArpEntry entry in entries)
			{
				if ((entry.IpSortKey & mask) != scanNetwork)
					continue;
				if (scanPrefix <= 30 && (entry.IpSortKey == scanNetwork || entry.IpSortKey == broadcast))
					continue;
				if (entry.MacDigits.Length > 0 && !IsRealDeviceMac(entry.MacDigits))
					continue;
				Device device = GetDevice(entry.IpText, entry.MacDigits, entry.MacAddress);
				AddIpv4(device, entry.IpAddress);
				if (device.InterfaceName.Length == 0)
					device.InterfaceName = entry.InterfaceName;
			}
		}

		/// <summary>
		/// Merges the IPv6 neighbor table, which is what ties devices' IPv6 addresses to the same
		/// row as their IPv4 addresses (via the shared MAC).  Only entries on the adapters attached
		/// to the scanned subnet are considered, and only entries with a usable MAC address:
		/// multicast entries and MAC-less entries cannot be correlated to a device.
		/// </summary>
		private void MergeNdpEntries(List<NdpEntry> entries)
		{
			foreach (NdpEntry entry in entries)
			{
				if (!ndpInterfaceIndexes.Contains(entry.InterfaceIndex))
					continue;
				if (entry.IpSortKey[0] == 0xFF) // Multicast destination, not a device.
					continue;
				if (entry.MacDigits.Length == 0 || !IsRealDeviceMac(entry.MacDigits))
					continue;
				Device device = GetDevice(entry.IpText, entry.MacDigits, entry.MacAddress);
				AddIpv6(device, entry.IpAddress, entry.InterfaceIndex);
				if (entry.IsRouter && !device.IsRouter)
				{
					device.IsRouter = true;
					MarkDirty(device);
				}
				if (device.InterfaceName.Length == 0)
					device.InterfaceName = entry.InterfaceName;
			}
		}

		/// <summary>
		/// Adds the known IPv6 addresses of discovered devices to the wave's ping targets, so
		/// IPv6 reachability and round-trip times stay current and the NDP entries stay fresh.
		/// Link-local targets carry their interface index as the scope id, without which they
		/// are unroutable.
		/// </summary>
		private void CollectIpv6PingTargets(List<IPAddress> targets)
		{
			foreach (Device device in devices.Values)
			{
				if (device.IsThisPc)
					continue;
				foreach (Ipv6Entry entry in device.Ipv6)
					targets.Add(entry.Address.IsIPv6LinkLocal && entry.ScopeInterfaceIndex != 0
						? new IPAddress(entry.SortKey, entry.ScopeInterfaceIndex)
						: entry.Address);
			}
		}

		/// <summary>
		/// Pings every target once, keeping at most <see cref="MaxPingsInFlight"/> pings in
		/// flight.  The semaphore self-paces the sends: each completed ping (reply or timeout)
		/// releases a slot for the next address.
		/// </summary>
		private async Task RunPingWave(List<IPAddress> targets, int mySession)
		{
			SemaphoreSlim gate = new SemaphoreSlim(MaxPingsInFlight, MaxPingsInFlight);
			List<Task> tasks = new List<Task>(targets.Count);
			foreach (IPAddress address in targets)
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
					PingReply reply = await ping.SendPingAsync(address, ScanPingTimeoutMs, PingUtil.PingPayload);
					if (session == mySession && !IsDisposed && reply.Status == IPStatus.Success)
						RecordPingReply(address, reply.RoundtripTime);
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

		private void RecordPingReply(IPAddress address, long rtt)
		{
			string text = TextWithoutScope(address);
			Device device;
			if (!deviceByAddress.TryGetValue(text, out device))
			{
				// Unknown IPv6 targets cannot appear (they were collected from known devices),
				// so this is a newly discovered IPv4 host.
				if (address.AddressFamily != AddressFamily.InterNetwork)
					return;
				device = GetDevice(text, "", "");
				AddIpv4(device, address);
			}
			device.PingRtt = rtt;
			device.LastReply = DateTime.Now;
			MarkDirty(device);
		}

		/// <summary>
		/// Resolves one address to a host name via reverse DNS.  Private addresses are only ever
		/// queried against a local DNS server; if the system has none, the lookup is skipped
		/// entirely so private addresses never leak to public resolvers.
		/// </summary>
		private async void BeginHostnameLookup(Device device, AddressEntry entry, int mySession)
		{
			entry.DnsAttempted = true;
			IPAddress server = reverseDns.ServerFor(entry.Address);
			if (server == null)
				return;
			await dnsGate.WaitAsync();
			try
			{
				if (session != mySession || IsDisposed)
					return;
				IPAddress target = entry.Address;
				string host = await Task.Run(() => ReverseDns.TryReverseLookup(server, target, DnsTimeoutMs));
				if (session != mySession || IsDisposed)
					return;
				if (host == null)
				{
					// The DNS server did not respond; allow the next wave to retry.
					entry.DnsAttempted = false;
					return;
				}
				if (host.Length > 0)
				{
					// The device may have been merged into another while the lookup was in
					// flight; the name belongs to whichever device owns the address now.
					Device owner = device;
					if (owner.Removed && !deviceByAddress.TryGetValue(entry.Text, out owner))
						return;
					AddHostname(owner, host);
				}
			}
			finally
			{
				dnsGate.Release();
			}
		}

		private static string TextWithoutScope(IPAddress address)
		{
			if (address.AddressFamily == AddressFamily.InterNetworkV6 && address.ScopeId != 0)
				return new IPAddress(address.GetAddressBytes()).ToString();
			return address.ToString();
		}

		#endregion

		#region List updating

		private void MarkDirty(Device device)
		{
			if (device.Dirty || device.Removed)
				return;
			device.Dirty = true;
			dirtyDevices.Add(device);
		}

		private void timerFlush_Tick(object sender, EventArgs e)
		{
			FlushUpdates();
		}

		/// <summary>
		/// Applies accumulated changes to the ListView.  Batching on a timer (rather than updating
		/// per event) and rewriting only cells whose text actually changed keeps the list
		/// flicker-free and cheap even when hundreds of replies arrive per second.  Rows keep
		/// their positions while values change; new rows are inserted at their sorted position.
		/// A device whose data changed can also enter or leave the view when a filter is active.
		/// </summary>
		private void FlushUpdates()
		{
			Device selected = SelectedDevice();
			bool selectedChanged = false;
			if (pendingNew.Count > 0)
			{
				listDevices.BeginUpdate();
				foreach (Device device in pendingNew)
				{
					device.Item = CreateItem(device);
					if (PassesFilter(device))
						InsertSorted(device.Item);
				}
				pendingNew.Clear();
				listDevices.EndUpdate();
			}
			if (dirtyDevices.Count > 0)
			{
				foreach (Device device in dirtyDevices)
				{
					device.Dirty = false;
					if (device.Removed || device.Item == null)
						continue;
					UpdateItemText(device);
					bool shown = device.Item.ListView != null;
					bool passes = PassesFilter(device);
					if (shown && !passes)
						device.Item.Remove();
					else if (!shown && passes)
						InsertSorted(device.Item);
					if (device == selected)
						selectedChanged = true;
				}
				dirtyDevices.Clear();
			}
			if (selectedChanged || (selected != null && selected.Removed))
				UpdateDetails();
			UpdateStatus();
		}

		private static ListViewItem CreateItem(Device device)
		{
			ListViewItem item = new ListViewItem(new string[]
			{
				NameText(device),
				SummarizeIpv4(device),
				SummarizeIpv6(device),
				device.MacAddress,
				PingText(device),
				LastReplyText(device),
				InfoText(device)
			});
			item.Tag = device;
			return item;
		}

		private static void UpdateItemText(Device device)
		{
			// Assigning identical text still triggers a repaint, so each cell is compared first.
			SetSubItemText(device.Item.SubItems[0], NameText(device));
			SetSubItemText(device.Item.SubItems[1], SummarizeIpv4(device));
			SetSubItemText(device.Item.SubItems[2], SummarizeIpv6(device));
			SetSubItemText(device.Item.SubItems[3], device.MacAddress);
			SetSubItemText(device.Item.SubItems[4], PingText(device));
			SetSubItemText(device.Item.SubItems[5], LastReplyText(device));
			SetSubItemText(device.Item.SubItems[6], InfoText(device));
		}

		private static void SetSubItemText(ListViewItem.ListViewSubItem subItem, string text)
		{
			if (subItem.Text != text)
				subItem.Text = text;
		}

		private void InsertSorted(ListViewItem item)
		{
			Device device = (Device)item.Tag;
			int lo = 0, hi = listDevices.Items.Count;
			while (lo < hi)
			{
				int mid = (lo + hi) / 2;
				if (CompareDevices((Device)listDevices.Items[mid].Tag, device) <= 0)
					lo = mid + 1;
				else
					hi = mid;
			}
			listDevices.Items.Insert(lo, item);
		}

		private static string NameText(Device device)
		{
			return string.Join(", ", device.Hostnames);
		}

		/// <summary>Shows the first (lowest) address, with a count when the device has more.</summary>
		private static string SummarizeIpv4(Device device)
		{
			if (device.Ipv4.Count == 0)
				return "";
			return device.Ipv4.Count == 1 ? device.Ipv4[0].Text : device.Ipv4[0].Text + " (+" + (device.Ipv4.Count - 1) + ")";
		}

		/// <summary>
		/// Shows the first IPv6 address, with a count when the device has more.  Numeric order
		/// lists global addresses (2000::/3) before link-local ones (fe80::/10), so the most
		/// broadly meaningful address is the one shown.
		/// </summary>
		private static string SummarizeIpv6(Device device)
		{
			if (device.Ipv6.Count == 0)
				return "";
			return device.Ipv6.Count == 1 ? device.Ipv6[0].Text : device.Ipv6[0].Text + " (+" + (device.Ipv6.Count - 1) + ")";
		}

		private static string PingText(Device device)
		{
			if (device.PingRtt < 0)
				return "N/A";
			return device.PingRtt <= 0 ? "<1 ms" : device.PingRtt + " ms";
		}

		private static string LastReplyText(Device device)
		{
			return device.LastReply == DateTime.MinValue ? "" : device.LastReply.ToString("HH:mm:ss");
		}

		private static string InfoText(Device device)
		{
			StringBuilder sb = new StringBuilder();
			if (device.IsThisPc)
				sb.Append("This PC");
			if (device.IsGateway)
				sb.Append(sb.Length > 0 ? ", " : "").Append("Gateway");
			else if (device.IsRouter)
				sb.Append(sb.Length > 0 ? ", " : "").Append("Router");
			return sb.ToString();
		}

		private void UpdateStatus()
		{
			string text;
			if (running)
			{
				if (waveCompleted < waveTotal)
					text = "Wave " + waveNumber + ": pinged " + waveCompleted + " of " + waveTotal
						+ " addresses, " + devices.Count + " devices found" + dnsNote;
				else
					text = "Wave " + waveNumber + " complete, " + devices.Count + " devices found, waiting to rescan" + dnsNote;
			}
			else
				text = devices.Count == 0 ? "" : "Stopped, " + devices.Count + " devices found" + dnsNote;
			if (lblStatus.Text != text)
				lblStatus.Text = text;
		}

		#endregion

		#region Details pane

		private Device SelectedDevice()
		{
			return listDevices.SelectedItems.Count > 0 ? (Device)listDevices.SelectedItems[0].Tag : null;
		}

		private void listDevices_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateDetails();
		}

		/// <summary>
		/// Shows everything known about the selected device in the detail pane.  The pane exists
		/// because a device can have many addresses and names; the list keeps one compact row per
		/// device and the full data lives here, selectable for copying.
		/// </summary>
		private void UpdateDetails()
		{
			Device device = SelectedDevice();
			string text;
			if (device == null)
				text = "Select a device to see all of its known addresses and details.";
			else
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("Name:       ").Append(device.Hostnames.Count == 0 ? "(unknown)" : NameText(device)).AppendLine();
				sb.Append("MAC:        ").Append(device.MacAddress.Length == 0 ? "(unknown)" : device.MacAddress).AppendLine();
				if (device.InterfaceName.Length > 0)
					sb.Append("Interface:  ").Append(device.InterfaceName).AppendLine();
				string info = InfoText(device);
				if (info.Length > 0)
					sb.Append("Info:       ").Append(info).AppendLine();
				sb.Append("Ping:       ").Append(device.LastReply == DateTime.MinValue
					? "no reply yet"
					: PingText(device) + "  (last reply " + LastReplyText(device) + ")").AppendLine();
				sb.Append("IPv4:       ");
				if (device.Ipv4.Count == 0)
					sb.Append("(none)").AppendLine();
				else
					for (int i = 0; i < device.Ipv4.Count; i++)
						sb.Append(i == 0 ? "" : "            ").Append(device.Ipv4[i].Text).AppendLine();
				sb.Append("IPv6:       ");
				if (device.Ipv6.Count == 0)
					sb.Append("(none)").AppendLine();
				else
					for (int i = 0; i < device.Ipv6.Count; i++)
						sb.Append(i == 0 ? "" : "            ").Append(device.Ipv6[i].Text)
							.Append(DescribeIpv6Kind(device.Ipv6[i].Address)).AppendLine();
				text = sb.ToString();
			}
			if (txtDetails.Text != text)
				txtDetails.Text = text;
		}

		private static string DescribeIpv6Kind(IPAddress address)
		{
			if (address.IsIPv6LinkLocal)
				return "  (link-local)";
			if (ReverseDns.IsIPv6UniqueLocal(address))
				return "  (unique local)";
			return "";
		}

		#endregion

		#region Filtering and sorting

		/// <summary>
		/// True when the device matches the filter box.  The filter matches anywhere within any
		/// host name, any address, the info labels, or the interface name; it also matches the MAC
		/// address ignoring separators, so "aabb", "aa-bb", and "aa:bb" are equivalent.
		/// </summary>
		private bool PassesFilter(Device device)
		{
			string filter = txtFilter.Text.Trim();
			if (filter.Length == 0)
				return true;
			foreach (string name in device.Hostnames)
				if (name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
					return true;
			foreach (Ipv4Entry entry in device.Ipv4)
				if (entry.Text.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
					return true;
			foreach (Ipv6Entry entry in device.Ipv6)
				if (entry.Text.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
					return true;
			string macFilter = NormalizeMacFilter(filter);
			if (macFilter.Length > 0 && device.MacDigits.IndexOf(macFilter, StringComparison.Ordinal) >= 0)
				return true;
			if (InfoText(device).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
				return true;
			if (device.InterfaceName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
				return true;
			return false;
		}

		/// <summary>
		/// Reduces a MAC filter string to bare lowercase digits so it can be matched against
		/// <see cref="Device.MacDigits"/> regardless of which separator style the user typed.
		/// </summary>
		private static string NormalizeMacFilter(string text)
		{
			StringBuilder sb = new StringBuilder(text.Length);
			foreach (char c in text)
				if (c != ':' && c != '-' && c != '.' && !char.IsWhiteSpace(c))
					sb.Append(char.ToLowerInvariant(c));
			return sb.ToString();
		}

		private void txtFilter_TextChanged(object sender, EventArgs e)
		{
			RebuildList();
		}

		private void listDevices_ColumnClick(object sender, ColumnClickEventArgs e)
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
			for (int i = 0; i < listDevices.Columns.Count; i++)
			{
				SortKey key = sortKeys.Find(k => k.Column == i);
				ListViewSortArrows.SetSortArrow(listDevices, i, key == null ? (bool?)null : key.Descending);
			}
		}

		/// <summary>
		/// Fully rebuilds the list from all known devices, applying the filter and sort.  Used
		/// when the filter or sort changes; during scanning, updates go through FlushUpdates so
		/// rows do not jump around while being read.
		/// </summary>
		private void RebuildList()
		{
			Device selected = SelectedDevice();
			List<Device> shown = new List<Device>(devices.Count);
			foreach (Device device in devices.Values)
			{
				if (device.Item == null)
					device.Item = CreateItem(device);
				else if (device.Dirty)
					UpdateItemText(device);
				device.Dirty = false;
				if (PassesFilter(device))
					shown.Add(device);
			}
			pendingNew.Clear();
			dirtyDevices.Clear();
			shown.Sort(CompareDevices);
			listDevices.BeginUpdate();
			listDevices.Items.Clear();
			foreach (Device device in shown)
			{
				listDevices.Items.Add(device.Item);
				if (device == selected)
					device.Item.Selected = true;
			}
			listDevices.EndUpdate();
			UpdateStatus();
		}

		/// <summary>
		/// Compares two devices by each clicked sort column in priority order, falling back to
		/// the natural order (numeric order of the lowest IPv4 address, then IPv6, then MAC;
		/// devices lacking the compared value sort last).
		/// </summary>
		private int CompareDevices(Device a, Device b)
		{
			foreach (SortKey key in sortKeys)
			{
				int c = CompareByColumn(a, b, key.Column);
				if (c != 0)
					return key.Descending ? -c : c;
			}
			int n = CompareFirstIpv4(a, b);
			if (n != 0)
				return n;
			n = CompareFirstIpv6(a, b);
			return n != 0 ? n : string.Compare(a.MacDigits, b.MacDigits, StringComparison.Ordinal);
		}

		private static int CompareByColumn(Device a, Device b, int column)
		{
			switch (column)
			{
				case 0: // Name; unnamed devices sort last
					return CompareTextEmptyLast(NameText(a), NameText(b));
				case 1: // IPv4, numerically; devices without one sort last
					return CompareFirstIpv4(a, b);
				case 2: // IPv6, numerically; devices without one sort last
					return CompareFirstIpv6(a, b);
				case 3: // MAC Address; devices with no known MAC sort last
					return CompareTextEmptyLast(a.MacDigits, b.MacDigits);
				case 4: // Ping Time; devices that never answered ("N/A") sort last
					long rttA = a.PingRtt < 0 ? long.MaxValue : a.PingRtt;
					long rttB = b.PingRtt < 0 ? long.MaxValue : b.PingRtt;
					return rttA.CompareTo(rttB);
				case 5: // Last Reply; devices that never replied sort last
					if ((a.LastReply == DateTime.MinValue) != (b.LastReply == DateTime.MinValue))
						return a.LastReply == DateTime.MinValue ? 1 : -1;
					return a.LastReply.CompareTo(b.LastReply);
				case 6: // Info; unlabeled devices sort last
					return CompareTextEmptyLast(InfoText(a), InfoText(b));
				default:
					return 0;
			}
		}

		private static int CompareFirstIpv4(Device a, Device b)
		{
			bool emptyA = a.Ipv4.Count == 0;
			bool emptyB = b.Ipv4.Count == 0;
			if (emptyA || emptyB)
				return emptyA == emptyB ? 0 : (emptyA ? 1 : -1);
			return a.Ipv4[0].Key.CompareTo(b.Ipv4[0].Key);
		}

		private static int CompareFirstIpv6(Device a, Device b)
		{
			bool emptyA = a.Ipv6.Count == 0;
			bool emptyB = b.Ipv6.Count == 0;
			if (emptyA || emptyB)
				return emptyA == emptyB ? 0 : (emptyA ? 1 : -1);
			return NdpEntry.CompareSortKeys(a.Ipv6[0].SortKey, b.Ipv6[0].SortKey);
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
			if (listDevices.SelectedItems.Count == 0)
			{
				MessageBox.Show(this, "Select a device in the list first.", "No device selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			PingSelected();
		}

		private void listDevices_ItemActivate(object sender, EventArgs e)
		{
			PingSelected();
		}

		private void PingSelected()
		{
			Device device = SelectedDevice();
			if (device == null)
				return;
			string target = PreferredPingTarget(device);
			if (target == null)
				return;
			MainForm main = FindForm() as MainForm;
			if (main == null)
				return;
			// Activating the Ping tool hides this one, which stops the scan (OnVisibleChanged).
			PingTool ping = main.ActivateTool<PingTool>();
			if (ping != null)
				ping.StartPing(target);
		}

		/// <summary>
		/// The address to hand to the Ping tool: IPv4 first, then a routable IPv6 address, then a
		/// link-local IPv6 address with its required scope id.
		/// </summary>
		private static string PreferredPingTarget(Device device)
		{
			if (device.Ipv4.Count > 0)
				return device.Ipv4[0].Text;
			foreach (Ipv6Entry entry in device.Ipv6)
				if (!entry.Address.IsIPv6LinkLocal)
					return entry.Text;
			foreach (Ipv6Entry entry in device.Ipv6)
				if (entry.ScopeInterfaceIndex != 0)
					return entry.Text + "%" + entry.ScopeInterfaceIndex;
			return device.Ipv6.Count > 0 ? device.Ipv6[0].Text : null;
		}

		#endregion
	}
}
