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
		private readonly AppSettings settings;

		public MainForm()
		{
			InitializeComponent();
			Text = "WindowsNetTool v" + Application.ProductVersion;

			settings = AppSettings.Load();
			ApplyWindowPlacement();

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
				listBoxTools.SelectedIndex = GetSavedToolIndex();
		}

		private int GetSavedToolIndex()
		{
			for (int i = 0; i < listBoxTools.Items.Count; i++)
				if (((ToolEntry)listBoxTools.Items[i]).ToolType.Name == settings.SelectedTool)
					return i;
			return 0;
		}

		/// <summary>
		/// Positions the window from saved settings, or centered on the display the mouse cursor
		/// is on when no placement has been saved yet (first launch).  The window is shrunk to
		/// fit the target screen's working area if needed (its MinimumSize permitting) and nudged
		/// the minimum amount required to be fully on-screen.  When the screen is too small to
		/// fit the window even at its minimum size, the top left corner (title bar) is kept
		/// on-screen so the window can still be moved and resized.
		/// </summary>
		private void ApplyWindowPlacement()
		{
			StartPosition = FormStartPosition.Manual;

			Rectangle wa;
			int x, y, width, height;
			if (settings.WindowWidth > 0 && settings.WindowHeight > 0)
			{
				Rectangle saved = new Rectangle(settings.WindowX, settings.WindowY, settings.WindowWidth, settings.WindowHeight);
				// The monitor layout may have changed since last run.  FromRectangle picks the
				// screen showing the largest part of the saved bounds, or the nearest screen if
				// the saved bounds are entirely off-screen.
				wa = Screen.FromRectangle(saved).WorkingArea;
				x = saved.X;
				y = saved.Y;
				width = saved.Width;
				height = saved.Height;
			}
			else
			{
				wa = Screen.FromPoint(Cursor.Position).WorkingArea;
				width = Width;
				height = Height;
				x = wa.X + (wa.Width - width) / 2;
				y = wa.Y + (wa.Height - height) / 2;
			}

			width = Math.Max(Math.Min(width, wa.Width), MinimumSize.Width);
			height = Math.Max(Math.Min(height, wa.Height), MinimumSize.Height);
			// The left/top clamps are applied last so that when the window cannot fit, it is the
			// right/bottom edges that hang off-screen.
			x = Math.Max(Math.Min(x, wa.Right - width), wa.X);
			y = Math.Max(Math.Min(y, wa.Bottom - height), wa.Y);
			Bounds = new Rectangle(x, y, width, height);

			if (settings.WindowMaximized)
				WindowState = FormWindowState.Maximized;
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

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			// When closing maximized or minimized, Bounds describes that transient state, so the
			// normal-state bounds are taken from RestoreBounds instead.
			Rectangle bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
			settings.WindowX = bounds.X;
			settings.WindowY = bounds.Y;
			settings.WindowWidth = bounds.Width;
			settings.WindowHeight = bounds.Height;
			settings.WindowMaximized = WindowState == FormWindowState.Maximized;
			ToolEntry selected = listBoxTools.SelectedItem as ToolEntry;
			settings.SelectedTool = selected == null ? null : selected.ToolType.Name;
			settings.Save();
			base.OnFormClosed(e);
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
