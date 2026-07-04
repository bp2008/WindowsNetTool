using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsNetTool.Tools.DnsLookup
{
	/// <summary>
	/// Looks up DNS records for a domain name (A, AAAA, MX, TXT, ...) and performs reverse (PTR)
	/// lookups when an IP address is entered, using the built-in <see cref="DnsClient"/> so any DNS
	/// server can be queried.  The server dropdown mixes the system's currently registered DNS
	/// servers with popular public resolvers (deduplicated), and accepts a custom server IP address
	/// typed directly into it.
	/// </summary>
	public partial class DnsLookupTool : UserControl, IRefreshOnActivate
	{
		/// <summary>Timeout for each UDP attempt and for the TCP fallback exchange.</summary>
		private const int QueryTimeoutMs = 3000;

		/// <summary>Keep the log around this size; the older half is dropped when it grows past this.</summary>
		private const int MaxLogLength = 400000;

		/// <summary>Well-known public DNS servers offered in the server dropdown: address, operator.</summary>
		private static readonly string[,] PublicDnsServers =
		{
			{ "1.1.1.1", "Cloudflare" },
			{ "1.0.0.1", "Cloudflare" },
			{ "8.8.8.8", "Google" },
			{ "8.8.4.4", "Google" },
			{ "9.9.9.9", "Quad9" },
			{ "149.112.112.112", "Quad9" },
			{ "208.67.222.222", "OpenDNS" },
			{ "208.67.220.220", "OpenDNS" }
		};

		/// <summary>One entry in the DNS server dropdown; Sources lists where the address came from.</summary>
		private class DnsServerChoice
		{
			public IPAddress Address;
			public List<string> Sources = new List<string>();
			public override string ToString()
			{
				return Address + "  (" + string.Join(", ", Sources) + ")";
			}
		}

		public DnsLookupTool()
		{
			InitializeComponent();
			comboType.Items.Add("Auto (A + AAAA)");
			comboType.Items.AddRange(new object[] { "A", "AAAA", "ANY", "CAA", "CNAME", "MX", "NS", "PTR", "SOA", "SRV", "TXT" });
			comboType.SelectedIndex = 0;
			PopulateServerList();
		}

		/// <summary>
		/// Rebuilds the server dropdown when the user returns to this tool, because interface DNS
		/// settings may have changed in the meantime (possibly via the IP Configuration tool).
		/// </summary>
		public void RefreshOnActivate()
		{
			PopulateServerList();
		}

		private void PopulateServerList()
		{
			List<DnsServerChoice> choices = new List<DnsServerChoice>();
			Dictionary<string, DnsServerChoice> byAddress = new Dictionary<string, DnsServerChoice>();

			// The system's registered DNS servers come first, gathered from every operational interface.
			try
			{
				foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
				{
					if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
						continue;
					foreach (IPAddress dns in nic.GetIPProperties().DnsAddresses)
					{
						// Windows reports auto-configured site-local IPv6 resolvers (fec0:0:0:ffff::1
						// through ::3) on interfaces with no real IPv6 DNS server; they are dead
						// addresses from a withdrawn standard.
						if (dns.IsIPv6SiteLocal)
							continue;
						DnsServerChoice choice;
						if (!byAddress.TryGetValue(dns.ToString(), out choice))
						{
							choice = new DnsServerChoice { Address = dns };
							byAddress.Add(dns.ToString(), choice);
							choices.Add(choice);
						}
						string source = "System: " + nic.Name;
						if (!choice.Sources.Contains(source))
							choice.Sources.Add(source);
					}
				}
			}
			catch (NetworkInformationException)
			{
				// If interface enumeration fails, the public resolvers below remain usable.
			}

			for (int i = 0; i < PublicDnsServers.GetLength(0); i++)
			{
				string address = PublicDnsServers[i, 0];
				string operatorName = PublicDnsServers[i, 1];
				DnsServerChoice choice;
				if (byAddress.TryGetValue(address, out choice))
				{
					// A system server that is also a well-known public resolver gets one combined entry.
					choice.Sources.Add(operatorName);
				}
				else
				{
					choice = new DnsServerChoice { Address = IPAddress.Parse(address) };
					choice.Sources.Add(operatorName);
					byAddress.Add(address, choice);
					choices.Add(choice);
				}
			}

			// Preserve the user's selection or custom entry across refreshes.
			string previousText = comboServer.Text;
			comboServer.BeginUpdate();
			comboServer.Items.Clear();
			comboServer.Items.AddRange(choices.ToArray());
			comboServer.EndUpdate();
			if (previousText.Length > 0)
				comboServer.Text = previousText;
			else if (comboServer.Items.Count > 0)
				comboServer.SelectedIndex = 0;
		}

		private void btnLookup_Click(object sender, EventArgs e)
		{
			BeginLookup();
		}

		private void btnClear_Click(object sender, EventArgs e)
		{
			txtLog.Clear();
		}

		private void queryInput_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
				BeginLookup();
			}
		}

		/// <summary>
		/// Parses the server combo's text into an IP address.  List items render as
		/// "1.1.1.1  (Cloudflare)", so only the leading token is parsed, which also lets the user
		/// type a bare IP address for a custom server.  Returns null if no valid address is present.
		/// </summary>
		private IPAddress GetSelectedServer()
		{
			string text = comboServer.Text.Trim();
			if (text.Length == 0)
				return null;
			int cut = text.IndexOfAny(new char[] { ' ', '\t', '(' });
			if (cut >= 0)
				text = text.Substring(0, cut);
			IPAddress address;
			return IPAddress.TryParse(text, out address) ? address : null;
		}

		private async void BeginLookup()
		{
			if (!btnLookup.Enabled)
				return; // a lookup is already in progress (the Enter key can bypass the disabled button)
			string query = txtQuery.Text.Trim();
			if (query.Length == 0)
			{
				MessageBox.Show(this, "Enter a domain name to look up, or an IP address for a reverse lookup.",
					"DNS Lookup", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			IPAddress server = GetSelectedServer();
			if (server == null)
			{
				MessageBox.Show(this, "Select a DNS server from the list, or type the IP address of the DNS server to query.",
					"DNS Lookup", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			bool auto = comboType.SelectedIndex == 0;
			DnsRecordType selectedType = auto ? DnsRecordType.A : (DnsRecordType)Enum.Parse(typeof(DnsRecordType), (string)comboType.SelectedItem);

			List<string> names = new List<string>();
			List<DnsRecordType> types = new List<DnsRecordType>();
			IPAddress queryAddress;
			if (IPAddress.TryParse(query, out queryAddress) && (auto || selectedType == DnsRecordType.PTR))
			{
				// Reverse lookup: an IP address is queried via its PTR name, e.g. "4.4.8.8.in-addr.arpa".
				names.Add(DnsClient.ReverseName(queryAddress));
				types.Add(DnsRecordType.PTR);
			}
			else if (auto)
			{
				names.Add(query);
				types.Add(DnsRecordType.A);
				names.Add(query);
				types.Add(DnsRecordType.AAAA);
			}
			else
			{
				names.Add(query);
				types.Add(selectedType);
			}

			btnLookup.Enabled = false;
			try
			{
				// All queries start concurrently, then are logged in order as each completes.
				List<Task<DnsResponse>> tasks = new List<Task<DnsResponse>>();
				for (int i = 0; i < names.Count; i++)
				{
					string name = names[i];
					DnsRecordType type = types[i];
					tasks.Add(Task.Run(() => DnsClient.Query(server, name, type, QueryTimeoutMs)));
				}
				for (int i = 0; i < tasks.Count; i++)
				{
					if (txtLog.TextLength > 0)
						AppendLog("");
					string described = names[i].Equals(query, StringComparison.OrdinalIgnoreCase) ? query : query + " (" + names[i] + ")";
					AppendLog("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + described + "  " + types[i] + "  @ " + server);
					try
					{
						DnsResponse response = await tasks[i];
						if (IsDisposed)
							return;
						AppendResponse(response);
					}
					catch (Exception ex)
					{
						if (IsDisposed)
							return;
						Exception inner = ex.InnerException ?? ex;
						AppendLog("Lookup failed: " + inner.Message);
					}
				}
			}
			finally
			{
				if (!IsDisposed)
					btnLookup.Enabled = true;
			}
		}

		private void AppendResponse(DnsResponse response)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Result: ").Append(DnsClient.ResponseCodeDescription(response.ResponseCode));
			if (response.ResponseCode != 0)
				sb.Append(" (").Append(DnsClient.ResponseCodeName(response.ResponseCode)).Append(")");
			sb.Append(" in ").Append(response.ElapsedMs).Append(" ms via ").Append(response.UsedTcp ? "TCP" : "UDP");
			if (response.AuthoritativeAnswer)
				sb.Append(", authoritative");
			if (!response.RecursionAvailable)
				sb.Append(", recursion unavailable");
			AppendLog(sb.ToString());
			if (response.Answers.Count == 0 && response.ResponseCode == 0)
				AppendLog("The name exists but has no records of the requested type.");
			AppendRecordSection("Answer records:", response.Answers);
			AppendRecordSection("Authority records:", response.Authority);
			AppendRecordSection("Additional records:", response.Additional);
		}

		private void AppendRecordSection(string caption, List<DnsRecord> records)
		{
			if (records.Count == 0)
				return;
			AppendLog(caption);
			// Size the name column to the records at hand; extremely long names simply overflow it.
			int nameWidth = 24;
			foreach (DnsRecord record in records)
				nameWidth = Math.Max(nameWidth, Math.Min(record.Name.Length, 44));
			foreach (DnsRecord record in records)
			{
				AppendLog("  " + record.Name.PadRight(nameWidth)
					+ "  " + record.Ttl.ToString().PadLeft(7)
					+ "  " + DnsClient.TypeName(record.Type).PadRight(6)
					+ "  " + record.Data);
			}
		}

		private void AppendLog(string line)
		{
			if (IsDisposed || Disposing)
				return;
			if (txtLog.TextLength > MaxLogLength)
			{
				// Drop the older half of the log at a line boundary so long sessions do not degrade
				// TextBox performance.
				string text = txtLog.Text;
				int cut = text.IndexOf("\r\n", text.Length / 2, StringComparison.Ordinal);
				txtLog.Text = cut >= 0 ? text.Substring(cut + 2) : "";
			}
			// AppendText moves the caret to the end, which keeps the log scrolled to the bottom.
			txtLog.AppendText(line + Environment.NewLine);
		}
	}
}
