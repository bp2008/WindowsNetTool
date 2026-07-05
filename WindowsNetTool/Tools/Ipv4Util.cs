using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace WindowsNetTool.Tools
{
	/// <summary>
	/// IPv4 address arithmetic shared by the subnet-scanning tools (IP Scanner, Device List).
	/// Addresses are handled as big-endian uints so that numeric order equals address order.
	/// </summary>
	public static class Ipv4Util
	{
		public static bool TryParseCidr(string text, out uint network, out int prefix, out string error)
		{
			network = 0;
			prefix = 0;
			int slash = text.IndexOf('/');
			if (slash < 0)
			{
				error = "Enter a subnet in CIDR notation, e.g. 192.168.1.0/24.";
				return false;
			}
			IPAddress ip;
			if (!IPAddress.TryParse(text.Substring(0, slash), out ip) || ip.AddressFamily != AddressFamily.InterNetwork)
			{
				error = "\"" + text.Substring(0, slash) + "\" is not a valid IPv4 address.";
				return false;
			}
			if (!int.TryParse(text.Substring(slash + 1), out prefix) || prefix < 0 || prefix > 32)
			{
				error = "The prefix length after the \"/\" must be a number from 0 to 32.";
				return false;
			}
			network = ToUint(ip) & MaskOf(prefix);
			error = null;
			return true;
		}

		/// <summary>
		/// Lists every host address in the subnet.  The network and broadcast addresses are
		/// excluded for ordinary subnets; /31 and /32 use all their addresses.
		/// </summary>
		public static List<IPAddress> EnumerateAddresses(uint network, int prefix)
		{
			uint broadcast = network | ~MaskOf(prefix);
			List<IPAddress> list = new List<IPAddress>();
			if (prefix == 32)
				list.Add(FromUint(network));
			else if (prefix == 31)
			{
				list.Add(FromUint(network));
				list.Add(FromUint(broadcast));
			}
			else
				for (uint value = network + 1; value < broadcast; value++)
					list.Add(FromUint(value));
			return list;
		}

		public static uint MaskOf(int prefix)
		{
			// The C# shift operator masks its count to 0-31, so a shift by 32 must be special-cased.
			return prefix == 0 ? 0u : uint.MaxValue << (32 - prefix);
		}

		/// <summary>Returns the prefix length of a subnet mask, or -1 if the mask is not contiguous.</summary>
		public static int PrefixFromMask(uint mask)
		{
			int prefix = 0;
			while ((mask & 0x80000000u) != 0)
			{
				prefix++;
				mask <<= 1;
			}
			return mask == 0 ? prefix : -1;
		}

		/// <summary>
		/// True for addresses that public DNS servers cannot meaningfully reverse-resolve:
		/// RFC 1918 private ranges, loopback, link-local, and carrier-grade NAT (100.64/10).
		/// </summary>
		public static bool IsPrivateIPv4(uint address)
		{
			byte a = (byte)(address >> 24);
			byte b = (byte)(address >> 16);
			return a == 10
				|| (a == 172 && b >= 16 && b <= 31)
				|| (a == 192 && b == 168)
				|| (a == 169 && b == 254)
				|| (a == 100 && b >= 64 && b <= 127)
				|| a == 127;
		}

		public static uint ToUint(IPAddress address)
		{
			byte[] bytes = address.GetAddressBytes();
			return (uint)(bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]);
		}

		public static IPAddress FromUint(uint value)
		{
			return new IPAddress(new byte[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value });
		}
	}
}
