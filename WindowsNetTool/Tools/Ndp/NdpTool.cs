using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsNetTool.Tools.Ping;

namespace WindowsNetTool.Tools.Ndp
{
	/// <summary>
	/// Displays the system's NDP table (IPv6 neighbor cache) with live filtering by IP address
	/// and by MAC address.  Activating an entry jumps to the Ping tool and begins pinging it.
	/// </summary>
	public partial class NdpTool : UserControl, IRefreshOnActivate
	{
		private class SortKey
		{
			public int Column;
			public bool Descending;
		}

		private bool busy = false;
		private List<NdpEntry> allEntries = new List<NdpEntry>();
		/// <summary>
		/// The column sort order chosen by clicking column headers, in priority order.  Empty
		/// until the first click; the list is then in its natural order (interface, then IP),
		/// which is also the tie-breaker whenever the clicked columns compare equal.
		/// </summary>
		private readonly List<SortKey> sortKeys = new List<SortKey>();

		public NdpTool()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignMode)
				RefreshNdp();
		}

		public void RefreshOnActivate()
		{
			RefreshNdp();
		}

		private async void RefreshNdp()
		{
			if (busy)
				return;
			SetBusy(true);
			try
			{
				allEntries = await Task.Run(() => IpHelperNdp.GetNdpEntries());
				ApplyFilter();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Failed to read the NDP table", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
			string previousSelection = listNdp.SelectedItems.Count > 0 ? SelectionKey((NdpEntry)listNdp.SelectedItems[0].Tag) : null;

			List<NdpEntry> shown = new List<NdpEntry>(allEntries.Count);
			foreach (NdpEntry entry in allEntries)
			{
				// IPv6 text is hexadecimal, so unlike the ARP tool the match ignores case.
				if (ipFilter.Length > 0 && entry.IpText.IndexOf(ipFilter, StringComparison.OrdinalIgnoreCase) < 0)
					continue;
				if (macFilter.Length > 0 && entry.MacDigits.IndexOf(macFilter, StringComparison.Ordinal) < 0)
					continue;
				shown.Add(entry);
			}
			if (sortKeys.Count > 0)
				shown.Sort(CompareEntries);

			listNdp.BeginUpdate();
			listNdp.Items.Clear();
			foreach (NdpEntry entry in shown)
			{
				ListViewItem item = new ListViewItem(new string[]
				{
					entry.IpText,
					entry.MacAddress,
					entry.InterfaceName,
					entry.State,
					entry.IsRouter ? "Yes" : ""
				});
				item.Tag = entry;
				if (SelectionKey(entry) == previousSelection)
					item.Selected = true;
				listNdp.Items.Add(item);
			}
			listNdp.EndUpdate();
			lblCount.Text = listNdp.Items.Count == allEntries.Count
				? allEntries.Count + " entries"
				: listNdp.Items.Count + " of " + allEntries.Count + " entries";
		}

		/// <summary>
		/// Identifies an entry across refreshes.  The interface index is included because the same
		/// link-local address can legitimately appear on multiple interfaces.
		/// </summary>
		private static string SelectionKey(NdpEntry entry)
		{
			return entry.IpText + "%" + entry.InterfaceIndex;
		}

		/// <summary>
		/// Compares two entries by each clicked sort column in priority order, falling back to
		/// the natural order (interface name, then IP address) when all of them compare equal.
		/// </summary>
		private int CompareEntries(NdpEntry a, NdpEntry b)
		{
			foreach (SortKey key in sortKeys)
			{
				int c = CompareByColumn(a, b, key.Column);
				if (c != 0)
					return key.Descending ? -c : c;
			}
			int n = string.Compare(a.InterfaceName, b.InterfaceName, StringComparison.OrdinalIgnoreCase);
			return n != 0 ? n : NdpEntry.CompareSortKeys(a.IpSortKey, b.IpSortKey);
		}

		private static int CompareByColumn(NdpEntry a, NdpEntry b, int column)
		{
			switch (column)
			{
				case 0: return NdpEntry.CompareSortKeys(a.IpSortKey, b.IpSortKey); // IP Address, numerically
				case 1: return string.Compare(a.MacDigits, b.MacDigits, StringComparison.Ordinal); // MAC Address
				case 2: return string.Compare(a.InterfaceName, b.InterfaceName, StringComparison.OrdinalIgnoreCase); // Interface
				case 3: return string.Compare(a.State, b.State, StringComparison.Ordinal); // State
				case 4: return a.IsRouter.CompareTo(b.IsRouter); // Router
				default: return 0;
			}
		}

		private void listNdp_ColumnClick(object sender, ColumnClickEventArgs e)
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
			for (int i = 0; i < listNdp.Columns.Count; i++)
			{
				SortKey key = sortKeys.Find(k => k.Column == i);
				ListViewSortArrows.SetSortArrow(listNdp, i, key == null ? (bool?)null : key.Descending);
			}
		}

		/// <summary>
		/// Reduces a MAC filter string to bare lowercase digits so it can be matched against
		/// <see cref="NdpEntry.MacDigits"/> regardless of which separator style the user typed.
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
			RefreshNdp();
		}

		private void btnPing_Click(object sender, EventArgs e)
		{
			if (listNdp.SelectedItems.Count == 0)
			{
				MessageBox.Show(this, "Select an entry in the list first.", "No entry selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			PingSelected();
		}

		private void listNdp_ItemActivate(object sender, EventArgs e)
		{
			PingSelected();
		}

		private void PingSelected()
		{
			if (listNdp.SelectedItems.Count == 0)
				return;
			NdpEntry entry = (NdpEntry)listNdp.SelectedItems[0].Tag;
			MainForm main = FindForm() as MainForm;
			if (main == null)
				return;
			PingTool ping = main.ActivateTool<PingTool>();
			if (ping != null)
			{
				// Link-local addresses are only routable with a scope id (the interface index).
				string target = entry.IpAddress.IsIPv6LinkLocal
					? entry.IpText + "%" + entry.InterfaceIndex
					: entry.IpText;
				ping.StartPing(target);
			}
		}
	}
}
