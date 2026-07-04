using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsNetTool.Tools.DnsLookup;
using WindowsNetTool.Tools.HostsFile;
using WindowsNetTool.Tools.IpConfig;
using WindowsNetTool.Tools.NetworkCategory;
using WindowsNetTool.Tools.Ping;
using WindowsNetTool.Tools.Routes;
using WindowsNetTool.Tools.WindowsTools;

namespace WindowsNetTool
{
	public partial class MainForm : Form
	{
		private class ToolEntry
		{
			public string Name;
			public Type ToolType;
			public Func<UserControl> Factory;
			public UserControl Instance;
			public override string ToString()
			{
				return Name;
			}
		}

		private UserControl activeTool;

		public MainForm()
		{
			InitializeComponent();
			Text = "WindowsNetTool v" + Application.ProductVersion;

			// Load the window icon from the embedded multi-resolution .ico so the title bar
			// and taskbar each get a native-size image.
			using (System.IO.Stream iconStream = typeof(MainForm).Assembly.GetManifestResourceStream("WindowsNetTool.app.ico"))
			{
				if (iconStream != null)
					Icon = new Icon(iconStream);
			}

			splitContainer.Panel2.ClientSizeChanged += Panel2_ClientSizeChanged;

			AddTool<IpConfigTool>("IP Configuration");
			AddTool<NetworkCategoryTool>("Network Category");
			AddTool<RoutesTool>("Static Routes");
			AddTool<PingTool>("Ping");
			AddTool<DnsLookupTool>("DNS Lookup");
			AddTool<HostsFileTool>("Hosts File Editor");
			AddTool<LinksTool>("Links / Shortcuts");

			if (listBoxTools.Items.Count > 0)
				listBoxTools.SelectedIndex = 0;
		}

		private void AddTool<T>(string name) where T : UserControl, new()
		{
			listBoxTools.Items.Add(new ToolEntry { Name = name, ToolType = typeof(T), Factory = () => new T() });
		}

		/// <summary>
		/// Selects the tool of the given type in the tool list (creating it upon first activation)
		/// and returns its instance, or null if no such tool is registered.  This lets tools link
		/// into each other, e.g. an address list could offer one-click ping monitoring via
		/// ((MainForm)FindForm()).ActivateTool&lt;PingTool&gt;().StartPing(address);
		/// </summary>
		public T ActivateTool<T>() where T : UserControl
		{
			for (int i = 0; i < listBoxTools.Items.Count; i++)
			{
				ToolEntry entry = (ToolEntry)listBoxTools.Items[i];
				if (entry.ToolType == typeof(T))
				{
					listBoxTools.SelectedIndex = i;
					return entry.Instance as T;
				}
			}
			return null;
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
				splitContainer.Panel2.Controls.Add(entry.Instance);
				created = true;
			}
			activeTool = entry.Instance;
			foreach (Control c in splitContainer.Panel2.Controls)
				c.Visible = c == entry.Instance;
			splitContainer.Panel2.AutoScrollPosition = Point.Empty;
			LayoutActiveTool();
			entry.Instance.BringToFront();
			// Tools load their data when first created; on later activations, tell them to reload
			// so they are not showing stale state (e.g. old interface names after a rename).
			if (!created && entry.Instance is IRefreshOnActivate refreshable)
				refreshable.RefreshOnActivate();
		}

		private void Panel2_ClientSizeChanged(object sender, EventArgs e)
		{
			LayoutActiveTool();
		}

		// Docked controls do not trigger Panel2's AutoScroll, so the active tool is laid out
		// manually: it fills the panel's client area but never shrinks below its MinimumSize,
		// which lets AutoScroll show scroll bars when the panel is too small.
		private void LayoutActiveTool()
		{
			if (activeTool == null)
				return;
			SplitterPanel panel = splitContainer.Panel2;
			Size size = new Size(
				Math.Max(panel.ClientSize.Width, activeTool.MinimumSize.Width),
				Math.Max(panel.ClientSize.Height, activeTool.MinimumSize.Height));
			// AutoScrollPosition is negative while scrolled; using it as the location keeps the
			// tool aligned with the panel's scrolled origin instead of snapping back to (0,0).
			activeTool.Bounds = new Rectangle(panel.AutoScrollPosition, size);
		}
	}
}
