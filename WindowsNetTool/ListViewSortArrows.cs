using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WindowsNetTool
{
	/// <summary>
	/// Shows or hides the native sort arrow in a ListView column header.  WinForms' ListView does
	/// not expose the header control's HDF_SORTUP/HDF_SORTDOWN format flags, so they are set
	/// through the underlying Win32 header control directly.
	/// </summary>
	public static class ListViewSortArrows
	{
		private const int LVM_GETHEADER = 0x101F; // LVM_FIRST + 31
		private const int HDM_GETITEMW = 0x120B;  // HDM_FIRST + 11
		private const int HDM_SETITEMW = 0x120C;  // HDM_FIRST + 12
		private const int HDI_FORMAT = 0x0004;
		private const int HDF_SORTDOWN = 0x0200;
		private const int HDF_SORTUP = 0x0400;

		// HDITEMW from commctrl.h.  Only the fmt field is read and written (mask = HDI_FORMAT),
		// but the struct must be full-size so HDM_GETITEMW does not write past the end.
		[StructLayout(LayoutKind.Sequential)]
		private struct HDITEM
		{
			public int mask;
			public int cxy;
			public IntPtr pszText;
			public IntPtr hbm;
			public int cchTextMax;
			public int fmt;
			public IntPtr lParam;
			public int iImage;
			public int iOrder;
			public int type;
			public IntPtr pvFilter;
			public int state;
		}

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref HDITEM lParam);

		/// <summary>
		/// Sets the sort arrow on one column header: ascending (false), descending (true), or no
		/// arrow (null).
		/// </summary>
		public static void SetSortArrow(ListView listView, int columnIndex, bool? descending)
		{
			IntPtr header = SendMessage(listView.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
			if (header == IntPtr.Zero)
				return;
			HDITEM item = new HDITEM { mask = HDI_FORMAT };
			if (SendMessage(header, HDM_GETITEMW, new IntPtr(columnIndex), ref item) == IntPtr.Zero)
				return;
			item.fmt &= ~(HDF_SORTUP | HDF_SORTDOWN);
			if (descending == true)
				item.fmt |= HDF_SORTDOWN;
			else if (descending == false)
				item.fmt |= HDF_SORTUP;
			SendMessage(header, HDM_SETITEMW, new IntPtr(columnIndex), ref item);
		}
	}
}
