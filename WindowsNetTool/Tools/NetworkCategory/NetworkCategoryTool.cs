using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WindowsNetTool.Tools.Export;

namespace WindowsNetTool.Tools.NetworkCategory
{
	public partial class NetworkCategoryTool : UserControl, IRefreshOnActivate, IExportableTool
	{
		public NetworkCategoryTool()
		{
			InitializeComponent();
		}

		/// <summary>Builds the Export button's content: the list of connected networks.</summary>
		public ExportableContent BuildExportContent()
		{
			ExportableContent content = new ExportableContent("Network Category");
			content.AddListView(null, listNetworks);
			return content;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignMode)
				RefreshNetworks();
		}

		public void RefreshOnActivate()
		{
			RefreshNetworks();
		}

		private void RefreshNetworks()
		{
			try
			{
				List<NetworkInfo> networks = NetworkCategoryService.GetConnectedNetworks();

				// Give duplicate network names a numeric suffix so list entries are distinguishable.
				Dictionary<string, NetworkInfo> networkMap = new Dictionary<string, NetworkInfo>();
				foreach (NetworkInfo network in networks)
				{
					string name = network.Name;
					int counter = 2;
					while (networkMap.ContainsKey(name))
					{
						name = network.Name + " (" + counter + ")";
						counter++;
					}
					networkMap[name] = network;
				}

				listNetworks.BeginUpdate();
				listNetworks.Items.Clear();
				foreach (KeyValuePair<string, NetworkInfo> pair in networkMap.OrderBy(p => p.Key))
				{
					ListViewItem item = new ListViewItem(new string[]
					{
						pair.Key,
						NetworkCategoryService.GetCategoryText(pair.Value.Category),
						pair.Value.IsConnectedToInternet ? "yes" : ""
					});
					item.Tag = pair.Value;
					listNetworks.Items.Add(item);
				}
				listNetworks.EndUpdate();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to list networks", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void SetCheckedNetworks(NlmNetworkCategory category)
		{
			string catText = NetworkCategoryService.GetCategoryText(category);
			List<ListViewItem> targets = listNetworks.CheckedItems.Cast<ListViewItem>().ToList();
			if (targets.Count == 0)
			{
				MessageBox.Show(this, "Check one or more networks in the list first.", "No networks checked", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			List<string> errors = new List<string>();
			foreach (ListViewItem item in targets)
			{
				NetworkInfo network = (NetworkInfo)item.Tag;
				try
				{
					network.SetCategory(category);
				}
				catch (Exception ex)
				{
					errors.Add("Failed to set network category of network \"" + item.Text + "\" to " + catText + ": " + ex.Message);
				}
			}
			RefreshNetworks();
			if (errors.Count > 0)
				MessageBox.Show(this, string.Join(Environment.NewLine + Environment.NewLine, errors), "Errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		private void btnRenameNetwork_Click(object sender, EventArgs e)
		{
			if (listNetworks.SelectedItems.Count == 0)
			{
				MessageBox.Show(this, "Select (highlight) a network row in the list first.", "No network selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			ListViewItem item = listNetworks.SelectedItems[0];
			NetworkInfo network = (NetworkInfo)item.Tag;
			string newName = TextPromptDialog.Show(FindForm(), "Rename Network", "New name for network \"" + network.Name + "\":", network.Name);
			if (newName == null)
				return;
			newName = newName.Trim();
			if (newName.Length == 0 || newName == network.Name)
				return;
			try
			{
				network.Rename(newName);
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, "Failed to rename network: " + ex.Message, "Rename failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			RefreshNetworks();
		}

		private void btnCheckAll_Click(object sender, EventArgs e)
		{
			bool allChecked = listNetworks.Items.Count > 0 && listNetworks.CheckedItems.Count == listNetworks.Items.Count;
			foreach (ListViewItem item in listNetworks.Items)
				item.Checked = !allChecked;
		}

		private void btnRefresh_Click(object sender, EventArgs e)
		{
			RefreshNetworks();
		}

		private void btnPublic_Click(object sender, EventArgs e)
		{
			SetCheckedNetworks(NlmNetworkCategory.Public);
		}

		private void btnPrivate_Click(object sender, EventArgs e)
		{
			SetCheckedNetworks(NlmNetworkCategory.Private);
		}

		private void btnDomain_Click(object sender, EventArgs e)
		{
			SetCheckedNetworks(NlmNetworkCategory.DomainAuthenticated);
		}
	}
}
