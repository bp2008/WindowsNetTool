using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;

namespace WindowsNetTool
{
	/// <summary>
	/// Persistent application settings, stored as JSON in %AppData%\WindowsNetTool\Settings.json.
	/// Serialized with DataContractJsonSerializer so no third-party JSON library is needed.
	/// </summary>
	public class AppSettings
	{
		// DataContractJsonSerializer creates instances without running constructors or field
		// initializers, so every field's default must be its type's zero value (0/false/null).

		// Last known window placement, in physical screen pixels.  When the window was closed
		// maximized or minimized, these hold the restored (normal-state) bounds.
		// WindowWidth <= 0 means placement has never been saved (first launch).
		public int WindowX;
		public int WindowY;
		public int WindowWidth;
		public int WindowHeight;
		public bool WindowMaximized;

		/// <summary>Type name of the most recently selected tool (e.g. "PingTool").</summary>
		public string SelectedTool;

		/// <summary>
		/// Returns the path of the program's data directory (%AppData%\WindowsNetTool),
		/// creating the directory if it does not exist yet.  Tools that need to store
		/// persistent files of their own should place them under this directory.
		/// </summary>
		public static string GetAppDataDirectory()
		{
			string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WindowsNetTool");
			Directory.CreateDirectory(dir);
			return dir;
		}

		private static string GetSettingsFilePath()
		{
			return Path.Combine(GetAppDataDirectory(), "Settings.json");
		}

		/// <summary>
		/// Loads settings from disk, returning defaults if the file does not exist or cannot
		/// be read (e.g. corrupt JSON) so a bad settings file can never prevent startup.
		/// </summary>
		public static AppSettings Load()
		{
			try
			{
				string path = GetSettingsFilePath();
				if (File.Exists(path))
				{
					using (FileStream fs = File.OpenRead(path))
						return (AppSettings)new DataContractJsonSerializer(typeof(AppSettings)).ReadObject(fs);
				}
			}
			catch { }
			return new AppSettings();
		}

		/// <summary>
		/// Saves settings to disk as indented JSON.  Failures are ignored because settings are
		/// non-critical and this typically runs during application shutdown.
		/// </summary>
		public void Save()
		{
			try
			{
				using (MemoryStream ms = new MemoryStream())
				{
					using (XmlDictionaryWriter writer = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.UTF8, false, true, "\t"))
						new DataContractJsonSerializer(typeof(AppSettings)).WriteObject(writer, this);
					File.WriteAllBytes(GetSettingsFilePath(), ms.ToArray());
				}
			}
			catch { }
		}
	}
}
