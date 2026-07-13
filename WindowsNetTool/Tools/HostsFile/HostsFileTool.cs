using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WindowsNetTool.Tools.Export;

namespace WindowsNetTool.Tools.HostsFile
{
	/// <summary>
	/// A simple notepad-like editor for the system hosts file.
	/// </summary>
	public partial class HostsFileTool : UserControl, IRefreshOnActivate, IHasUnsavedChanges, IExportableTool
	{
		private static readonly string HostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers", "etc", "hosts");

		private string savedText = null;
		/// <summary>Encoding the hosts file was read with, so saving does not change the file's encoding.</summary>
		private Encoding fileEncoding = null;

		public bool HasUnsavedChanges
		{
			get { return savedText != null && txtHosts.Text != savedText; }
		}

		public HostsFileTool()
		{
			InitializeComponent();
			lblHostsPath.Text = HostsPath;
		}

		/// <summary>Builds the Export button's content: the editor content (including unsaved edits).</summary>
		public ExportableContent BuildExportContent()
		{
			ExportableContent content = new ExportableContent("Hosts File");
			content.AddText(HostsPath, txtHosts.Text);
			return content;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignMode)
				LoadHostsFile(false);
		}

		public void RefreshOnActivate()
		{
			// Pick up outside changes when the user switches back to this tool,
			// but never clobber unsaved edits.
			if (!HasUnsavedChanges)
				LoadHostsFile(true);
		}

		private void LoadHostsFile(bool silent)
		{
			try
			{
				string text;
				// Detect a BOM if present; otherwise assume the system ANSI code page,
				// matching how Notepad has classically treated the hosts file.
				using (StreamReader reader = new StreamReader(HostsPath, Encoding.Default, true))
				{
					text = reader.ReadToEnd();
					fileEncoding = reader.CurrentEncoding;
				}
				text = NormalizeLineEndings(text);
				txtHosts.Text = text;
				savedText = text;
				UpdateDirtyState();
			}
			catch (Exception ex)
			{
				if (!silent)
					MessageBox.Show(this, ex.Message, "Failed to read hosts file", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// Converts all line endings to CRLF, which the TextBox requires for display and which is
		/// conventional for the hosts file on Windows.
		/// </summary>
		private static string NormalizeLineEndings(string text)
		{
			return text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			try
			{
				if (SaveHostsFile())
					UpdateDirtyState();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to save hosts file", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// Writes the editor content to the hosts file, offering to clear the file's read-only
		/// attribute if that is what blocks the write.  Returns true if the file was saved.
		/// </summary>
		private bool SaveHostsFile()
		{
			string text = txtHosts.Text;
			Encoding encoding = fileEncoding ?? Encoding.Default;
			try
			{
				File.WriteAllText(HostsPath, text, encoding);
			}
			catch (UnauthorizedAccessException)
			{
				FileAttributes attributes = File.GetAttributes(HostsPath);
				if ((attributes & FileAttributes.ReadOnly) == 0)
					throw;
				DialogResult clear = MessageBox.Show(this,
					"The hosts file has the read-only attribute set.  Remove the attribute and save anyway?",
					"Hosts file is read-only", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (clear != DialogResult.Yes)
					return false;
				File.SetAttributes(HostsPath, attributes & ~FileAttributes.ReadOnly);
				File.WriteAllText(HostsPath, text, encoding);
			}
			savedText = text;
			return true;
		}

		private void btnReload_Click(object sender, EventArgs e)
		{
			if (HasUnsavedChanges)
			{
				DialogResult discard = MessageBox.Show(this,
					"Discard unsaved changes and reload the hosts file from disk?",
					"Unsaved changes", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (discard != DialogResult.Yes)
					return;
			}
			LoadHostsFile(false);
		}

		private void btnFlushDns_Click(object sender, EventArgs e)
		{
			try
			{
				using (Process p = new Process())
				{
					p.StartInfo.FileName = "ipconfig";
					p.StartInfo.Arguments = "/flushdns";
					p.StartInfo.UseShellExecute = false;
					p.StartInfo.CreateNoWindow = true;
					p.StartInfo.RedirectStandardOutput = true;
					p.Start();
					string output = p.StandardOutput.ReadToEnd();
					p.WaitForExit();
					MessageBox.Show(this, output.Trim(), "Flush DNS Cache", MessageBoxButtons.OK,
						p.ExitCode == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Error);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to flush DNS cache", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void txtHosts_TextChanged(object sender, EventArgs e)
		{
			UpdateDirtyState();
		}

		private void txtHosts_KeyDown(object sender, KeyEventArgs e)
		{
			// A multiline TextBox does not handle Ctrl+A natively.
			if (e.Control && e.KeyCode == Keys.A)
			{
				txtHosts.SelectAll();
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}

		private void UpdateDirtyState()
		{
			lblHostsPath.Text = HostsPath + (HasUnsavedChanges ? "   (unsaved changes)" : "");
		}
	}
}
