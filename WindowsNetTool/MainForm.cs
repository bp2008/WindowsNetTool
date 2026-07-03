using System;
using System.Windows.Forms;
using WindowsNetTool.Tools.HostsFile;
using WindowsNetTool.Tools.IpConfig;
using WindowsNetTool.Tools.NetworkCategory;
using WindowsNetTool.Tools.Routes;
using WindowsNetTool.Tools.WindowsTools;

namespace WindowsNetTool
{
	public partial class MainForm : Form
	{
		private class ToolEntry
		{
			public string Name;
			public Func<UserControl> Factory;
			public UserControl Instance;
			public override string ToString()
			{
				return Name;
			}
		}

		public MainForm()
		{
			InitializeComponent();
			Text = "WindowsNetTool v" + Application.ProductVersion;

			AddTool("IP Configuration", () => new IpConfigTool());
			AddTool("Network Category", () => new NetworkCategoryTool());
			AddTool("Static Routes", () => new RoutesTool());
			AddTool("Hosts File Editor", () => new HostsFileTool());
			AddTool("Windows Tools", () => new WindowsToolsTool());

			if (listBoxTools.Items.Count > 0)
				listBoxTools.SelectedIndex = 0;
		}

		private void AddTool(string name, Func<UserControl> factory)
		{
			listBoxTools.Items.Add(new ToolEntry { Name = name, Factory = factory });
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			foreach (object listItem in listBoxTools.Items)
			{
				ToolEntry entry = listItem as ToolEntry;
				if (entry != null && entry.Instance is IHasUnsavedChanges dirtyTool && dirtyTool.HasUnsavedChanges)
				{
					DialogResult result = MessageBox.Show(this,
						"\"" + entry.Name + "\" has unsaved changes.  Exit anyway?",
						"Unsaved changes", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
					if (result != DialogResult.Yes)
					{
						e.Cancel = true;
						break;
					}
				}
			}
			base.OnFormClosing(e);
		}

		private void listBoxTools_SelectedIndexChanged(object sender, EventArgs e)
		{
			ToolEntry entry = listBoxTools.SelectedItem as ToolEntry;
			if (entry == null)
				return;
			bool created = false;
			if (entry.Instance == null)
			{
				entry.Instance = entry.Factory();
				entry.Instance.Dock = DockStyle.Fill;
				splitContainer.Panel2.Controls.Add(entry.Instance);
				created = true;
			}
			foreach (Control c in splitContainer.Panel2.Controls)
				c.Visible = c == entry.Instance;
			entry.Instance.BringToFront();
			// Tools load their data when first created; on later activations, tell them to reload
			// so they are not showing stale state (e.g. old interface names after a rename).
			if (!created && entry.Instance is IRefreshOnActivate refreshable)
				refreshable.RefreshOnActivate();
		}
	}
}
