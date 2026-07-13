using System.Collections.Generic;
using System.Windows.Forms;

namespace WindowsNetTool.Tools.Export
{
	/// <summary>
	/// The exportable content of a tool: a titled list of sections, each being either a table or a
	/// block of preformatted text.  Tools build one of these on demand when the user activates the
	/// Export button, and <see cref="Exporter"/> renders it as plain text, HTML, or CSV.
	/// </summary>
	public class ExportableContent
	{
		/// <summary>The tool name, used in document titles, default file names, and email subjects.</summary>
		public readonly string Title;

		public readonly List<ExportSection> Sections = new List<ExportSection>();

		public ExportableContent(string title)
		{
			Title = title;
		}

		/// <summary>True when no section has any text or table rows, i.e. there is nothing to export.</summary>
		public bool IsEmpty
		{
			get
			{
				foreach (ExportSection section in Sections)
					if (!section.IsEmpty)
						return false;
				return true;
			}
		}

		/// <summary>Adds a block of preformatted text.  Null or whitespace-only text is ignored.</summary>
		public void AddText(string heading, string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return;
			Sections.Add(new ExportSection { Heading = heading, Text = text });
		}

		public void AddTable(string heading, string[] columns, List<string[]> rows)
		{
			Sections.Add(new ExportSection { Heading = heading, Columns = columns, Rows = rows });
		}

		/// <summary>
		/// Adds the rows of a details-view ListView as a table, in displayed order, so the export
		/// reflects the filter and sort the user is looking at.
		/// </summary>
		public void AddListView(string heading, ListView list)
		{
			string[] columns = new string[list.Columns.Count];
			for (int i = 0; i < columns.Length; i++)
				columns[i] = list.Columns[i].Text;
			List<string[]> rows = new List<string[]>(list.Items.Count);
			foreach (ListViewItem item in list.Items)
			{
				string[] row = new string[columns.Length];
				for (int i = 0; i < columns.Length; i++)
					row[i] = i < item.SubItems.Count ? item.SubItems[i].Text : "";
				rows.Add(row);
			}
			AddTable(heading, columns, rows);
		}
	}

	/// <summary>One section of exportable content: either a table or a block of preformatted text.</summary>
	public class ExportSection
	{
		public string Heading;
		/// <summary>Preformatted text, or null when the section is a table.</summary>
		public string Text;
		/// <summary>Table column captions, or null when the section is text.</summary>
		public string[] Columns;
		public List<string[]> Rows;

		public bool IsTable
		{
			get { return Columns != null; }
		}

		public bool IsEmpty
		{
			get { return IsTable ? Rows == null || Rows.Count == 0 : string.IsNullOrWhiteSpace(Text); }
		}
	}
}
