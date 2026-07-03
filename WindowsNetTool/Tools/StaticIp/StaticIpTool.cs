using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsNetTool.Tools.StaticIp
{
	public partial class StaticIpTool : UserControl
	{
		private bool busy = false;

		public StaticIpTool()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignMode)
				RefreshInterfaces();
		}

		private async void RefreshInterfaces()
		{
			if (busy)
				return;
			SetBusy(true);
			try
			{
				ScanResult result = await Task.Run(() => NetshIpv4.Scan());

				string previousSelection = (comboInterfaces.SelectedItem as Ipv4Interface)?.InterfaceName;
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

		private void UpdateInterfaceDetails()
		{
			listAddresses.Items.Clear();
			Ipv4Interface iface = comboInterfaces.SelectedItem as Ipv4Interface;
			if (iface == null)
			{
				lblInterfaceInfo.Text = "No interface selected.";
				return;
			}
			lblInterfaceInfo.Text = "Default Gateway: " + (iface.DefaultGateway != null ? iface.DefaultGateway.ToString() : "(none)")
				+ "      DHCP: " + (iface.DhcpEnabled ? "enabled" : "disabled")
				+ "      DHCP+Static coexistence: " + (iface.DhcpStaticIpCoexistence ? "enabled" : "disabled");
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
		}

		private void SetBusy(bool value)
		{
			busy = value;
			comboInterfaces.Enabled = !value;
			btnRefresh.Enabled = !value;
			btnAdd.Enabled = !value;
			btnDelete.Enabled = !value;
			txtNewAddress.Enabled = !value;
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

		private async void btnAdd_Click(object sender, EventArgs e)
		{
			if (busy)
				return;
			Ipv4Interface iface = comboInterfaces.SelectedItem as Ipv4Interface;
			if (iface == null)
			{
				MessageBox.Show(this, "Select an interface first.", "No interface selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			if (!NetshIpv4.TryParseIpInput(txtNewAddress.Text, out IPAddress ip, out IPAddress mask))
			{
				MessageBox.Show(this, "Enter a static IPv4 address with subnet prefix size, e.g. \"192.168.1.2/24\".", "Invalid address input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			SetBusy(true);
			try
			{
				await Task.Run(() => NetshIpv4.AddStaticAddress(iface, ip, mask));
				txtNewAddress.Clear();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to add static IP", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				SetBusy(false);
			}
			RefreshInterfaces();
		}

		private async void btnDelete_Click(object sender, EventArgs e)
		{
			if (busy)
				return;
			Ipv4Interface iface = comboInterfaces.SelectedItem as Ipv4Interface;
			if (iface == null || listAddresses.SelectedItems.Count == 0)
			{
				MessageBox.Show(this, "Select an address in the list first.", "No address selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			Ipv4Address addr = (Ipv4Address)listAddresses.SelectedItems[0].Tag;
			if (!addr.IsStatic)
			{
				MessageBox.Show(this, "Only static addresses can be deleted with this tool.", "Not a static address", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
			}
			RefreshInterfaces();
		}
	}
}
