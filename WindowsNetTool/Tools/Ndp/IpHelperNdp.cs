using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowsNetTool.Tools.Ndp
{
	/// <summary>
	/// One row of the system's IPv6 neighbor cache (NDP table), with fields preformatted for
	/// display and filtering.
	/// </summary>
	public class NdpEntry
	{
		public IPAddress IpAddress;
		/// <summary>The IPv6 address formatted for display and substring filtering, without a scope id.</summary>
		public string IpText;
		/// <summary>The MAC address formatted as in Windows' arp command, e.g. "aa-bb-cc-dd-ee-ff".</summary>
		public string MacAddress;
		/// <summary>The MAC address as bare lowercase hex digits, for separator-insensitive filtering.</summary>
		public string MacDigits;
		public string InterfaceName;
		/// <summary>
		/// The interface's index, needed as the scope id when connecting to the entry's address
		/// (link-local addresses are ambiguous without one).
		/// </summary>
		public uint InterfaceIndex;
		/// <summary>Neighbor state, e.g. "Reachable", "Stale", or "Permanent".</summary>
		public string State;
		/// <summary>True when the neighbor has announced itself as a router.</summary>
		public bool IsRouter;
		/// <summary>The 16 address bytes; comparing them big-endian sorts entries in numeric address order.</summary>
		public byte[] IpSortKey;

		/// <summary>Compares two 16-byte address sort keys in numeric (big-endian) order.</summary>
		public static int CompareSortKeys(byte[] a, byte[] b)
		{
			for (int i = 0; i < a.Length && i < b.Length; i++)
			{
				int c = a[i].CompareTo(b[i]);
				if (c != 0)
					return c;
			}
			return a.Length.CompareTo(b.Length);
		}
	}

	/// <summary>
	/// Reads the IPv6 neighbor cache through the IP Helper API (GetIpNetTable2).  Using the API
	/// instead of parsing "netsh interface ipv6 show neighbors" output keeps the tool independent
	/// of the Windows display language.
	/// </summary>
	public static class IpHelperNdp
	{
		private const ushort AF_INET6 = 23;
		private const uint ERROR_NOT_FOUND = 1168;

		// MIB_IPNET_ROW2 from netioapi.h.  The leading SOCKADDR_INET union is declared through its
		// IPv6 arm (family, port, flowinfo, address, scope id), which is the union's largest member,
		// and is safe because only AF_INET6 rows are requested.
		[StructLayout(LayoutKind.Sequential)]
		private struct MIB_IPNET_ROW2_V6
		{
			public ushort si_family;
			public ushort sin6_port;
			public uint sin6_flowinfo;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public byte[] sin6_addr;
			public uint sin6_scope_id;
			public uint InterfaceIndex;
			public ulong InterfaceLuid;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] // IF_MAX_PHYS_ADDRESS_LENGTH
			public byte[] PhysicalAddress;
			public uint PhysicalAddressLength;
			public uint State; // NL_NEIGHBOR_STATE
			public byte Flags; // bit 0: IsRouter, bit 1: IsUnreachable
			public uint ReachabilityTime;
		}

		[DllImport("iphlpapi.dll")]
		private static extern uint GetIpNetTable2(ushort Family, out IntPtr Table);

		[DllImport("iphlpapi.dll")]
		private static extern void FreeMibTable(IntPtr Memory);

		/// <summary>
		/// Returns the current IPv6 neighbor table, sorted by interface name and then by IP address.
		/// Entries in the Unreachable and Incomplete states are omitted (they never have a resolved
		/// MAC address), matching the ARP tool's treatment of the IPv4 table.  Entries on interfaces
		/// that do not use MAC addressing (e.g. some VPN tunnels) are included with a blank MAC
		/// address.
		/// </summary>
		public static List<NdpEntry> GetNdpEntries()
		{
			List<NdpEntry> entries = new List<NdpEntry>();
			IntPtr table;
			uint result = GetIpNetTable2(AF_INET6, out table);
			if (result == ERROR_NOT_FOUND)
				return entries; // The neighbor cache is empty.
			if (result != 0)
				throw new Win32Exception((int)result, "GetIpNetTable2 failed with error " + result);
			try
			{
				Dictionary<uint, string> interfaceNames = GetInterfaceNames();
				int numEntries = Marshal.ReadInt32(table);
				int rowSize = Marshal.SizeOf(typeof(MIB_IPNET_ROW2_V6));
				// MIB_IPNET_TABLE2 is a ULONG entry count followed by the row array; the rows begin
				// at offset 8 because they contain a ULONG64 and are therefore 8-byte aligned.
				long firstRow = table.ToInt64() + 8;
				for (int i = 0; i < numEntries; i++)
				{
					MIB_IPNET_ROW2_V6 row = (MIB_IPNET_ROW2_V6)Marshal.PtrToStructure(new IntPtr(firstRow + (long)i * rowSize), typeof(MIB_IPNET_ROW2_V6));
					if (row.State <= 1) // Unreachable (0) or Incomplete (1)
						continue;
					// Constructed without a scope id so link-local addresses display without "%n";
					// the interface is a separate column, and InterfaceIndex preserves the scope.
					IPAddress address = new IPAddress(row.sin6_addr);
					entries.Add(new NdpEntry
					{
						IpAddress = address,
						IpText = address.ToString(),
						MacAddress = FormatMac(row.PhysicalAddress, (int)row.PhysicalAddressLength, "-"),
						MacDigits = FormatMac(row.PhysicalAddress, (int)row.PhysicalAddressLength, ""),
						InterfaceName = interfaceNames.TryGetValue(row.InterfaceIndex, out string name) ? name : "Interface #" + row.InterfaceIndex,
						InterfaceIndex = row.InterfaceIndex,
						State = DescribeState(row.State),
						IsRouter = (row.Flags & 1) != 0,
						IpSortKey = row.sin6_addr
					});
				}
			}
			finally
			{
				FreeMibTable(table);
			}
			entries.Sort((a, b) =>
			{
				int c = string.Compare(a.InterfaceName, b.InterfaceName, StringComparison.OrdinalIgnoreCase);
				return c != 0 ? c : NdpEntry.CompareSortKeys(a.IpSortKey, b.IpSortKey);
			});
			return entries;
		}

		private static string FormatMac(byte[] mac, int length, string separator)
		{
			StringBuilder sb = new StringBuilder(length * 3);
			for (int i = 0; i < length; i++)
			{
				if (i > 0)
					sb.Append(separator);
				sb.Append(mac[i].ToString("x2"));
			}
			return sb.ToString();
		}

		/// <summary>
		/// Maps IPv6 interface indexes to friendly interface names ("Ethernet", "Wi-Fi", ...).
		/// </summary>
		private static Dictionary<uint, string> GetInterfaceNames()
		{
			Dictionary<uint, string> names = new Dictionary<uint, string>();
			foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
			{
				try
				{
					IPv6InterfaceProperties props = nic.GetIPProperties().GetIPv6Properties();
					if (props != null)
						names[(uint)props.Index] = nic.Name;
				}
				catch (NetworkInformationException)
				{
					// The adapter has no IPv6 binding; its index cannot appear in AF_INET6 rows.
				}
			}
			return names;
		}

		private static string DescribeState(uint state)
		{
			// NL_NEIGHBOR_STATE.  The distinct states shown here are more informative than the
			// static/dynamic distinction shown by command-line tools.
			switch (state)
			{
				case 0: return "Unreachable";
				case 1: return "Incomplete";
				case 2: return "Probe";
				case 3: return "Delay";
				case 4: return "Stale";
				case 5: return "Reachable";
				case 6: return "Permanent";
				default: return "State " + state;
			}
		}
	}
}
