using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace WindowsNetTool.Tools.Export
{
	public enum ExportDestination
	{
		File,
		Clipboard,
		Email
	}

	/// <summary>
	/// Gives the Export button its dropdown menu, offering the active tool's content as Text, HTML,
	/// or CSV, delivered to a file or the clipboard, or announced in a new email message.  The
	/// content is produced on demand by a callback, so each use exports exactly what the tool is
	/// showing at that moment.
	/// </summary>
	public static class ExportMenu
	{
		private static readonly Encoding Utf8Bom = new UTF8Encoding(true);

		/// <summary>
		/// Wires the button to show the export menu when clicked.  <paramref name="contentProvider"/>
		/// is invoked each time the user picks a menu item.
		/// </summary>
		public static void Attach(Button button, Func<ExportableContent> contentProvider)
		{
			ContextMenuStrip menu = new ContextMenuStrip();
			menu.Items.Add(BuildFormatItem(button, contentProvider, "Save to File...", ExportDestination.File));
			menu.Items.Add(BuildFormatItem(button, contentProvider, "Copy to Clipboard", ExportDestination.Clipboard));
			// Email offers no format choice: a mailto: link can carry neither an attachment nor an
			// HTML body, so email always delivers plain text via the clipboard (see SendByEmail).
			menu.Items.Add("Send by Email...", null, (sender, e) => Execute(button, contentProvider, ExportFormat.Text, ExportDestination.Email));
			button.Click += (sender, e) => menu.Show(button, new Point(0, button.Height));
			button.Disposed += (sender, e) => menu.Dispose();
		}

		private static ToolStripMenuItem BuildFormatItem(Button button, Func<ExportableContent> contentProvider, string caption, ExportDestination destination)
		{
			ToolStripMenuItem item = new ToolStripMenuItem(caption);
			foreach (ExportFormat format in new ExportFormat[] { ExportFormat.Text, ExportFormat.Html, ExportFormat.Csv })
			{
				ExportFormat chosenFormat = format;
				string label = format == ExportFormat.Text ? "Text" : format.ToString().ToUpperInvariant();
				item.DropDownItems.Add(label, null, (sender, e) => Execute(button, contentProvider, chosenFormat, destination));
			}
			return item;
		}

		private static void Execute(Control owner, Func<ExportableContent> contentProvider, ExportFormat format, ExportDestination destination)
		{
			try
			{
				ExportableContent content = contentProvider();
				if (content == null || content.IsEmpty)
				{
					MessageBox.Show(owner, "There is nothing to export yet.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}
				switch (destination)
				{
					case ExportDestination.File: SaveToFile(owner, content, format); break;
					case ExportDestination.Clipboard: CopyToClipboard(content, format); break;
					case ExportDestination.Email: SendByEmail(content); break;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(owner, ex.Message, "Export failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#region Destinations

		private static void SaveToFile(Control owner, ExportableContent content, ExportFormat format)
		{
			using (SaveFileDialog dialog = new SaveFileDialog())
			{
				dialog.Title = "Save " + content.Title;
				dialog.FileName = DefaultFileName(content, format);
				switch (format)
				{
					case ExportFormat.Html: dialog.Filter = "HTML files (*.html)|*.html|All files (*.*)|*.*"; break;
					case ExportFormat.Csv: dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"; break;
					default: dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"; break;
				}
				if (dialog.ShowDialog(owner) != DialogResult.OK)
					return;
				File.WriteAllText(dialog.FileName, Exporter.Render(content, format), Utf8Bom);
			}
		}

		private static void CopyToClipboard(ExportableContent content, ExportFormat format)
		{
			string rendered = Exporter.Render(content, format);
			DataObject data = new DataObject();
			switch (format)
			{
				case ExportFormat.Html:
					// CF_HTML lets rich editors (Outlook, Word, ...) paste the rendered document;
					// the plain-text fallback carries the HTML source for plain editors.
					data.SetData(DataFormats.Html, WrapCfHtml(rendered));
					data.SetText(rendered);
					break;
				case ExportFormat.Csv:
					// The CSV clipboard format lets spreadsheet applications paste cells directly;
					// the plain-text fallback carries the same CSV for other targets.
					byte[] preamble = Utf8Bom.GetPreamble();
					byte[] csvBytes = Encoding.UTF8.GetBytes(rendered);
					MemoryStream stream = new MemoryStream(preamble.Length + csvBytes.Length);
					stream.Write(preamble, 0, preamble.Length);
					stream.Write(csvBytes, 0, csvBytes.Length);
					stream.Position = 0;
					data.SetData(DataFormats.CommaSeparatedValue, stream);
					data.SetText(rendered);
					break;
				default:
					data.SetText(rendered);
					break;
			}
			Clipboard.SetDataObject(data, true, 10, 100);
		}

		/// <summary>
		/// Opens the default mail client's compose window via a mailto: link.  mailto: can carry
		/// neither an attachment nor a body long enough for real output (command line length
		/// limits), so the output is placed on the clipboard as plain text and the email body tells
		/// the user to paste it.  (Simple MAPI could attach a file, but it hangs for ~30 seconds
		/// before failing on some mail clients that register as MAPI providers, so it is not used.)
		/// </summary>
		private static void SendByEmail(ExportableContent content)
		{
			CopyToClipboard(content, ExportFormat.Text);
			string subject = "WindowsNetTool - " + content.Title;
			string body = "The " + content.Title + " output from WindowsNetTool has been copied to the clipboard."
				+ "\r\n\r\nClick here in the message body and paste (Ctrl+V) to insert it.";
			Process.Start("mailto:?subject=" + Uri.EscapeDataString(subject) + "&body=" + Uri.EscapeDataString(body));
		}

		#endregion

		/// <summary>e.g. "IP Scanner 2026-07-12 14-03-59.csv"; invalid file name characters in the title are replaced.</summary>
		private static string DefaultFileName(ExportableContent content, ExportFormat format)
		{
			StringBuilder sb = new StringBuilder(content.Title.Length);
			foreach (char c in content.Title)
				sb.Append(Array.IndexOf(Path.GetInvalidFileNameChars(), c) >= 0 ? '_' : c);
			return sb + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + Exporter.FileExtension(format);
		}

		/// <summary>
		/// Wraps an HTML document in the clipboard CF_HTML header, whose offsets are byte positions
		/// within the UTF-8 encoding of the complete payload (header included).  WinForms encodes
		/// HTML-format clipboard strings as UTF-8, matching these offsets.
		/// </summary>
		private static string WrapCfHtml(string html)
		{
			const string headerFormat = "Version:0.9\r\nStartHTML:{0:0000000000}\r\nEndHTML:{1:0000000000}\r\nStartFragment:{2:0000000000}\r\nEndFragment:{3:0000000000}\r\n";
			// The offset placeholders are fixed-width and the header is pure ASCII, so its length
			// in characters equals its length in bytes regardless of the values filled in.
			int headerLength = string.Format(headerFormat, 0, 0, 0, 0).Length;

			const string startMarker = "<!--StartFragment-->";
			const string endMarker = "<!--EndFragment-->";
			int startChars = html.IndexOf(startMarker, StringComparison.Ordinal);
			int endChars = html.IndexOf(endMarker, StringComparison.Ordinal);
			startChars = startChars >= 0 ? startChars + startMarker.Length : 0;
			if (endChars < 0)
				endChars = html.Length;

			Encoding utf8 = Encoding.UTF8;
			int startHtml = headerLength;
			int endHtml = headerLength + utf8.GetByteCount(html);
			int startFragment = headerLength + utf8.GetByteCount(html.Substring(0, startChars));
			int endFragment = headerLength + utf8.GetByteCount(html.Substring(0, endChars));
			return string.Format(headerFormat, startHtml, endHtml, startFragment, endFragment) + html;
		}
	}
}
