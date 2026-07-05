using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml;

namespace WindowsNetTool.Tools.LinksShortcuts
{
	/// <summary>
	/// Opens Windows' built-in networking-related settings panels and dialogs.
	/// </summary>
	public static class NetworkPanels
	{
		/// <summary>GUID of the Network Connections shell folder (the ncpa.cpl view).</summary>
		private const string NetworkConnectionsFolder = "::{7007ACC7-3202-11D1-AAD2-00805FC1270E}";

		public static void OpenNetworkConnections()
		{
			Start("control.exe", "ncpa.cpl");
		}

		public static void OpenNetworkAndSharingCenter()
		{
			Start("control.exe", "/name Microsoft.NetworkAndSharingCenter");
		}

		public static void OpenInternetOptions()
		{
			Start("control.exe", "inetcpl.cpl");
		}

		public static void OpenFirewallControlPanel()
		{
			Start("control.exe", "/name Microsoft.WindowsFirewall");
		}

		public static void OpenFirewallAdvancedSecurity()
		{
			Start("wf.msc", null);
		}

		/// <summary>
		/// Opens a page of the Windows Settings app, e.g. "ms-settings:network-status".
		/// </summary>
		public static void OpenSettingsUri(string uri)
		{
			Start(uri, null);
		}

		/// <summary>
		/// Opens Windows' Resource Monitor on its Network tab.  Resmon opens on whichever tab was
		/// focused when it last closed, a preference it stores in %LocalAppData%\Resmon.ResmonCfg,
		/// so this best-effort edits that file before launching.  The tab ids in the file are
		/// locale-independent.  If resmon is already running, launching it again just activates the
		/// existing window and the file edit has no effect (resmon overwrites the file on close).
		/// </summary>
		public static void OpenResourceMonitor()
		{
			try
			{
				SetResourceMonitorStartupTab("Network");
			}
			catch (Exception)
			{
				// Best-effort only: if the config file can't be edited, Resource Monitor just
				// opens on whatever tab it last remembered instead of the Network tab.
			}
			Start("resmon.exe", null);
		}

		/// <summary>
		/// Rewrites %LocalAppData%\Resmon.ResmonCfg so the tab with the given locale-independent id
		/// (e.g. "Network") is the focused one.  Does nothing if the file does not exist yet (first
		/// ever launch) because resmon's tolerance of a minimal config file is unknown.
		/// </summary>
		private static void SetResourceMonitorStartupTab(string tabId)
		{
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Resmon.ResmonCfg");
			if (!File.Exists(path))
				return;

			XmlDocument doc = new XmlDocument();
			doc.Load(path);

			XmlElement target = null;
			foreach (XmlElement tab in doc.DocumentElement.SelectNodes("tab"))
			{
				tab.RemoveAttribute("focused");
				if (string.Equals(tab.GetAttribute("id"), tabId, StringComparison.OrdinalIgnoreCase))
					target = tab;
			}
			if (target == null)
				return;
			target.SetAttribute("focused", "true");

			// Resmon writes the file as UTF-8 without a byte order mark; match that.
			using (XmlWriter writer = XmlWriter.Create(path, new XmlWriterSettings { Encoding = new UTF8Encoding(false) }))
				doc.Save(writer);
		}

		private static void Start(string fileName, string arguments)
		{
			ProcessStartInfo psi = new ProcessStartInfo();
			psi.FileName = fileName;
			if (arguments != null)
				psi.Arguments = arguments;
			psi.UseShellExecute = true;
			Process.Start(psi);
		}

		/// <summary>
		/// Opens Windows' native "Status" dialog for the named network connection (the same dialog
		/// reached by right-clicking a connection in ncpa.cpl), which shows duration, speed, and has
		/// a Details button.  Returns false if the connection or its Status verb could not be found
		/// (e.g. the connection is disconnected, or Windows is not in English).
		/// Must be called from an STA thread (e.g. the UI thread) because it uses the Windows Shell.
		/// </summary>
		public static bool OpenConnectionStatus(string connectionName)
		{
			return InvokeConnectionVerb(connectionName, "Status");
		}

		/// <summary>
		/// Opens Windows' native adapter "Properties" dialog for the named network connection.
		/// Returns false if the connection or its Properties verb could not be found.
		/// Must be called from an STA thread.
		/// </summary>
		public static bool OpenConnectionProperties(string connectionName)
		{
			return InvokeConnectionVerb(connectionName, "Properties");
		}

		private static bool InvokeConnectionVerb(string connectionName, string verbName)
		{
			// The Network Connections shell folder caches connection names per process, so after a
			// connection is renamed, item Names here are stale until the program restarts.  The
			// folder's "Device Name" column (the adapter description, e.g. "Realtek Gaming 2.5GbE
			// Family Controller #2") does not change when a connection is renamed and is unique per
			// adapter, so items are matched primarily by device name, falling back to display name.
			// NetworkInterface data comes fresh from the OS on every call.
			string deviceName = null;
			foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (string.Equals(nic.Name, connectionName, StringComparison.OrdinalIgnoreCase))
				{
					deviceName = nic.Description;
					break;
				}
			}

			Type shellType = Type.GetTypeFromProgID("Shell.Application");
			if (shellType == null)
				return false;
			dynamic shell = Activator.CreateInstance(shellType);
			dynamic folder = shell.NameSpace(NetworkConnectionsFolder);
			if (folder == null)
				return false;

			int deviceNameColumn = -1;
			if (deviceName != null)
			{
				for (int i = 0; i < 20; i++)
				{
					string header = null;
					try
					{
						header = (string)folder.GetDetailsOf(null, i);
					}
					catch (Exception) { }
					if (string.Equals(header, "Device Name", StringComparison.OrdinalIgnoreCase))
					{
						deviceNameColumn = i;
						break;
					}
				}
			}

			dynamic match = null;
			foreach (dynamic item in folder.Items())
			{
				bool isMatch = false;
				if (deviceName != null && deviceNameColumn != -1)
				{
					string itemDeviceName = null;
					try
					{
						itemDeviceName = (string)folder.GetDetailsOf(item, deviceNameColumn);
					}
					catch (Exception) { }
					if (string.Equals(itemDeviceName, deviceName, StringComparison.OrdinalIgnoreCase))
						isMatch = true;
				}
				if (!isMatch && string.Equals((string)item.Name, connectionName, StringComparison.OrdinalIgnoreCase))
					isMatch = true;
				if (isMatch)
				{
					match = item;
					break;
				}
			}
			if (match == null)
				return false;
			foreach (dynamic verb in match.Verbs())
			{
				string name = ((string)verb.Name).Replace("&", "");
				if (string.Equals(name, verbName, StringComparison.OrdinalIgnoreCase))
				{
					verb.DoIt();
					return true;
				}
			}
			return false;
		}
	}
}
