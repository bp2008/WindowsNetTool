using System;
using System.Windows.Forms;
using WindowsNetTool.Tools.NetworkCategory;
using WindowsNetTool.Tools.StaticIp;

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

			AddTool("Static IP Manager", () => new StaticIpTool());
			AddTool("Network Category", () => new NetworkCategoryTool());

			if (listBoxTools.Items.Count > 0)
				listBoxTools.SelectedIndex = 0;
		}

		private void AddTool(string name, Func<UserControl> factory)
		{
			listBoxTools.Items.Add(new ToolEntry { Name = name, Factory = factory });
		}

		private void listBoxTools_SelectedIndexChanged(object sender, EventArgs e)
		{
			ToolEntry entry = listBoxTools.SelectedItem as ToolEntry;
			if (entry == null)
				return;
			if (entry.Instance == null)
			{
				entry.Instance = entry.Factory();
				entry.Instance.Dock = DockStyle.Fill;
				splitContainer.Panel2.Controls.Add(entry.Instance);
			}
			foreach (Control c in splitContainer.Panel2.Controls)
				c.Visible = c == entry.Instance;
			entry.Instance.BringToFront();
		}
	}
}
