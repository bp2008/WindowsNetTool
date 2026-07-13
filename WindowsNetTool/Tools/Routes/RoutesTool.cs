using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsNetTool.Tools.Export;

namespace WindowsNetTool.Tools.Routes
{
	/// <summary>
	/// Manages persistent IPv4 static routes (routes which survive reboots).
	/// </summary>
	public partial class RoutesTool : UserControl, IRefreshOnActivate, IExportableTool
	{
		private bool busy = false;

		public RoutesTool()
		{
			InitializeComponent();
		}

		/// <summary>Builds the Export button's content: the persistent static route table.</summary>
		public ExportableContent BuildExportContent()
		{
			ExportableContent content = new ExportableContent("Static Routes");
			content.AddListView(null, listRoutes);
			return content;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignMode)
				RefreshRoutes();
		}

		public void RefreshOnActivate()
		{
			RefreshRoutes();
		}

		private async void RefreshRoutes()
		{
			if (busy)
				return;
			SetBusy(true);
			try
			{
				List<StaticRoute> routes = await Task.Run(() => NetshRoutes.GetPersistentRoutes());

				listRoutes.BeginUpdate();
				listRoutes.Items.Clear();
				foreach (StaticRoute route in routes)
				{
					ListViewItem item = new ListViewItem(new string[]
					{
						route.Prefix,
						route.InterfaceName,
						route.NextHop != null ? route.NextHop.ToString() : "(on-link)",
						route.Metric.HasValue ? route.Metric.Value.ToString() : "automatic"
					});
					item.Tag = route;
					listRoutes.Items.Add(item);
				}
				listRoutes.EndUpdate();

				// Refresh the interface choices for the add-route form, preserving the selection.
				string previousInterface = comboRouteInterface.SelectedItem as string;
				comboRouteInterface.Items.Clear();
				foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces().OrderBy(n => n.Name))
					comboRouteInterface.Items.Add(nic.Name);
				if (previousInterface != null && comboRouteInterface.Items.Contains(previousInterface))
					comboRouteInterface.SelectedItem = previousInterface;
				else if (comboRouteInterface.Items.Count > 0)
					comboRouteInterface.SelectedIndex = 0;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to read routes", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				SetBusy(false);
			}
		}

		private void SetBusy(bool value)
		{
			busy = value;
			listRoutes.Enabled = !value;
			txtRoutePrefix.Enabled = !value;
			comboRouteInterface.Enabled = !value;
			txtRouteNextHop.Enabled = !value;
			txtRouteMetric.Enabled = !value;
			btnAddRoute.Enabled = !value;
			btnRefreshRoutes.Enabled = !value;
			btnDeleteRoute.Enabled = !value;
			Cursor = value ? Cursors.WaitCursor : Cursors.Default;
		}

		private void btnRefreshRoutes_Click(object sender, EventArgs e)
		{
			RefreshRoutes();
		}

		private async void btnAddRoute_Click(object sender, EventArgs e)
		{
			if (busy)
				return;
			if (!NetshRoutes.TryParsePrefix(txtRoutePrefix.Text, out string prefix))
			{
				MessageBox.Show(this, "Enter a destination prefix in CIDR form, e.g. \"10.0.0.0/8\".  A bare address is treated as a /32 host route.", "Invalid prefix", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			string interfaceName = comboRouteInterface.SelectedItem as string;
			if (interfaceName == null)
			{
				MessageBox.Show(this, "Select an interface for the route.", "No interface selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			IPAddress nextHop = null;
			string nextHopText = txtRouteNextHop.Text.Trim();
			if (nextHopText.Length > 0)
			{
				if (!IPAddress.TryParse(nextHopText, out nextHop) || nextHop.AddressFamily != AddressFamily.InterNetwork)
				{
					MessageBox.Show(this, "Enter a valid IPv4 next hop address, or leave it blank for an on-link route.", "Invalid next hop", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
			}
			int? metric = null;
			string metricText = txtRouteMetric.Text.Trim();
			if (metricText.Length > 0)
			{
				if (!int.TryParse(metricText, out int metricValue) || metricValue < 0 || metricValue > 9999)
				{
					MessageBox.Show(this, "Metric must be a number from 0 to 9999, or blank for automatic.", "Invalid metric", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
				metric = metricValue;
			}

			foreach (ListViewItem item in listRoutes.Items)
			{
				StaticRoute existing = (StaticRoute)item.Tag;
				if (existing.Prefix == prefix && existing.InterfaceName == interfaceName && Equals(existing.NextHop, nextHop))
				{
					MessageBox.Show(this, "An identical route already exists.", "Duplicate route", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}
			}

			StaticRoute route = new StaticRoute
			{
				Prefix = prefix,
				InterfaceName = interfaceName,
				NextHop = nextHop,
				Metric = metric
			};
			SetBusy(true);
			try
			{
				await Task.Run(() => NetshRoutes.AddRoute(route));
				txtRoutePrefix.Clear();
				txtRouteNextHop.Clear();
				txtRouteMetric.Clear();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to add route", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				SetBusy(false);
				RefreshRoutes();
			}
		}

		private async void btnDeleteRoute_Click(object sender, EventArgs e)
		{
			if (busy)
				return;
			if (listRoutes.SelectedItems.Count == 0)
			{
				MessageBox.Show(this, "Select a route in the list first.", "No route selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			StaticRoute route = (StaticRoute)listRoutes.SelectedItems[0].Tag;
			DialogResult confirm = MessageBox.Show(this,
				"Delete route " + route.Prefix + " via " + (route.NextHop != null ? route.NextHop.ToString() : "(on-link)") + " on interface \"" + route.InterfaceName + "\"?",
				"Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (confirm != DialogResult.Yes)
				return;
			SetBusy(true);
			try
			{
				await Task.Run(() => NetshRoutes.DeleteRoute(route));
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to delete route", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				SetBusy(false);
				RefreshRoutes();
			}
		}
	}
}
