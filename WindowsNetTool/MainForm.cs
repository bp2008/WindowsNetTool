using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WindowsNetTool.Tools.Arp;
using WindowsNetTool.Tools.DeviceList;
using WindowsNetTool.Tools.DnsLookup;
using WindowsNetTool.Tools.Export;
using WindowsNetTool.Tools.HostsFile;
using WindowsNetTool.Tools.IpConfig;
using WindowsNetTool.Tools.IpScanner;
using WindowsNetTool.Tools.Ndp;
using WindowsNetTool.Tools.NetworkCategory;
using WindowsNetTool.Tools.Ping;
using WindowsNetTool.Tools.Routes;
using WindowsNetTool.Tools.TcpTest;
using WindowsNetTool.Tools.Traceroute;
using WindowsNetTool.Tools.LinksShortcuts;

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
		/// <summary>
		/// Persistent application settings, loaded from disk on startup and saved on exit or when certain settings change.
		/// </summary>
		public static AppSettings settings { get; private set; }

		// Browser-style tool selection history, navigated with the mouse's back/forward
		// buttons (or a keyboard's Back/Forward media keys).  Selecting a tool by any other
		// means pushes the previous tool onto backStack and clears forwardStack, exactly
		// like following a link in a web browser.
		private readonly Stack<ToolEntry> backStack = new Stack<ToolEntry>();
		private readonly Stack<ToolEntry> forwardStack = new Stack<ToolEntry>();
		private ToolEntry currentToolEntry;
		private bool navigatingHistory;

		public MainForm()
		{
			InitializeComponent();
			Text = "WindowsNetTool v" + Application.ProductVersion;

			if (settings == null)
				settings = AppSettings.Load();
			ApplyWindowPlacement();

			// Load the window icon from the embedded multi-resolution .ico so the title bar
			// and taskbar each get a native-size image.
			using (System.IO.Stream iconStream = typeof(MainForm).Assembly.GetManifestResourceStream("WindowsNetTool.app-min.ico"))
			{
				if (iconStream != null)
					Icon = new Icon(iconStream);
			}

			splitContainer.Panel2.ClientSizeChanged += Panel2_ClientSizeChanged;

			// One Export button serves every tool; it exports whatever the active tool is showing
			// and is disabled while the active tool has nothing exportable.
			ExportMenu.Attach(btnExport, () => (activeTool as IExportableTool)?.BuildExportContent());

			AddTool<IpConfigTool>("IP Configuration");
			AddTool<NetworkCategoryTool>("Network Category");
			AddTool<RoutesTool>("Static Routes");
			AddTool<PingTool>("Ping");
			AddTool<TracerouteTool>("Traceroute");
			AddTool<DnsLookupTool>("DNS Lookup");
			AddTool<TcpTestTool>("TCP Connection Test");
			AddTool<ArpTool>("ARP Viewer");
			AddTool<NdpTool>("NDP Viewer");
			AddTool<IpScannerTool>("IP Scanner");
			AddTool<DeviceListTool>("Device List");
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
			if (entry != currentToolEntry)
			{
				if (!navigatingHistory)
				{
					if (currentToolEntry != null)
						backStack.Push(currentToolEntry);
					forwardStack.Clear();
				}
				currentToolEntry = entry;
			}
			bool created = false;
			if (entry.Instance == null)
			{
				entry.Instance = entry.Factory();
				splitContainer.Panel2.Controls.Add(entry.Instance);
				created = true;
			}
			activeTool = entry.Instance;
			btnExport.Enabled = activeTool is IExportableTool;
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

		private const int WM_APPCOMMAND = 0x0319;
		private const int APPCOMMAND_BROWSER_BACKWARD = 1;
		private const int APPCOMMAND_BROWSER_FORWARD = 2;

		/// <summary>
		/// Handles WM_APPCOMMAND to implement back/forward tool navigation.  Windows generates
		/// this message from mouse X-button clicks (and keyboard Back/Forward keys), and
		/// DefWindowProc bubbles it up the parent chain from whichever child control was
		/// clicked, so the form sees it no matter where the mouse is over the window.
		/// </summary>
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_APPCOMMAND)
			{
				// GET_APPCOMMAND_LPARAM: command in the high word, minus the device/key flags.
				int cmd = ((int)((long)m.LParam >> 16)) & 0x0FFF;
				if (cmd == APPCOMMAND_BROWSER_BACKWARD || cmd == APPCOMMAND_BROWSER_FORWARD)
				{
					NavigateHistory(back: cmd == APPCOMMAND_BROWSER_BACKWARD);
					m.Result = (IntPtr)1;
					return;
				}
			}
			base.WndProc(ref m);
		}

		private void NavigateHistory(bool back)
		{
			Stack<ToolEntry> from = back ? backStack : forwardStack;
			Stack<ToolEntry> to = back ? forwardStack : backStack;
			if (from.Count == 0)
				return;
			ToolEntry target = from.Pop();
			if (currentToolEntry != null)
				to.Push(currentToolEntry);
			navigatingHistory = true;
			try
			{
				listBoxTools.SelectedIndex = listBoxTools.Items.IndexOf(target);
			}
			finally
			{
				navigatingHistory = false;
			}
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
