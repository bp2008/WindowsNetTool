using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsNetTool.Tools.Export;
// The System.Net.NetworkInformation.Ping type must be aliased because the simple name "Ping"
// binds to this tool's enclosing namespace.
using NetPing = System.Net.NetworkInformation.Ping;

namespace WindowsNetTool.Tools.Ping
{
	/// <summary>
	/// A continuous ping monitor with output styled after Windows' ping command, an adjustable
	/// ping rate, and automatic stopping when the user switches to a different tool.  Other tools
	/// can link into this one via MainForm.ActivateTool&lt;PingTool&gt;().StartPing(target).
	/// </summary>
	public partial class PingTool : UserControl, IExportableTool
	{
		/// <summary>
		/// Time to wait for each echo reply, matching the Windows ping command's default.  Pings
		/// are sent concurrently, so a slow or dead host does not reduce the configured ping rate.
		/// </summary>
		private const int PingTimeoutMs = 4000;

		/// <summary>Keep the log around this size; the older half is dropped when it grows past this.</summary>
		private const int MaxLogLength = 400000;

		/// <summary>
		/// Milliseconds between pings for each slider position, from 1 ping per 10 seconds up to
		/// 10 pings per second.  Values under 1000 are chosen so the displayed rate rounds to a
		/// clean "pings per second" number.
		/// </summary>
		private static readonly int[] RateIntervalsMs = { 10000, 8000, 6000, 5000, 4000, 3000, 2000, 1500, 1000, 667, 500, 333, 250, 200, 167, 125, 100 };
		private const int DefaultRateIndex = 8; // 1 ping per second

		private bool running = false;
		/// <summary>Incremented whenever pinging starts or stops so in-flight replies from an earlier session are ignored.</summary>
		private int session = 0;
		private IPAddress targetAddress;
		private int sent, received;
		private long rttMin, rttMax, rttSum;

		public PingTool()
		{
			InitializeComponent();
			trackRate.Minimum = 0;
			trackRate.Maximum = RateIntervalsMs.Length - 1;
			trackRate.Value = DefaultRateIndex;
			lblRate.Text = DescribeRate(CurrentIntervalMs);
		}

		/// <summary>Builds the Export button's content: the ping log plus the current statistics line.</summary>
		public ExportableContent BuildExportContent()
		{
			ExportableContent content = new ExportableContent("Ping");
			content.AddText(null, txtLog.Text);
			if (lblStats.Text.Length > 0)
				content.AddText(null, "Statistics: " + lblStats.Text);
			return content;
		}

		private int CurrentIntervalMs
		{
			get { return RateIntervalsMs[trackRate.Value]; }
		}

		/// <summary>
		/// Sets the ping target and begins pinging it, stopping any ping already in progress.
		/// This is the entry point for other tools (e.g. the ARP viewer or a future IP scanner)
		/// to begin ping monitoring of an address:
		/// ((MainForm)FindForm()).ActivateTool&lt;PingTool&gt;().StartPing(address);
		/// </summary>
		public void StartPing(string hostOrAddress)
		{
			StopPing(null);
			txtTarget.Text = hostOrAddress;
			BeginStartPing();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			// MainForm hides this control when the user switches to a different tool; pinging
			// should not continue in the background.
			if (!Visible && running)
				StopPing("Ping stopped because the Ping tool was deactivated.");
		}

		private void btnStartStop_Click(object sender, EventArgs e)
		{
			if (running)
				StopPing(null);
			else
				BeginStartPing();
		}

		private void btnClear_Click(object sender, EventArgs e)
		{
			txtLog.Clear();
		}

		private void txtTarget_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
				if (!running)
					BeginStartPing();
			}
		}

		private void trackRate_ValueChanged(object sender, EventArgs e)
		{
			lblRate.Text = DescribeRate(CurrentIntervalMs);
			if (running)
				pingTimer.Interval = CurrentIntervalMs;
		}

		private async void BeginStartPing()
		{
			string target = txtTarget.Text.Trim();
			if (target.Length == 0)
			{
				MessageBox.Show(this, "Enter a host name or IP address to ping.", "Ping", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			// Disabled while the host name resolves so a second click cannot start a second session.
			btnStartStop.Enabled = false;
			try
			{
				IPAddress address;
				if (!IPAddress.TryParse(target, out address))
				{
					address = await ResolveAsync(target);
					if (address == null)
					{
						AppendLog("Ping request could not find host " + target + ". Please check the name and try again.");
						return;
					}
				}
				// The user may have switched tools while the host name was resolving.
				if (!Visible || running)
					return;

				session++;
				running = true;
				targetAddress = address;
				sent = received = 0;
				rttMin = rttMax = rttSum = 0;
				btnStartStop.Text = "Stop";
				txtTarget.ReadOnly = true;
				UpdateStatsLabel();

				string header = target.Equals(address.ToString()) ? target : target + " [" + address + "]";
				if (txtLog.TextLength > 0)
					AppendLog("");
				AppendLog("Pinging " + header + " with " + PingUtil.PayloadSize + " bytes of data:");

				pingTimer.Interval = CurrentIntervalMs;
				pingTimer.Start();
				SendOnePing();
			}
			finally
			{
				btnStartStop.Enabled = true;
			}
		}

		private static async Task<IPAddress> ResolveAsync(string host)
		{
			try
			{
				IPAddress[] addresses = await Dns.GetHostAddressesAsync(host);
				// The app is IPv4-focused, so prefer an IPv4 address when the host has both kinds.
				return addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork) ?? addresses.FirstOrDefault();
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
		/// Stops pinging and appends a Windows-style statistics summary.  <paramref name="reason"/>
		/// is logged before the summary if not null.  Does nothing if pinging is not in progress.
		/// </summary>
		private void StopPing(string reason)
		{
			if (!running)
				return;
			session++;
			running = false;
			pingTimer.Stop();
			btnStartStop.Text = "Start";
			txtTarget.ReadOnly = false;
			if (reason != null)
				AppendLog(reason);
			AppendStatisticsSummary();
		}

		private void pingTimer_Tick(object sender, EventArgs e)
		{
			SendOnePing();
		}

		private async void SendOnePing()
		{
			int mySession = session;
			sent++;
			UpdateStatsLabel();
			// The reply is stamped with the send time, since at fast ping rates a timed-out ping
			// is logged up to 4 seconds after later pings have already been answered.
			string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
			try
			{
				// A Ping instance only supports one operation at a time, so each concurrent send
				// gets its own.
				using (NetPing ping = new NetPing())
				{
					PingReply reply = await ping.SendPingAsync(targetAddress, PingTimeoutMs, PingUtil.PingPayload);
					if (mySession != session || IsDisposed)
						return;
					LogReply(timestamp, reply);
				}
			}
			catch (Exception ex)
			{
				if (mySession != session || IsDisposed)
					return;
				Exception inner = ex.InnerException ?? ex;
				AppendLog("[" + timestamp + "] Ping failed: " + inner.Message);
			}
		}

		private void LogReply(string timestamp, PingReply reply)
		{
			string line;
			if (reply.Status == IPStatus.Success)
			{
				long rtt = reply.RoundtripTime;
				received++;
				if (received == 1 || rtt < rttMin)
					rttMin = rtt;
				if (received == 1 || rtt > rttMax)
					rttMax = rtt;
				rttSum += rtt;
				string time = rtt <= 0 ? "time<1ms" : "time=" + rtt + "ms";
				// Options is null for IPv6 replies, which do not carry a TTL in the same way.
				string ttl = reply.Options != null ? " TTL=" + reply.Options.Ttl : "";
				line = "Reply from " + reply.Address + ": bytes=" + (reply.Buffer != null ? reply.Buffer.Length : 0) + " " + time + ttl;
			}
			else if (reply.Status == IPStatus.TimedOut)
				line = "Request timed out.";
			else if (reply.Address != null && !reply.Address.Equals(IPAddress.Any) && !reply.Address.Equals(IPAddress.IPv6Any))
				line = "Reply from " + reply.Address + ": " + PingUtil.DescribeStatus(reply.Status) + ".";
			else
				line = PingUtil.DescribeStatus(reply.Status) + ".";
			AppendLog("[" + timestamp + "] " + line);
			UpdateStatsLabel();
		}

		private void AppendStatisticsSummary()
		{
			if (sent == 0)
				return;
			int lost = sent - received;
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendLine("Ping statistics for " + targetAddress + ":");
			sb.Append("    Packets: Sent = " + sent + ", Received = " + received + ", Lost = " + lost
				+ " (" + PercentLoss(lost, sent) + "% loss)");
			if (received > 0)
			{
				sb.AppendLine(",");
				sb.AppendLine("Approximate round trip times in milli-seconds:");
				sb.Append("    Minimum = " + rttMin + "ms, Maximum = " + rttMax + "ms, Average = " + (rttSum / received) + "ms");
			}
			AppendLog(sb.ToString());
		}

		private void UpdateStatsLabel()
		{
			if (sent == 0)
			{
				lblStats.Text = "";
				return;
			}
			int lost = sent - received;
			string text = "Sent = " + sent + ", Received = " + received + ", Lost = " + lost + " (" + PercentLoss(lost, sent) + "% loss)";
			if (received > 0)
				text += ", RTT min/avg/max = " + rttMin + "/" + (rttSum / received) + "/" + rttMax + " ms";
			lblStats.Text = text;
		}

		private static string PercentLoss(int lost, int sent)
		{
			return (100.0 * lost / sent).ToString("0.#");
		}

		private void AppendLog(string line)
		{
			if (IsDisposed || Disposing)
				return;
			if (txtLog.TextLength > MaxLogLength)
			{
				// Drop the older half of the log at a line boundary so long monitoring sessions
				// do not degrade TextBox performance.
				string text = txtLog.Text;
				int cut = text.IndexOf("\r\n", text.Length / 2, StringComparison.Ordinal);
				txtLog.Text = cut >= 0 ? text.Substring(cut + 2) : "";
			}
			// AppendText moves the caret to the end, which keeps the log scrolled to the bottom.
			txtLog.AppendText(line + Environment.NewLine);
		}

		private static string DescribeRate(int intervalMs)
		{
			if (intervalMs >= 1000)
			{
				double seconds = intervalMs / 1000.0;
				return seconds == 1 ? "1 ping per second" : "1 ping per " + seconds.ToString("0.#") + " seconds";
			}
			return Math.Round(1000.0 / intervalMs, 1).ToString("0.#") + " pings per second";
		}
	}
}
