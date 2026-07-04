using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowsNetTool.Tools.Arp
{
	/// <summary>
	/// One row of the system's ARP table (IPv4 neighbor cache), with fields preformatted for
	/// display and filtering.
	/// </summary>
	public class ArpEntry
	{
		public IPAddress IpAddress;
		/// <summary>The IP address formatted for display and substring filtering.</summary>
		public string IpText;
		/// <summary>The MAC address formatted as in Windows' arp command, e.g. "aa-bb-cc-dd-ee-ff".</summary>
		public string MacAddress;
		/// <summary>The MAC address as bare lowercase hex digits, for separator-insensitive filtering.</summary>
		public string MacDigits;
		public string InterfaceName;
		/// <summary>Neighbor state, e.g. "Reachable", "Stale", or "Permanent" (what arp calls "static").</summary>
		public string State;
		/// <summary>The IP address as a big-endian number so entries sort in numeric address order.</summary>
		public uint IpSortKey;
	}

	/// <summary>
	/// Reads the IPv4 ARP table through the IP Helper API (GetIpNetTable2).  Using the API instead
	/// of parsing "arp -a" output keeps the tool independent of the Windows display language.
	/// </summary>
	public static class IpHelperArp
	{
		private const ushort AF_INET = 2;
		private const uint ERROR_NOT_FOUND = 1168;

		// MIB_IPNET_ROW2 from netioapi.h.  The leading SOCKADDR_INET union is declared through its
		// IPv4 arm (family, port, address) with the IPv6 arm of the union covered by padding, which
		// is safe because only AF_INET rows are requested.
		[StructLayout(LayoutKind.Sequential)]
		private struct MIB_IPNET_ROW2
		{
			public ushort si_family;
			public ushort sin_port;
			public uint sin_addr; // IPv4 address in network byte order
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
			public byte[] addr_padding;
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
		/// Returns the current IPv4 ARP table, sorted by interface name and then by IP address.
		/// Entries in the Unreachable and Incomplete states are omitted (they never have a resolved
		/// MAC address), matching the behavior of Windows' arp command.  Entries on interfaces that
		/// do not use MAC addressing (e.g. some VPN tunnels) are included with a blank MAC address,
		/// which also matches arp.
		/// </summary>
		public static List<ArpEntry> GetArpEntries()
		{
			List<ArpEntry> entries = new List<ArpEntry>();
			IntPtr table;
			uint result = GetIpNetTable2(AF_INET, out table);
			if (result == ERROR_NOT_FOUND)
				return entries; // The neighbor cache is empty.
			if (result != 0)
				throw new Win32Exception((int)result, "GetIpNetTable2 failed with error " + result);
			try
			{
				Dictionary<uint, string> interfaceNames = GetInterfaceNames();
				int numEntries = Marshal.ReadInt32(table);
				int rowSize = Marshal.SizeOf(typeof(MIB_IPNET_ROW2));
				// MIB_IPNET_TABLE2 is a ULONG entry count followed by the row array; the rows begin
				// at offset 8 because they contain a ULONG64 and are therefore 8-byte aligned.
				long firstRow = table.ToInt64() + 8;
				for (int i = 0; i < numEntries; i++)
				{
					MIB_IPNET_ROW2 row = (MIB_IPNET_ROW2)Marshal.PtrToStructure(new IntPtr(firstRow + (long)i * rowSize), typeof(MIB_IPNET_ROW2));
					if (row.State <= 1) // Unreachable (0) or Incomplete (1)
						continue;
					byte[] ipBytes = BitConverter.GetBytes(row.sin_addr); // network byte order
					entries.Add(new ArpEntry
					{
						IpAddress = new IPAddress(ipBytes),
						IpText = new IPAddress(ipBytes).ToString(),
						MacAddress = FormatMac(row.PhysicalAddress, (int)row.PhysicalAddressLength, "-"),
						MacDigits = FormatMac(row.PhysicalAddress, (int)row.PhysicalAddressLength, ""),
						InterfaceName = interfaceNames.TryGetValue(row.InterfaceIndex, out string name) ? name : "Interface #" + row.InterfaceIndex,
						State = DescribeState(row.State),
						IpSortKey = (uint)(ipBytes[0] << 24 | ipBytes[1] << 16 | ipBytes[2] << 8 | ipBytes[3])
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
				return c != 0 ? c : a.IpSortKey.CompareTo(b.IpSortKey);
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
		/// Maps IPv4 interface indexes to friendly interface names ("Ethernet", "Wi-Fi", ...).
		/// </summary>
		private static Dictionary<uint, string> GetInterfaceNames()
		{
			Dictionary<uint, string> names = new Dictionary<uint, string>();
			foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
			{
				try
				{
					IPv4InterfaceProperties props = nic.GetIPProperties().GetIPv4Properties();
					if (props != null)
						names[(uint)props.Index] = nic.Name;
				}
				catch (NetworkInformationException)
				{
					// The adapter has no IPv4 binding; its index cannot appear in AF_INET rows.
				}
			}
			return names;
		}

		private static string DescribeState(uint state)
		{
			// NL_NEIGHBOR_STATE.  Windows' arp command displays Permanent entries as "static" and
			// everything else as "dynamic"; the distinct states shown here are more informative.
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
