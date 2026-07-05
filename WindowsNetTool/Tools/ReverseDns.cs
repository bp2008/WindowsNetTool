using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using WindowsNetTool.Tools.DnsLookup;

namespace WindowsNetTool.Tools
{
	/// <summary>
	/// Reverse DNS support shared by the discovery tools (IP Scanner, Device List).  Picks which of
	/// the system's DNS servers to query, enforcing the privacy rule that private addresses are only
	/// ever looked up on a local DNS server, never sent to a public resolver.
	/// </summary>
	public class ReverseDns
	{
		/// <summary>A system DNS server suitable for reverse-resolving private addresses (itself a private or on-link address), or null.</summary>
		public IPAddress Local;
		/// <summary>Any system DNS server, used for reverse-resolving public addresses, or null.</summary>
		public IPAddress Any;

		/// <summary>
		/// Picks the DNS servers used for reverse lookups from the system's configured servers:
		/// <see cref="Local"/> is one that is a private or directly-attached (on-link) address,
		/// <see cref="Any"/> is simply the first IPv4 server.  Either may be null.
		/// </summary>
		public static ReverseDns ChooseServers()
		{
			ReverseDns chosen = new ReverseDns();
			List<uint> onLinkNetworks = new List<uint>();
			List<uint> onLinkMasks = new List<uint>();
			try
			{
				NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
				foreach (NetworkInterface nic in nics)
				{
					if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
						continue;
					foreach (UnicastIPAddressInformation ua in nic.GetIPProperties().UnicastAddresses)
					{
						if (ua.Address.AddressFamily != AddressFamily.InterNetwork || ua.IPv4Mask == null)
							continue;
						int prefix = Ipv4Util.PrefixFromMask(Ipv4Util.ToUint(ua.IPv4Mask));
						if (prefix <= 0)
							continue;
						onLinkNetworks.Add(Ipv4Util.ToUint(ua.Address) & Ipv4Util.MaskOf(prefix));
						onLinkMasks.Add(Ipv4Util.MaskOf(prefix));
					}
				}
				foreach (NetworkInterface nic in nics)
				{
					if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
						continue;
					foreach (IPAddress dns in nic.GetIPProperties().DnsAddresses)
					{
						if (dns.AddressFamily != AddressFamily.InterNetwork)
							continue;
						uint address = Ipv4Util.ToUint(dns);
						if (chosen.Any == null)
							chosen.Any = dns;
						if (chosen.Local == null && (Ipv4Util.IsPrivateIPv4(address) || IsOnLink(address, onLinkNetworks, onLinkMasks)))
							chosen.Local = dns;
					}
				}
			}
			catch (NetworkInformationException)
			{
				// No DNS servers found; host name lookups are skipped.
			}
			return chosen;
		}

		/// <summary>
		/// Returns the server that may be asked to reverse-resolve the given address, or null when
		/// none may be: private addresses (IPv4 private ranges, IPv6 link-local and unique-local)
		/// go only to the local server so they never leak to a public resolver.
		/// </summary>
		public IPAddress ServerFor(IPAddress target)
		{
			bool isPrivate;
			if (target.AddressFamily == AddressFamily.InterNetworkV6)
				isPrivate = target.IsIPv6LinkLocal || IsIPv6UniqueLocal(target) || target.IsIPv6SiteLocal || IPAddress.IPv6Loopback.Equals(target);
			else
				isPrivate = Ipv4Util.IsPrivateIPv4(Ipv4Util.ToUint(target));
			return isPrivate ? Local : (Any ?? Local);
		}

		/// <summary>True for IPv6 unique local addresses (fc00::/7), the IPv6 analog of RFC 1918 space.</summary>
		public static bool IsIPv6UniqueLocal(IPAddress address)
		{
			return (address.GetAddressBytes()[0] & 0xFE) == 0xFC;
		}

		/// <summary>
		/// Performs one reverse (PTR) lookup.  Returns the host name, "" when the address
		/// definitively has no name (so the lookup is not repeated), or null when the query timed
		/// out (so the caller may retry it later).
		/// </summary>
		public static string TryReverseLookup(IPAddress server, IPAddress target, int timeoutMs)
		{
			try
			{
				DnsResponse response = DnsClient.Query(server, DnsClient.ReverseName(target), DnsRecordType.PTR, timeoutMs);
				foreach (DnsRecord record in response.Answers)
					if (record.Type == (ushort)DnsRecordType.PTR)
						return record.Data;
				return "";
			}
			catch (TimeoutException)
			{
				return null;
			}
			catch (IOException)
			{
				return null;
			}
			catch
			{
				return ""; // Malformed response, refused, etc.: do not retry.
			}
		}

		private static bool IsOnLink(uint address, List<uint> networks, List<uint> masks)
		{
			for (int i = 0; i < networks.Count; i++)
				if ((address & masks[i]) == networks[i])
					return true;
			return false;
		}
	}
}
