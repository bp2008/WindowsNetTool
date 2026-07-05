using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace WindowsNetTool.Tools.LinksShortcuts
{
	/// <summary>
	/// A launcher panel for Windows' built-in networking-related settings and control panel pages.
	/// </summary>
	public partial class LinksTool : UserControl
	{
		public LinksTool()
		{
			InitializeComponent();

			AddHeading("Control Panel");
			AddLauncher("Network Connections (ncpa.cpl)", NetworkPanels.OpenNetworkConnections);
			AddLauncher("Network and Sharing Center", NetworkPanels.OpenNetworkAndSharingCenter);
			AddLauncher("Internet Options (inetcpl.cpl)", NetworkPanels.OpenInternetOptions);
			AddLauncher("Windows Defender Firewall", NetworkPanels.OpenFirewallControlPanel);
			AddLauncher("Firewall Advanced Security (wf.msc)", NetworkPanels.OpenFirewallAdvancedSecurity);
			AddLauncher("Resource Monitor - Network (resmon.exe)", NetworkPanels.OpenResourceMonitor);

			AddHeading("Windows Settings");
			AddLauncher("Network Status", () => NetworkPanels.OpenSettingsUri("ms-settings:network-status"));
			AddLauncher("Ethernet", () => NetworkPanels.OpenSettingsUri("ms-settings:network-ethernet"));
			AddLauncher("Wi-Fi", () => NetworkPanels.OpenSettingsUri("ms-settings:network-wifi"));
			AddLauncher("VPN", () => NetworkPanels.OpenSettingsUri("ms-settings:network-vpn"));
			AddLauncher("Proxy", () => NetworkPanels.OpenSettingsUri("ms-settings:network-proxy"));
			AddLauncher("Advanced Network Settings", () => NetworkPanels.OpenSettingsUri("ms-settings:network-advancedsettings"));

			AddHeading("Web Resources");
			AddLauncher("WindowsNetTool", () => Process.Start("https://github.com/bp2008/WindowsNetTool"));
			AddLauncher("PingTracer", () => Process.Start("https://github.com/bp2008/PingTracer"));
		}

		private Button previousButton = null;
		private void AddHeading(string text)
		{
			if (previousButton != null)
				flowPanel.SetFlowBreak(previousButton, true);
			Label label = new Label();
			label.Text = text;
			label.Font = new System.Drawing.Font(Font, System.Drawing.FontStyle.Bold);
			label.AutoSize = true;
			label.Margin = new Padding(6, 14, 6, 2);
			flowPanel.Controls.Add(label);
			flowPanel.SetFlowBreak(label, true);
		}

		private void AddLauncher(string text, Action open)
		{
			Button button = previousButton = new Button();
			button.Text = text;
			button.Size = new System.Drawing.Size(260, 30);
			button.Margin = new Padding(6, 3, 6, 3);
			button.UseVisualStyleBackColor = true;
			button.Click += (sender, e) =>
			{
				try
				{
					open();
				}
				catch (Exception ex)
				{
					MessageBox.Show(this, "Failed to open \"" + text + "\": " + ex.Message, "Launch failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			};
			flowPanel.Controls.Add(button);
		}
	}
}
