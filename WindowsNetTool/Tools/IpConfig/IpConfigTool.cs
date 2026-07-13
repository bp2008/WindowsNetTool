using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsNetTool.Tools.NetworkCategory;
using WindowsNetTool.Tools.LinksShortcuts;
using WindowsNetTool.Tools.Export;

namespace WindowsNetTool.Tools.IpConfig
{
	public partial class IpConfigTool : UserControl, IRefreshOnActivate, IExportableTool
	{
		private bool busy = false;
		private Dictionary<string, InterfaceExtraInfo> extraInfoByName = new Dictionary<string, InterfaceExtraInfo>();

		private Ipv4Interface SelectedInterface
		{
			get { return comboInterfaces.SelectedItem as Ipv4Interface; }
		}

		public IpConfigTool()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Builds the Export button's content: every interface from the last scan with its info
		/// summary, addresses, gateways, and DNS servers (the on-screen view shows one interface
		/// at a time).
		/// </summary>
		public ExportableContent BuildExportContent()
		{
			ExportableContent content = new ExportableContent("IP Configuration");
			foreach (object item in comboInterfaces.Items)
			{
				Ipv4Interface iface = (Ipv4Interface)item;
				content.AddText("Interface: " + iface.InterfaceName, BuildInfoText(iface));

				List<string[]> addressRows = new List<string[]>(iface.IpAddresses.Count);
				foreach (Ipv4Address addr in iface.IpAddresses)
					addressRows.Add(new string[]
					{
						addr.Ip.ToString(),
						addr.Mask != null ? addr.Mask.ToString() + " (/" + NetshIpv4.GetPrefixSizeOfMask(addr.Mask) + ")" : "",
						addr.IsStatic ? "static" : "DHCP"
					});
				content.AddTable("IPv4 Addresses", new string[] { "Address", "Subnet Mask", "Source" }, addressRows);

				List<string[]> gatewayRows = new List<string[]>(iface.Gateways.Count);
				foreach (Ipv4Gateway gw in iface.Gateways)
					gatewayRows.Add(new string[]
					{
						gw.Ip.ToString(),
						gw.Metric == 0 ? "0 (automatic)" : gw.Metric.ToString(),
						gw.IsStatic ? "static" : "dynamic"
					});
				content.AddTable("Default Gateways", new string[] { "Gateway", "Metric", "Source" }, gatewayRows);

				List<string[]> dnsRows = new List<string[]>(iface.DnsServers.Count);
				foreach (IPAddress dns in iface.DnsServers)
					dnsRows.Add(new string[] { dns.ToString() });
				content.AddTable("DNS Servers (source: " + (iface.DnsFromDhcp ? "DHCP" : "static") + ")", new string[] { "DNS Server" }, dnsRows);
			}
			return content;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignMode)
				RefreshInterfaces();
		}

		public void RefreshOnActivate()
		{
			RefreshInterfaces();
		}

		/// <summary>
		/// Rescans and repopulates the UI.  The current interface selection is preserved,
		/// or the interface named by selectName is selected instead (e.g. after a rename).
		/// </summary>
		private async void RefreshInterfaces(string selectName = null)
		{
			if (busy)
				return;
			SetBusy(true);
			try
			{
				ScanResult result = null;
				Dictionary<string, InterfaceExtraInfo> extra = null;
				await Task.Run(() =>
				{
					result = NetshIpv4.Scan();
					extra = GatherExtraInfo();
				});
				extraInfoByName = extra;

				string previousSelection = selectName ?? SelectedInterface?.InterfaceName;
				comboInterfaces.Items.Clear();
				foreach (Ipv4Interface iface in result.Interfaces)
					comboInterfaces.Items.Add(iface);

				if (comboInterfaces.Items.Count > 0)
				{
					int selectIndex = 0;
					if (previousSelection != null)
					{
						for (int i = 0; i < comboInterfaces.Items.Count; i++)
						{
							if (((Ipv4Interface)comboInterfaces.Items[i]).InterfaceName == previousSelection)
							{
								selectIndex = i;
								break;
							}
						}
					}
					comboInterfaces.SelectedIndex = selectIndex;
					UpdateInterfaceDetails();
				}
				else
					UpdateInterfaceDetails();

				if (result.Errors.Count > 0)
					MessageBox.Show(this, string.Join(Environment.NewLine + Environment.NewLine, result.Errors), "Interface scan problems", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Interface scan failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				SetBusy(false);
			}
		}

		/// <summary>
		/// Gathers diagnostic information about each interface: which network owns it, operational
		/// status, link speed, MAC address, and adapter description.  Runs on a background thread.
		/// </summary>
		private static Dictionary<string, InterfaceExtraInfo> GatherExtraInfo()
		{
			Dictionary<string, InterfaceExtraInfo> result = new Dictionary<string, InterfaceExtraInfo>();
			Dictionary<Guid, AdapterNetwork> networkMap;
			try
			{
				networkMap = NetworkCategoryService.GetAdapterNetworkMap();
			}
			catch
			{
				networkMap = new Dictionary<Guid, AdapterNetwork>();
			}
			foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
			{
				try
				{
					InterfaceExtraInfo info = new InterfaceExtraInfo();
					info.Status = nic.OperationalStatus;
					info.Description = nic.Description;
					try
					{
						info.SpeedBps = nic.Speed;
					}
					catch (Exception)
					{
						info.SpeedBps = -1;
					}
					byte[] mac = nic.GetPhysicalAddress().GetAddressBytes();
					if (mac.Length > 0)
						info.Mac = BitConverter.ToString(mac);
					if (Guid.TryParse(nic.Id, out Guid adapterId) && networkMap.TryGetValue(adapterId, out AdapterNetwork adapterNetwork))
					{
						info.NetworkName = adapterNetwork.NetworkName;
						info.NetworkConnectedUtc = adapterNetwork.ConnectedSinceUtc;
					}
					result[nic.Name] = info;
				}
				catch (Exception)
				{
					// Skip adapters that fail to report information.
				}
			}
			return result;
		}

		private void UpdateInterfaceDetails()
		{
			listAddresses.Items.Clear();
			listGateways.Items.Clear();
			listDns.Items.Clear();
			Ipv4Interface iface = SelectedInterface;
			bool haveInterface = iface != null;
			btnDhcpToggle.Enabled = haveInterface;
			btnStatusWindow.Enabled = haveInterface;
			btnRenameInterface.Enabled = haveInterface;
			groupAddresses.Enabled = haveInterface;
			groupGateways.Enabled = haveInterface;
			groupDns.Enabled = haveInterface;
			if (!haveInterface)
			{
				lblInterfaceInfo.Text = "No interface selected.";
				btnDhcpToggle.Text = "Enable DHCP";
				lblDnsSource.Text = "";
				return;
			}
			lblInterfaceInfo.Text = BuildInfoText(iface);
			btnDhcpToggle.Text = iface.DhcpEnabled ? "Disable DHCP" : "Enable DHCP";

			foreach (Ipv4Address addr in iface.IpAddresses)
			{
				ListViewItem item = new ListViewItem(new string[]
				{
					addr.Ip.ToString(),
					addr.Mask != null ? addr.Mask.ToString() + " (/" + NetshIpv4.GetPrefixSizeOfMask(addr.Mask) + ")" : "",
					addr.IsStatic ? "static" : "DHCP"
				});
				item.Tag = addr;
				listAddresses.Items.Add(item);
			}

			foreach (Ipv4Gateway gw in iface.Gateways)
			{
				ListViewItem item = new ListViewItem(new string[]
				{
					gw.Ip.ToString(),
					gw.Metric == 0 ? "0 (automatic)" : gw.Metric.ToString(),
					gw.IsStatic ? "static" : "dynamic"
				});
				item.Tag = gw;
				listGateways.Items.Add(item);
			}
			// Reordering rewrites gateway metrics, which is only possible when every gateway is static.
			bool canReorderGateways = iface.Gateways.Count > 1 && iface.Gateways.All(g => g.IsStatic);
			btnGwMoveUp.Enabled = canReorderGateways;
			btnGwMoveDown.Enabled = canReorderGateways;

			lblDnsSource.Text = "Source: " + (iface.DnsFromDhcp ? "DHCP (automatic)" : "Static");
			foreach (IPAddress dns in iface.DnsServers)
				listDns.Items.Add(dns.ToString());

			// DHCP-provided DNS servers cannot be edited in place; only switching to static is possible.
			bool staticDns = !iface.DnsFromDhcp;
			btnDnsRemove.Enabled = staticDns;
			btnDnsMoveUp.Enabled = staticDns;
			btnDnsMoveDown.Enabled = staticDns;
			btnDnsUseDhcp.Enabled = staticDns;
		}

		private string BuildInfoText(Ipv4Interface iface)
		{
			extraInfoByName.TryGetValue(iface.InterfaceName, out InterfaceExtraInfo extra);
			string line1;
			if (extra != null && extra.NetworkName != null)
			{
				line1 = "Network: " + extra.NetworkName;
				if (extra.NetworkConnectedUtc > DateTime.MinValue)
					line1 += "   (connected " + FormatDuration(DateTime.UtcNow - extra.NetworkConnectedUtc) + ")";
			}
			else
				line1 = "Network: (none)";
			string line2 = "Status: " + (extra != null ? extra.Status.ToString() : "unknown")
				+ "      Speed: " + FormatSpeed(extra != null ? extra.SpeedBps : -1)
				+ (extra != null && extra.Mac != null ? "      MAC: " + extra.Mac : "");
			string line3 = "Adapter: " + (extra != null && !string.IsNullOrEmpty(extra.Description) ? extra.Description : "(unknown)");
			string line4 = "DHCP: " + (iface.DhcpEnabled ? "enabled" : "disabled")
				+ "      DHCP+Static coexistence: " + (iface.DhcpStaticIpCoexistence ? "enabled" : "disabled");
			return line1 + Environment.NewLine + line2 + Environment.NewLine + line3 + Environment.NewLine + line4;
		}

		private static string FormatSpeed(long bps)
		{
			if (bps < 0)
				return "unknown";
			if (bps >= 1000000000L)
				return TrimNumber(bps / 1000000000.0) + " Gbps";
			if (bps >= 1000000L)
				return TrimNumber(bps / 1000000.0) + " Mbps";
			if (bps >= 1000L)
				return TrimNumber(bps / 1000.0) + " Kbps";
			return bps + " bps";
		}

		private static string TrimNumber(double value)
		{
			return value.ToString("0.##");
		}

		private static string FormatDuration(TimeSpan span)
		{
			if (span < TimeSpan.Zero)
				span = TimeSpan.Zero;
			if (span.TotalDays >= 1)
				return (int)span.TotalDays + "d " + span.Hours + "h " + span.Minutes + "m";
			if (span.TotalHours >= 1)
				return span.Hours + "h " + span.Minutes + "m";
			return span.Minutes + "m";
		}

		private void SetBusy(bool value)
		{
			busy = value;
			bool enable = !value;
			bool haveInterface = SelectedInterface != null;
			comboInterfaces.Enabled = enable;
			btnRefresh.Enabled = enable;
			btnRenameInterface.Enabled = enable && haveInterface;
			btnDhcpToggle.Enabled = enable && haveInterface;
			btnStatusWindow.Enabled = enable && haveInterface;
			groupAddresses.Enabled = enable && haveInterface;
			groupGateways.Enabled = enable && haveInterface;
			groupDns.Enabled = enable && haveInterface;
			Cursor = value ? Cursors.WaitCursor : Cursors.Default;
		}

		private void comboInterfaces_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateInterfaceDetails();
		}

		private void btnRefresh_Click(object sender, EventArgs e)
		{
			RefreshInterfaces();
		}

		private async void btnRenameInterface_Click(object sender, EventArgs e)
		{
			if (busy)
				return;
			Ipv4Interface iface = SelectedInterface;
			if (iface == null)
				return;
			string newName = TextPromptDialog.Show(FindForm(), "Rename Interface", "New name for interface \"" + iface.InterfaceName + "\":", iface.InterfaceName);
			if (newName == null)
				return;
			newName = newName.Trim();
			if (newName.Length == 0 || newName == iface.InterfaceName)
				return;
			bool renamed = false;
			SetBusy(true);
			try
			{
				await Task.Run(() => NetshIpv4.RenameInterface(iface.InterfaceName, newName));
				renamed = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to rename interface", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				SetBusy(false);
				// Keep the renamed interface selected under its new name.
				RefreshInterfaces(renamed ? newName : null);
			}
		}

		private void btnStatusWindow_Click(object sender, EventArgs e)
		{
			Ipv4Interface iface = SelectedInterface;
			if (iface == null)
				return;
			try
			{
				// Must run on the UI thread; the Windows Shell requires an STA thread.
				if (!NetworkPanels.OpenConnectionStatus(iface.InterfaceName))
				{
					DialogResult open = MessageBox.Show(this,
						"Could not open the Status window for \"" + iface.InterfaceName + "\".  (It is only available while the connection is connected.)"
						+ Environment.NewLine + Environment.NewLine + "Open Network Connections instead?",
						"Status window unavailable", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
					if (open == DialogResult.Yes)
						NetworkPanels.OpenNetworkConnections();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to open Status window", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#region Address Management
		private async void btnAdd_Click(object sender, EventArgs e)
		{
			if (busy)
				return;
			Ipv4Interface iface = SelectedInterface;
			if (iface == null)
				return;
			if (!NetshIpv4.TryParseIpInput(txtNewAddress.Text, out IPAddress ip, out IPAddress mask))
			{
				MessageBox.Show(this, "Enter a static IPv4 address with subnet prefix size, e.g. \"192.168.1.2/24\".", "Invalid address input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			SetBusy(true);
			try
			{
				// Rescan just before adding so the duplicate address check uses current data.
				ScanResult fresh = await Task.Run(() => NetshIpv4.Scan());
				Ipv4Interface freshIface = fresh.Interfaces.FirstOrDefault(i => i.InterfaceName == iface.InterfaceName);
				if (freshIface == null)
				{
					MessageBox.Show(this, "Interface \"" + iface.InterfaceName + "\" no longer exists.", "Interface not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				Ipv4Interface owner = NetshIpv4.FindAddressOwner(fresh, ip);
				if (owner == freshIface)
				{
					MessageBox.Show(this, "The address " + ip + " is already assigned to this interface.", "Duplicate address", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}
				if (owner != null)
				{
					DialogResult proceed = MessageBox.Show(this,
						"The address " + ip + " is already assigned to interface \"" + owner.InterfaceName + "\"."
						+ Environment.NewLine + Environment.NewLine
						+ "Assigning the same address to two interfaces will cause an address conflict, and Windows may mark the address as \"Duplicate\" and refuse to use it."
						+ Environment.NewLine + Environment.NewLine
						+ "Add it anyway?",
						"Address conflict", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
					if (proceed != DialogResult.Yes)
						return;
				}
				await Task.Run(() => NetshIpv4.AddStaticAddress(freshIface, ip, mask));
				txtNewAddress.Clear();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to add static IP", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				SetBusy(false);
				RefreshInterfaces();
			}
		}

		private async void btnDelete_Click(object sender, EventArgs e)
		{
			if (busy)
				return;
			Ipv4Interface iface = SelectedInterface;
			if (iface == null || listAddresses.SelectedItems.Count == 0)
			{
				MessageBox.Show(this, "Select an address in the list first.", "No address selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			Ipv4Address addr = (Ipv4Address)listAddresses.SelectedItems[0].Tag;
			if (!addr.IsStatic)
			{
				MessageBox.Show(this, "Only static addresses can be deleted with this tool.  DHCP-assigned addresses are managed by the DHCP server.", "Not a static address", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			DialogResult confirm = MessageBox.Show(this,
				"Delete static address " + addr.Ip + " from interface \"" + iface.InterfaceName + "\"?",
				"Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (confirm != DialogResult.Yes)
				return;
			SetBusy(true);
			try
			{
				await Task.Run(() => NetshIpv4.DeleteStaticAddress(iface.InterfaceName, addr.Ip));
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to delete static IP", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				SetBusy(false);
				RefreshInterfaces();
			}
		}
		#endregion

		#region Gateway Management
		private async void btnGwAdd_Click(object sender, EventArgs e)
		{
			if (busy)
				return;
			Ipv4Interface iface = SelectedInterface;
			if (iface == null)
				return;
			if (!IPAddress.TryParse(txtNewGateway.Text.Trim(), out IPAddress gateway) || gateway.AddressFamily != AddressFamily.InterNetwork)
			{
				MessageBox.Show(this, "Enter a valid IPv4 gateway address, e.g. \"192.168.1.1\".", "Invalid gateway input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			int? metric = null;
			string metricText = txtNewGwMetric.Text.Trim();
			if (metricText.Length > 0)
			{
				if (!int.TryParse(metricText, out int metricValue) || metricValue < 0 || metricValue > 9999)
				{
					MessageBox.Show(this, "Gateway metric must be a number from 0 to 9999, or blank for automatic.", "Invalid metric", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
				metric = metricValue;
			}
			if (iface.Gateways.Any(g => g.Ip.Equals(gateway)))
			{
				MessageBox.Show(this, "Gateway " + gateway + " is already configured on this interface.", "Duplicate gateway", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			SetBusy(true);
			try
			{
				await Task.Run(() => NetshIpv4.AddGateway(iface.InterfaceName, gateway, metric));
				txtNewGateway.Clear();
				txtNewGwMetric.Clear();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to add gateway", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				SetBusy(false);
				RefreshInterfaces();
			}
		}

		private async void btnGwRemove_Click(object sender, EventArgs e)
		{
			if (busy)
				return;
			Ipv4Interface iface = SelectedInterface;
			if (iface == null || listGateways.SelectedItems.Count == 0)
			{
				MessageBox.Show(this, "Select a gateway in the list first.", "No gateway selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			Ipv4Gateway gw = (Ipv4Gateway)listGateways.SelectedItems[0].Tag;
			if (!gw.IsStatic)
			{
				MessageBox.Show(this, "Only static gateways can be removed with this tool.  This gateway was configured dynamically (e.g. by DHCP or by VPN software such as ZeroTier).", "Not a static gateway", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			DialogResult confirm = MessageBox.Show(this,
				"Remove gateway " + gw.Ip + " from interface \"" + iface.InterfaceName + "\"?",
				"Confirm remove", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (confirm != DialogResult.Yes)
				return;
			SetBusy(true);
			try
			{
				await Task.Run(() => NetshIpv4.DeleteGateway(iface.InterfaceName, gw.Ip));
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to remove gateway", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				SetBusy(false);
				RefreshInterfaces();
			}
		}

		private void btnGwMoveUp_Click(object sender, EventArgs e)
		{
			MoveGateway(-1);
		}

		private void btnGwMoveDown_Click(object sender, EventArgs e)
		{
			MoveGateway(1);
		}

		private async void MoveGateway(int direction)
		{
			if (busy)
				return;
			Ipv4Interface iface = SelectedInterface;
			if (iface == null)
				return;
			if (!iface.Gateways.All(g => g.IsStatic))
			{
				MessageBox.Show(this, "Gateways can only be reordered when all of the interface's gateways are static.", "Cannot reorder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			int idx = listGateways.SelectedItems.Count > 0 ? listGateways.SelectedItems[0].Index : -1;
			if (idx < 0 || idx >= iface.Gateways.Count)
			{
				MessageBox.Show(this, "Select a gateway in the list first.", "No gateway selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			int newIdx = idx + direction;
			if (newIdx < 0 || newIdx >= iface.Gateways.Count)
				return;

			List<IPAddress> ordered = iface.Gateways.Select(g => g.Ip).ToList();
			IPAddress moved = ordered[idx];
			ordered[idx] = ordered[newIdx];
			ordered[newIdx] = moved;

			DialogResult confirm = MessageBox.Show(this,
				"Reordering rewrites all of this interface's gateways with explicit metrics (1, 2, 3, ...) so that gateways earlier in the list are preferred."
				+ Environment.NewLine + Environment.NewLine
				+ "New order: " + string.Join(", ", ordered.Select(ip => ip.ToString()))
				+ Environment.NewLine + Environment.NewLine
				+ "Connectivity may be interrupted for a moment.  Continue?",
				"Reorder gateways", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (confirm != DialogResult.Yes)
				return;

			SetBusy(true);
			try
			{
				await Task.Run(() => NetshIpv4.ReorderStaticGateways(iface.InterfaceName, ordered));
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to reorder gateways", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				SetBusy(false);
				RefreshInterfaces();
			}
		}
		#endregion

		#region DHCP Toggle
		private async void btnDhcpToggle_Click(object sender, EventArgs e)
		{
			if (busy)
				return;
			Ipv4Interface iface = SelectedInterface;
			if (iface == null)
				return;

			if (iface.DhcpEnabled)
				await DisableDhcp(iface);
			else
				await EnableDhcp(iface);
		}

		private async Task EnableDhcp(Ipv4Interface iface)
		{
			List<Ipv4Address> staticAddresses = iface.IpAddresses.Where(a => a.IsStatic).ToList();
			string prompt = "Enable DHCP on \"" + iface.InterfaceName + "\"?";
			if (staticAddresses.Count > 0)
			{
				prompt += Environment.NewLine + Environment.NewLine
					+ "Windows removes all static addresses when DHCP is enabled.  This tool will automatically re-add the following static address" + (staticAddresses.Count == 1 ? "" : "es") + " afterward:"
					+ Environment.NewLine
					+ string.Join(Environment.NewLine, staticAddresses.Select(a => "    " + FormatAddress(a)));
			}
			if (iface.Gateways.Any(g => g.IsStatic))
				prompt += Environment.NewLine + Environment.NewLine + "Static gateways will NOT be restored; the DHCP-provided gateway will be used instead.";
			if (!iface.DnsFromDhcp && iface.DnsServers.Count > 0)
				prompt += Environment.NewLine + Environment.NewLine + "The static DNS server configuration will remain in effect; DHCP-provided DNS servers will not be used.  (Use the \"Use DHCP DNS\" button to change this.)";
			if (MessageBox.Show(this, prompt, "Enable DHCP", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
				return;

			SetBusy(true);
			try
			{
				DhcpToggleResult result = await Task.Run(() => NetshIpv4.EnableDhcp(iface.InterfaceName));
				ReportDhcpToggleResult(result);
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to enable DHCP", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				SetBusy(false);
				RefreshInterfaces();
			}
		}

		private async Task DisableDhcp(Ipv4Interface iface)
		{
			// Disabling DHCP requires assigning a static primary address.  Prefer an existing static
			// address; otherwise offer to convert the current DHCP-assigned address to static;
			// otherwise use a valid address from the input box.
			IPAddress primaryIp;
			IPAddress primaryMask;
			string primaryDescription;

			Ipv4Address existingStatic = iface.IpAddresses.FirstOrDefault(a => a.IsStatic && a.Mask != null);
			if (existingStatic != null)
			{
				primaryIp = existingStatic.Ip;
				primaryMask = existingStatic.Mask;
				primaryDescription = "The existing static address " + FormatAddress(existingStatic) + " will become the interface's primary address.";
			}
			else
			{
				Ipv4Address dhcpAddress = iface.IpAddresses.FirstOrDefault(a => !a.IsStatic && a.Mask != null);
				if (dhcpAddress != null)
				{
					primaryIp = dhcpAddress.Ip;
					primaryMask = dhcpAddress.Mask;
					primaryDescription = "The current DHCP-assigned address " + FormatAddress(dhcpAddress) + " will be converted to a static address.";
				}
				else if (NetshIpv4.TryParseIpInput(txtNewAddress.Text, out primaryIp, out primaryMask) && primaryMask != null)
				{
					primaryDescription = "The address " + primaryIp + "/" + NetshIpv4.GetPrefixSizeOfMask(primaryMask) + " from the input box will be assigned as the interface's static address.";
				}
				else
				{
					MessageBox.Show(this,
						"Disabling DHCP requires a static address, but this interface has no addresses to keep."
						+ Environment.NewLine + Environment.NewLine
						+ "Type an address with prefix size (e.g. \"192.168.1.2/24\") into the address input box, then click \"Disable DHCP\" again.",
						"Static address required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
			}

			string prompt = "Disable DHCP on \"" + iface.InterfaceName + "\"?"
				+ Environment.NewLine + Environment.NewLine + primaryDescription;
			if (iface.DefaultGateway != null)
				prompt += Environment.NewLine + "The current default gateway (" + iface.DefaultGateway + ") will be preserved.";
			if (iface.DnsFromDhcp && iface.DnsServers.Count > 0)
				prompt += Environment.NewLine + "The current DNS server" + (iface.DnsServers.Count == 1 ? "" : "s") + " ("
					+ string.Join(", ", iface.DnsServers.Select(d => d.ToString()))
					+ ") will be preserved by converting " + (iface.DnsServers.Count == 1 ? "it" : "them") + " to static DNS configuration.";
			int otherStaticCount = iface.IpAddresses.Count(a => a.IsStatic && !a.Ip.Equals(primaryIp));
			if (otherStaticCount > 0)
				prompt += Environment.NewLine + "The other " + otherStaticCount + " static address" + (otherStaticCount == 1 ? "" : "es") + " will be re-added automatically if Windows removes them.";
			if (MessageBox.Show(this, prompt, "Disable DHCP", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
				return;

			IPAddress ip = primaryIp, mask = primaryMask;
			SetBusy(true);
			try
			{
				DhcpToggleResult result = await Task.Run(() => NetshIpv4.DisableDhcp(iface.InterfaceName, ip, mask));
				txtNewAddress.Clear();
				ReportDhcpToggleResult(result);
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to disable DHCP", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				SetBusy(false);
				RefreshInterfaces();
			}
		}

		private void ReportDhcpToggleResult(DhcpToggleResult result)
		{
			if (result.Errors.Count > 0)
			{
				string message = string.Join(Environment.NewLine + Environment.NewLine, result.Errors);
				List<string> restored = result.RestoredAddresses.Concat(result.RestoredGateways.Select(g => "gateway " + g)).ToList();
				if (restored.Count > 0)
					message += Environment.NewLine + Environment.NewLine + "Successfully restored: " + string.Join(", ", restored);
				MessageBox.Show(this, message, "Problems restoring static configuration", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private static string FormatAddress(Ipv4Address addr)
		{
			return addr.Mask != null ? addr.Ip + "/" + NetshIpv4.GetPrefixSizeOfMask(addr.Mask) : addr.Ip.ToString();
		}
		#endregion

		#region DNS Management
		private void btnDnsAdd_Click(object sender, EventArgs e)
		{
			if (busy)
				return;
			Ipv4Interface iface = SelectedInterface;
			if (iface == null)
				return;
			if (!IPAddress.TryParse(txtNewDns.Text.Trim(), out IPAddress dns) || dns.AddressFamily != AddressFamily.InterNetwork)
			{
				MessageBox.Show(this, "Enter a valid IPv4 DNS server address, e.g. \"8.8.8.8\".", "Invalid DNS server input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			List<IPAddress> newList;
			if (iface.DnsFromDhcp)
			{
				DialogResult proceed = MessageBox.Show(this,
					"Interface \"" + iface.InterfaceName + "\" currently obtains DNS servers automatically from DHCP."
					+ Environment.NewLine + Environment.NewLine
					+ "Adding a static DNS server will switch this interface to static DNS configuration, replacing the DHCP-provided servers with only the server you are adding."
					+ Environment.NewLine + Environment.NewLine
					+ "Continue?",
					"Switch to static DNS", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (proceed != DialogResult.Yes)
					return;
				newList = new List<IPAddress> { dns };
			}
			else
			{
				if (iface.DnsServers.Any(d => d.Equals(dns)))
				{
					MessageBox.Show(this, "DNS server " + dns + " is already in the list.", "Duplicate DNS server", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}
				newList = new List<IPAddress>(iface.DnsServers);
				newList.Add(dns);
			}
			ApplyDnsServers(iface, newList);
		}

		private void btnDnsRemove_Click(object sender, EventArgs e)
		{
			if (busy)
				return;
			Ipv4Interface iface = SelectedInterface;
			if (iface == null || iface.DnsFromDhcp)
				return;
			int idx = listDns.SelectedIndex;
			if (idx < 0 || idx >= iface.DnsServers.Count)
			{
				MessageBox.Show(this, "Select a DNS server in the list first.", "No DNS server selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			List<IPAddress> newList = new List<IPAddress>(iface.DnsServers);
			IPAddress removed = newList[idx];
			newList.RemoveAt(idx);
			if (newList.Count == 0)
			{
				DialogResult proceed = MessageBox.Show(this,
					"Remove the last static DNS server (" + removed + ")?"
					+ Environment.NewLine + Environment.NewLine
					+ "The interface will then have no DNS servers configured.  (Use the \"Use DHCP DNS\" button instead if you want DNS servers to be obtained automatically.)",
					"Remove last DNS server", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (proceed != DialogResult.Yes)
					return;
			}
			ApplyDnsServers(iface, newList);
		}

		private void btnDnsMoveUp_Click(object sender, EventArgs e)
		{
			MoveDnsServer(-1);
		}

		private void btnDnsMoveDown_Click(object sender, EventArgs e)
		{
			MoveDnsServer(1);
		}

		private void MoveDnsServer(int direction)
		{
			if (busy)
				return;
			Ipv4Interface iface = SelectedInterface;
			if (iface == null || iface.DnsFromDhcp)
				return;
			int idx = listDns.SelectedIndex;
			if (idx < 0 || idx >= iface.DnsServers.Count)
			{
				MessageBox.Show(this, "Select a DNS server in the list first.", "No DNS server selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			int newIdx = idx + direction;
			if (newIdx < 0 || newIdx >= iface.DnsServers.Count)
				return;
			List<IPAddress> newList = new List<IPAddress>(iface.DnsServers);
			IPAddress moved = newList[idx];
			newList[idx] = newList[newIdx];
			newList[newIdx] = moved;
			ApplyDnsServers(iface, newList, newIdx);
		}

		private async void btnDnsUseDhcp_Click(object sender, EventArgs e)
		{
			if (busy)
				return;
			Ipv4Interface iface = SelectedInterface;
			if (iface == null || iface.DnsFromDhcp)
				return;
			DialogResult proceed = MessageBox.Show(this,
				"Switch interface \"" + iface.InterfaceName + "\" to obtain DNS servers automatically from DHCP?"
				+ Environment.NewLine + Environment.NewLine
				+ "The static DNS server list will be discarded.",
				"Use DHCP DNS", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (proceed != DialogResult.Yes)
				return;
			SetBusy(true);
			try
			{
				await Task.Run(() => NetshIpv4.SetDnsServersDhcp(iface.InterfaceName));
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to set DNS source", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				SetBusy(false);
				RefreshInterfaces();
			}
		}

		/// <summary>
		/// Rewrites the interface's static DNS server list, then refreshes.
		/// If selectIndex is 0 or greater, that DNS list entry is re-selected after the refresh.
		/// </summary>
		private async void ApplyDnsServers(Ipv4Interface iface, List<IPAddress> servers, int selectIndex = -1)
		{
			SetBusy(true);
			try
			{
				await Task.Run(() => NetshIpv4.SetDnsServers(iface.InterfaceName, servers));
				txtNewDns.Clear();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to set DNS servers", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				SetBusy(false);
			}
			await RefreshAndReselectDns(selectIndex);
		}

		private async Task RefreshAndReselectDns(int selectIndex)
		{
			// RefreshInterfaces rebuilds the DNS list asynchronously; wait for it to finish before re-selecting.
			RefreshInterfaces();
			while (busy)
				await Task.Delay(50);
			if (selectIndex >= 0 && selectIndex < listDns.Items.Count)
				listDns.SelectedIndex = selectIndex;
		}
		#endregion
	}

	/// <summary>
	/// Diagnostic information about a network interface gathered from the OS
	/// (System.Net.NetworkInformation and the Network List Manager).
	/// </summary>
	public class InterfaceExtraInfo
	{
		/// <summary>Name of the network which owns this adapter (as seen in the Network Category tool), or null.</summary>
		public string NetworkName;
		/// <summary>UTC time the owning network was last connected, or DateTime.MinValue if unknown.</summary>
		public DateTime NetworkConnectedUtc = DateTime.MinValue;
		public OperationalStatus Status = OperationalStatus.Unknown;
		/// <summary>Link speed in bits per second, or -1 if unknown.</summary>
		public long SpeedBps = -1;
		public string Mac;
		public string Description;
	}
}
