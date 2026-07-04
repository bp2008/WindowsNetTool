using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsNetTool.Tools.Ping;

namespace WindowsNetTool.Tools.Arp
{
	/// <summary>
	/// Displays the system's ARP table (IPv4 neighbor cache) with live filtering by IP address
	/// and by MAC address.  Activating an entry jumps to the Ping tool and begins pinging it.
	/// </summary>
	public partial class ArpTool : UserControl, IRefreshOnActivate
	{
		private class SortKey
		{
			public int Column;
			public bool Descending;
		}

		private bool busy = false;
		private List<ArpEntry> allEntries = new List<ArpEntry>();
		/// <summary>
		/// The column sort order chosen by clicking column headers, in priority order.  Empty
		/// until the first click; the list is then in its natural order (interface, then IP),
		/// which is also the tie-breaker whenever the clicked columns compare equal.
		/// </summary>
		private readonly List<SortKey> sortKeys = new List<SortKey>();

		public ArpTool()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignMode)
				RefreshArp();
		}

		public void RefreshOnActivate()
		{
			RefreshArp();
		}

		private async void RefreshArp()
		{
			if (busy)
				return;
			SetBusy(true);
			try
			{
				allEntries = await Task.Run(() => IpHelperArp.GetArpEntries());
				ApplyFilter();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to read the ARP table", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				SetBusy(false);
			}
		}

		private void SetBusy(bool value)
		{
			busy = value;
			// The filter boxes stay enabled so typing is not interrupted by a refresh.
			btnRefresh.Enabled = !value;
			btnPing.Enabled = !value;
			Cursor = value ? Cursors.WaitCursor : Cursors.Default;
		}

		/// <summary>
		/// Repopulates the list from the cached entries, keeping only those matching the current
		/// filter boxes and applying the current column sort.  Both filters match anywhere within
		/// the address; the MAC filter ignores separator characters so "aabb", "aa-bb", and
		/// "aa:bb" are equivalent.
		/// </summary>
		private void ApplyFilter()
		{
			string ipFilter = txtFilterIp.Text.Trim();
			string macFilter = NormalizeMacFilter(txtFilterMac.Text);
			string previousSelection = listArp.SelectedItems.Count > 0 ? ((ArpEntry)listArp.SelectedItems[0].Tag).IpText : null;

			List<ArpEntry> shown = new List<ArpEntry>(allEntries.Count);
			foreach (ArpEntry entry in allEntries)
			{
				if (ipFilter.Length > 0 && entry.IpText.IndexOf(ipFilter, StringComparison.Ordinal) < 0)
					continue;
				if (macFilter.Length > 0 && entry.MacDigits.IndexOf(macFilter, StringComparison.Ordinal) < 0)
					continue;
				shown.Add(entry);
			}
			if (sortKeys.Count > 0)
				shown.Sort(CompareEntries);

			listArp.BeginUpdate();
			listArp.Items.Clear();
			foreach (ArpEntry entry in shown)
			{
				ListViewItem item = new ListViewItem(new string[]
				{
					entry.IpText,
					entry.MacAddress,
					entry.InterfaceName,
					entry.State
				});
				item.Tag = entry;
				if (entry.IpText == previousSelection)
					item.Selected = true;
				listArp.Items.Add(item);
			}
			listArp.EndUpdate();
			lblCount.Text = listArp.Items.Count == allEntries.Count
				? allEntries.Count + " entries"
				: listArp.Items.Count + " of " + allEntries.Count + " entries";
		}

		/// <summary>
		/// Compares two entries by each clicked sort column in priority order, falling back to
		/// the natural order (interface name, then IP address) when all of them compare equal.
		/// </summary>
		private int CompareEntries(ArpEntry a, ArpEntry b)
		{
			foreach (SortKey key in sortKeys)
			{
				int c = CompareByColumn(a, b, key.Column);
				if (c != 0)
					return key.Descending ? -c : c;
			}
			int n = string.Compare(a.InterfaceName, b.InterfaceName, StringComparison.OrdinalIgnoreCase);
			return n != 0 ? n : a.IpSortKey.CompareTo(b.IpSortKey);
		}

		private static int CompareByColumn(ArpEntry a, ArpEntry b, int column)
		{
			switch (column)
			{
				case 0: return a.IpSortKey.CompareTo(b.IpSortKey); // IP Address, numerically
				case 1: return string.Compare(a.MacDigits, b.MacDigits, StringComparison.Ordinal); // MAC Address
				case 2: return string.Compare(a.InterfaceName, b.InterfaceName, StringComparison.OrdinalIgnoreCase); // Interface
				case 3: return string.Compare(a.State, b.State, StringComparison.Ordinal); // State
				default: return 0;
			}
		}

		private void listArp_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			SortKey existing = sortKeys.Find(k => k.Column == e.Column);
			if ((ModifierKeys & Keys.Shift) == Keys.Shift)
			{
				// Shift+click adds the column as a further sort level, or reverses it if it is
				// already one.
				if (existing != null)
					existing.Descending = !existing.Descending;
				else
					sortKeys.Add(new SortKey { Column = e.Column });
			}
			else
			{
				// A plain click sorts by just this column: ascending first, reversed when it is
				// already the primary sort column.
				bool descending = sortKeys.Count > 0 && sortKeys[0].Column == e.Column && !sortKeys[0].Descending;
				sortKeys.Clear();
				sortKeys.Add(new SortKey { Column = e.Column, Descending = descending });
			}
			ApplyFilter();
			UpdateSortArrows();
		}

		private void UpdateSortArrows()
		{
			for (int i = 0; i < listArp.Columns.Count; i++)
			{
				SortKey key = sortKeys.Find(k => k.Column == i);
				ListViewSortArrows.SetSortArrow(listArp, i, key == null ? (bool?)null : key.Descending);
			}
		}

		/// <summary>
		/// Reduces a MAC filter string to bare lowercase digits so it can be matched against
		/// <see cref="ArpEntry.MacDigits"/> regardless of which separator style the user typed.
		/// </summary>
		private static string NormalizeMacFilter(string text)
		{
			StringBuilder sb = new StringBuilder(text.Length);
			foreach (char c in text)
				if (c != ':' && c != '-' && c != '.' && !char.IsWhiteSpace(c))
					sb.Append(char.ToLowerInvariant(c));
			return sb.ToString();
		}

		private void txtFilter_TextChanged(object sender, EventArgs e)
		{
			ApplyFilter();
		}

		private void btnRefresh_Click(object sender, EventArgs e)
		{
			RefreshArp();
		}

		private void btnPing_Click(object sender, EventArgs e)
		{
			if (listArp.SelectedItems.Count == 0)
			{
				MessageBox.Show(this, "Select an entry in the list first.", "No entry selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			PingSelected();
		}

		private void listArp_ItemActivate(object sender, EventArgs e)
		{
			PingSelected();
		}

		private void PingSelected()
		{
			if (listArp.SelectedItems.Count == 0)
				return;
			ArpEntry entry = (ArpEntry)listArp.SelectedItems[0].Tag;
			MainForm main = FindForm() as MainForm;
			if (main == null)
				return;
			PingTool ping = main.ActivateTool<PingTool>();
			if (ping != null)
				ping.StartPing(entry.IpText);
		}
	}
}
