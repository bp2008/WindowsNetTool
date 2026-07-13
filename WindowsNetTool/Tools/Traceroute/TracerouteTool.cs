using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsNetTool.Tools.Ping;
using WindowsNetTool.Tools.Export;
// The System.Net.NetworkInformation.Ping type must be aliased because the simple name "Ping"
// binds to the WindowsNetTool.Tools.Ping namespace.
using NetPing = System.Net.NetworkInformation.Ping;

namespace WindowsNetTool.Tools.Traceroute
{
	/// <summary>
	/// An asynchronous/concurrent traceroute.  Instead of probing one hop at a time and waiting
	/// for each reply like classic traceroute tools, every TTL from 1 to MaxHops is probed
	/// simultaneously, so the whole route is usually discovered in about one round-trip time.
	/// To ride out packet loss, each hop is probed up to ProbeAttempts times at ProbeIntervalMs
	/// intervals; a hop that has answered is not probed again, and the trace ends as soon as
	/// every hop up to the destination has answered.  Host names are filled in with concurrent
	/// reverse DNS lookups as hops are discovered.  Everything runs as async continuations on
	/// the UI thread; no background threads are created.
	/// </summary>
	public partial class TracerouteTool : UserControl, IExportableTool
	{
		/// <summary>Highest TTL probed, matching tracert's default maximum.</summary>
		private const int MaxHops = 32;

		/// <summary>How many times each unanswered hop is probed before it is declared unresponsive.</summary>
		private const int ProbeAttempts = 15;

		/// <summary>
		/// Delay between probe rounds; ProbeAttempts rounds at this interval spread each hop's
		/// probes evenly across 3 seconds.
		/// </summary>
		private const int ProbeIntervalMs = 200;

		/// <summary>Time to wait for each echo reply.</summary>
		private const int ProbeTimeoutMs = 3000;

		/// <summary>One row of the route: the router (or destination) that answers probes sent with a particular TTL.</summary>
		private class Hop
		{
			public int Ttl;
			public IPAddress Address;
			/// <summary>Round-trip time in ms of the first reply, or -1 while unanswered.</summary>
			public long Rtt = -1;
			public IPStatus Status;
			public string Hostname = "";
			/// <summary>True once a reply has been recorded (or the hop is beyond the destination); the hop is not probed again.</summary>
			public bool Answered;
			/// <summary>The hop's row, or null once the hop is dropped for being beyond the destination.</summary>
			public ListViewItem Item;
		}

		private bool running = false;
		/// <summary>Incremented whenever tracing starts or stops so callbacks from an earlier session are ignored.</summary>
		private int session = 0;
		private IPAddress targetAddress;
		/// <summary>"host [address]" text used in status messages.</summary>
		private string targetText;
		/// <summary>The hops of the current trace, indexed by TTL; [0] is unused.</summary>
		private Hop[] hops;
		/// <summary>The TTL at which the destination (or a router reporting it unreachable) answered, or 0 while unknown.</summary>
		private int destinationHop;

		public TracerouteTool()
		{
			InitializeComponent();
		}

		/// <summary>Builds the Export button's content: the traced route and its status line.</summary>
		public ExportableContent BuildExportContent()
		{
			ExportableContent content = new ExportableContent("Traceroute");
			if (targetText != null)
			{
				string text = "Target: " + targetText;
				if (lblStatus.Text.Length > 0)
					text += Environment.NewLine + lblStatus.Text;
				content.AddText(null, text);
			}
			content.AddListView(null, listHops);
			return content;
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			// MainForm hides this control when the user switches to a different tool; tracing
			// should not continue in the background.
			if (!Visible && running)
				StopTrace("Trace stopped because the Traceroute tool was deactivated.");
		}

		private void btnStartStop_Click(object sender, EventArgs e)
		{
			if (running)
				StopTrace("Trace stopped.");
			else
				BeginStartTrace();
		}

		private void txtTarget_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
				if (!running)
					BeginStartTrace();
			}
		}

		private async void BeginStartTrace()
		{
			string target = txtTarget.Text.Trim();
			if (target.Length == 0)
			{
				MessageBox.Show(this, "Enter a host name or IP address to trace the route to.", "Traceroute", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			IPAddress address;
			// Disabled while the host name resolves so a second click cannot start a second session.
			btnStartStop.Enabled = false;
			try
			{
				if (!IPAddress.TryParse(target, out address))
				{
					address = await ResolveAsync(target, chkPreferIpv4.Checked);
					if (address == null)
					{
						lblStatus.Text = "Unable to resolve target system name " + target + ".";
						return;
					}
				}
				// The user may have switched tools while the host name was resolving.
				if (!Visible || running)
					return;
			}
			finally
			{
				btnStartStop.Enabled = true;
			}
			await RunTrace(target, address);
		}

		/// <summary>
		/// Resolves a host name to an address of the preferred family, falling back to whatever
		/// family the name does resolve to when the preferred one is unavailable.
		/// </summary>
		private static async Task<IPAddress> ResolveAsync(string host, bool preferIPv4)
		{
			try
			{
				IPAddress[] addresses = await Dns.GetHostAddressesAsync(host);
				AddressFamily preferred = preferIPv4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6;
				return addresses.FirstOrDefault(a => a.AddressFamily == preferred) ?? addresses.FirstOrDefault();
			}
			catch (SocketException)
			{
				return null;
			}
			catch (ArgumentException)
			{
				// Thrown for malformed host names, which IPAddress.TryParse did not catch.
				return null;
			}
		}

		/// <summary>
		/// The trace itself.  Each round probes every hop that has not answered yet, so all hops
		/// are discovered concurrently and each gets up to ProbeAttempts chances to answer in the
		/// face of packet loss.  The rounds stop early once the whole route has answered;
		/// otherwise the final round's probes are given their full timeout before unanswered hops
		/// are declared unresponsive.
		/// </summary>
		private async Task RunTrace(string target, IPAddress address)
		{
			session++;
			int mySession = session;
			running = true;
			targetAddress = address;
			targetText = target.Equals(address.ToString()) ? target : target + " [" + address + "]";
			destinationHop = 0;
			btnStartStop.Text = "Stop";
			txtTarget.ReadOnly = true;
			chkPreferIpv4.Enabled = false;
			lblStatus.Text = "Tracing route to " + targetText + " over a maximum of " + MaxHops + " hops...";

			hops = new Hop[MaxHops + 1];
			listHops.BeginUpdate();
			listHops.Items.Clear();
			for (int ttl = 1; ttl <= MaxHops; ttl++)
			{
				Hop hop = new Hop { Ttl = ttl };
				hop.Item = new ListViewItem(new string[] { ttl.ToString(), "", "", "" });
				hop.Item.Tag = hop;
				hops[ttl] = hop;
				listHops.Items.Add(hop.Item);
			}
			listHops.EndUpdate();

			List<Task> probes = new List<Task>();
			for (int round = 0; round < ProbeAttempts; round++)
			{
				if (round > 0)
				{
					await Task.Delay(ProbeIntervalMs);
					if (session != mySession || IsDisposed)
						return;
				}
				if (RouteComplete())
					break;
				int lastTtl = destinationHop > 0 ? destinationHop : MaxHops;
				for (int ttl = 1; ttl <= lastTtl; ttl++)
					if (!hops[ttl].Answered)
						probes.Add(ProbeHop(hops[ttl], mySession));
			}
			// If the route completes while waiting here, RecordReply finishes the trace
			// immediately and the FinishTrace below becomes a no-op.
			if (!RouteComplete())
			{
				await Task.WhenAll(probes);
				if (session != mySession || IsDisposed)
					return;
			}
			FinishTrace();
		}

		private async Task ProbeHop(Hop hop, int mySession)
		{
			try
			{
				// A Ping instance only supports one operation at a time, so each concurrent probe
				// gets its own.  The TTL in PingOptions is what makes the router that many hops
				// away answer with "TTL expired in transit".
				using (NetPing ping = new NetPing())
				{
					// PingReply.RoundtripTime is only reliably filled in for Success replies, so
					// the round trip is also timed here as a fallback for TTL-expired replies.
					// The measured time includes a little async scheduling overhead, but is
					// accurate to within a few milliseconds.
					Stopwatch timer = Stopwatch.StartNew();
					PingReply reply = await ping.SendPingAsync(targetAddress, ProbeTimeoutMs, PingUtil.PingPayload, new PingOptions(hop.Ttl, false));
					long elapsedMs = timer.ElapsedMilliseconds;
					if (session != mySession || IsDisposed || hop.Answered)
						return;
					if (reply.Status == IPStatus.Success || IsDestinationUnreachable(reply.Status)
						|| ((reply.Status == IPStatus.TtlExpired || reply.Status == IPStatus.TimeExceeded) && IsUsableAddress(reply.Address)))
						RecordReply(hop, reply, elapsedMs, mySession);
					// Any other status (usually TimedOut) leaves the hop unanswered so a later
					// round retries it.
				}
			}
			catch
			{
				// Probe errors are routine (transient ICMP failures, unreachable routes); the hop
				// simply stays unanswered and is retried by the next round.
			}
		}

		private void RecordReply(Hop hop, PingReply reply, long elapsedMs, int mySession)
		{
			bool ttlExpired = reply.Status == IPStatus.TtlExpired || reply.Status == IPStatus.TimeExceeded;
			// With many echo requests outstanding to the same destination, a reply is very
			// occasionally matched to the wrong request (observed in testing; most likely a NAT
			// remapping ICMP ids).  A "destination reached" reply at this TTL is provably spurious
			// if a router at an equal or greater TTL has already answered "TTL expired" — the
			// destination cannot be nearer than a router that expired a larger TTL.  Discarding
			// the reply leaves the hop unanswered, so a later probe round records the real one.
			if (!ttlExpired && HasTtlExpiredAtOrBeyond(hop.Ttl))
				return;
			hop.Answered = true;
			hop.Address = IsUsableAddress(reply.Address) ? reply.Address : null;
			hop.Rtt = reply.RoundtripTime > 0 ? reply.RoundtripTime : elapsedMs;
			hop.Status = reply.Status;
			// A reply other than "TTL expired" marks the end of the route.  Because all TTLs are
			// probed concurrently, the destination answers every TTL >= its true distance, so the
			// end point is moved up each time a smaller TTL gets such a reply, and the rows
			// beyond it are dropped.
			if (!ttlExpired && (destinationHop == 0 || hop.Ttl < destinationHop))
			{
				destinationHop = hop.Ttl;
				listHops.BeginUpdate();
				for (int ttl = destinationHop + 1; ttl <= MaxHops; ttl++)
				{
					hops[ttl].Answered = true; // Stops further probes of hops beyond the destination.
					if (hops[ttl].Item != null)
					{
						hops[ttl].Item.Remove();
						hops[ttl].Item = null;
					}
				}
				listHops.EndUpdate();
			}
			UpdateHopRow(hop);
			if (hop.Address != null)
				BeginHostnameLookup(hop, mySession);
			if (running && RouteComplete())
				FinishTrace();
		}

		private bool HasTtlExpiredAtOrBeyond(int ttl)
		{
			for (int k = ttl; k <= MaxHops; k++)
				if (hops[k].Answered && (hops[k].Status == IPStatus.TtlExpired || hops[k].Status == IPStatus.TimeExceeded))
					return true;
			return false;
		}

		/// <summary>True once the destination has answered and so has every hop before it.</summary>
		private bool RouteComplete()
		{
			if (destinationHop == 0)
				return false;
			for (int ttl = 1; ttl <= destinationHop; ttl++)
				if (!hops[ttl].Answered)
					return false;
			return true;
		}

		/// <summary>
		/// Resolves the hop's host name with the system resolver, the same reverse lookup tracert
		/// performs.  Results arriving after the trace finishes still fill in, because finishing
		/// naturally does not increment the session counter.
		/// </summary>
		private async void BeginHostnameLookup(Hop hop, int mySession)
		{
			try
			{
				IPHostEntry entry = await Dns.GetHostEntryAsync(hop.Address);
				if (session != mySession || IsDisposed)
					return;
				hop.Hostname = entry.HostName;
				UpdateHopRow(hop);
			}
			catch (SocketException)
			{
				// No PTR record; the row simply shows no host name.
			}
			catch (ArgumentException)
			{
			}
		}

		/// <summary>
		/// Ends a trace that ran to completion: unanswered hops are marked as timed out and the
		/// status line summarizes the result.  Unlike StopTrace, the session counter is not
		/// incremented, so reverse DNS lookups still in flight continue to fill in host names.
		/// </summary>
		private void FinishTrace()
		{
			if (!running)
				return;
			running = false;
			btnStartStop.Text = "Start";
			txtTarget.ReadOnly = false;
			chkPreferIpv4.Enabled = true;
			int lastTtl = destinationHop > 0 ? destinationHop : MaxHops;
			for (int ttl = 1; ttl <= lastTtl; ttl++)
			{
				Hop hop = hops[ttl];
				if (!hop.Answered && hop.Item != null)
				{
					hop.Item.SubItems[1].Text = "*";
					hop.Item.SubItems[2].Text = "Request timed out.";
				}
			}
			if (destinationHop == 0)
				lblStatus.Text = "Trace complete: " + targetText + " did not respond within " + MaxHops + " hops.";
			else if (hops[destinationHop].Status == IPStatus.Success)
				lblStatus.Text = "Trace complete to " + targetText + ": " + destinationHop + " hop" + (destinationHop == 1 ? "" : "s") + ".";
			else
				lblStatus.Text = "Trace ended at hop " + destinationHop + ": " + PingUtil.DescribeStatus(hops[destinationHop].Status) + ".";
		}

		/// <summary>
		/// Stops a trace before it completes (Stop pressed or the tool deactivated).  Incrementing
		/// the session makes in-flight probe and DNS callbacks discard their results.
		/// </summary>
		private void StopTrace(string statusText)
		{
			if (!running)
				return;
			session++;
			running = false;
			btnStartStop.Text = "Start";
			txtTarget.ReadOnly = false;
			chkPreferIpv4.Enabled = true;
			lblStatus.Text = statusText;
		}

		private void UpdateHopRow(Hop hop)
		{
			if (hop.Item == null)
				return;
			hop.Item.SubItems[1].Text = hop.Rtt < 0 ? "" : (hop.Rtt <= 0 ? "<1 ms" : hop.Rtt + " ms");
			hop.Item.SubItems[2].Text = hop.Address != null ? hop.Address.ToString() : "";
			hop.Item.SubItems[3].Text = HostText(hop);
		}

		/// <summary>
		/// The Host Name cell: the reverse-DNS name, plus a note when the hop's reply was
		/// something other than an ordinary echo reply or "TTL expired" (e.g. a router reporting
		/// the destination unreachable).
		/// </summary>
		private static string HostText(Hop hop)
		{
			string note = "";
			if (hop.Answered && hop.Status != IPStatus.Success && hop.Status != IPStatus.TtlExpired && hop.Status != IPStatus.TimeExceeded)
				note = PingUtil.DescribeStatus(hop.Status);
			if (hop.Hostname.Length == 0)
				return note;
			return note.Length == 0 ? hop.Hostname : hop.Hostname + "  (" + note + ")";
		}

		/// <summary>
		/// True for replies where a router (or the local stack) reports that the destination
		/// cannot be reached at all.  Such a reply ends the route the same way a Success reply
		/// does, since probing beyond the reporting router is pointless.
		/// </summary>
		private static bool IsDestinationUnreachable(IPStatus status)
		{
			switch (status)
			{
				case IPStatus.DestinationHostUnreachable:
				case IPStatus.DestinationNetworkUnreachable:
				case IPStatus.DestinationPortUnreachable:
				case IPStatus.DestinationProtocolUnreachable: // Same value as DestinationProhibited (IPv6).
				case IPStatus.DestinationUnreachable:
				case IPStatus.DestinationScopeMismatch:
				case IPStatus.BadDestination:
					return true;
				default:
					return false;
			}
		}

		private static bool IsUsableAddress(IPAddress address)
		{
			return address != null && !address.Equals(IPAddress.Any) && !address.Equals(IPAddress.IPv6Any);
		}

		private void listHops_ItemActivate(object sender, EventArgs e)
		{
			if (listHops.SelectedItems.Count == 0)
				return;
			Hop hop = (Hop)listHops.SelectedItems[0].Tag;
			if (hop.Address == null)
				return;
			MainForm main = FindForm() as MainForm;
			if (main == null)
				return;
			// Activating the Ping tool hides this one, which stops the trace (OnVisibleChanged).
			PingTool ping = main.ActivateTool<PingTool>();
			if (ping != null)
				ping.StartPing(hop.Address.ToString());
		}
	}
}
