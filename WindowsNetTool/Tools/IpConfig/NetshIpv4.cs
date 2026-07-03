using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

namespace WindowsNetTool.Tools.IpConfig
{
	/// <summary>
	/// Reads and modifies IPv4 address, gateway, and DNS configuration using the `netsh` command line tool.
	/// Static addresses can coexist with DHCP thanks to the "dhcpstaticipcoexistence" interface option.
	/// </summary>
	public static class NetshIpv4
	{
		/// <summary>
		/// Milliseconds to wait after changing the address source (DHCP/static) before rescanning,
		/// giving Windows time to apply the new configuration.
		/// </summary>
		public const int SettleDelayMs = 750;

		/// <summary>
		/// Scans all network interfaces via `netsh interface ipv4 show addresses`, `netsh interface ipv4 dump`,
		/// and `netsh interface ipv4 show dnsservers`.
		/// Parsing problems are collected in the result's Errors list.  Throws NetshException if netsh itself fails.
		/// </summary>
		public static ScanResult Scan()
		{
			ScanResult result = new ScanResult();

			ScanStage1(result);

			foreach (Ipv4Interface iface in result.Interfaces)
				foreach (Ipv4Address addr in iface.IpAddresses)
					if (addr.Mask == null)
						result.Errors.Add("Interface \"" + iface.InterfaceName + "\" has address " + addr.Ip + " which did not have a subnet mask specified.");

			ScanStage2(result);
			ScanStage3Dns(result);

			return result;
		}

		/// <summary>
		/// Returns the interface which already has the given IP address assigned, or null if no interface has it.
		/// </summary>
		public static Ipv4Interface FindAddressOwner(ScanResult scan, IPAddress ip)
		{
			foreach (Ipv4Interface iface in scan.Interfaces)
				if (iface.IpAddresses.Any(a => a.Ip.Equals(ip)))
					return iface;
			return null;
		}

		#region Modification Operations
		/// <summary>
		/// Adds a static IPv4 address to the given interface, first enabling the "dhcpstaticipcoexistence"
		/// interface option if it is not already enabled.  If mask is null, netsh chooses a default mask.
		/// </summary>
		public static void AddStaticAddress(Ipv4Interface iface, IPAddress ip, IPAddress mask)
		{
			if (!iface.DhcpStaticIpCoexistence)
			{
				Netsh.RunChecked("interface ipv4 set interface interface=\"" + iface.InterfaceName + "\" dhcpstaticipcoexistence=enabled");
				iface.DhcpStaticIpCoexistence = true;
			}
			string command = "interface ipv4 add address \"" + iface.InterfaceName + "\" address=" + ip;
			if (mask != null)
				command += " mask=" + mask;
			Netsh.RunChecked(command);
		}

		/// <summary>
		/// Deletes a static IPv4 address from the given interface.
		/// </summary>
		public static void DeleteStaticAddress(string interfaceName, IPAddress ip)
		{
			Netsh.RunChecked("interface ipv4 delete address \"" + interfaceName + "\" address=" + ip);
		}

		/// <summary>
		/// Adds a static default gateway to the given interface.
		/// A null metric means automatic (netsh default).
		/// </summary>
		public static void AddGateway(string interfaceName, IPAddress gateway, int? metric)
		{
			string command = "interface ipv4 add address \"" + interfaceName + "\" gateway=" + gateway;
			if (metric.HasValue)
				command += " gwmetric=" + metric.Value;
			Netsh.RunChecked(command);
		}

		/// <summary>
		/// Deletes a static default gateway from the given interface.
		/// </summary>
		public static void DeleteGateway(string interfaceName, IPAddress gateway)
		{
			Netsh.RunChecked("interface ipv4 delete address \"" + interfaceName + "\" gateway=" + gateway);
		}

		/// <summary>
		/// Rewrites all static default gateways of the interface with explicit metrics 1..N matching the
		/// given order, so that earlier gateways are preferred.  Windows selects gateways by metric, not
		/// by insertion order, so explicit metrics are the only reliable way to control priority.
		/// </summary>
		public static void ReorderStaticGateways(string interfaceName, IList<IPAddress> orderedGateways)
		{
			foreach (IPAddress gw in orderedGateways)
				DeleteGateway(interfaceName, gw);
			for (int i = 0; i < orderedGateways.Count; i++)
				AddGateway(interfaceName, orderedGateways[i], i + 1);
		}

		/// <summary>
		/// Renames a network interface (connection), e.g. "Ethernet 3" -> "LAN".
		/// </summary>
		public static void RenameInterface(string oldName, string newName)
		{
			Netsh.RunChecked("interface set interface name=\"" + oldName + "\" newname=\"" + newName + "\"");
		}

		/// <summary>
		/// Enables DHCP addressing on the interface.  Windows removes all static addresses when DHCP is
		/// enabled, so this method snapshots the static addresses first and automatically re-adds any that
		/// disappeared (using DHCP+static coexistence).  Static default gateways are intentionally not
		/// restored because they would conflict with the DHCP-provided gateway.
		/// </summary>
		public static DhcpToggleResult EnableDhcp(string interfaceName)
		{
			DhcpToggleResult result = new DhcpToggleResult();
			ScanResult before = Scan();
			Ipv4Interface iface = before.Interfaces.FirstOrDefault(i => i.InterfaceName == interfaceName);
			if (iface == null)
				throw new NetshException("Interface \"" + interfaceName + "\" was not found.");
			if (iface.DhcpEnabled)
				return result; // Nothing to do.
			List<Ipv4Address> staticAddresses = iface.IpAddresses.Where(a => a.IsStatic).ToList();

			Netsh.RunChecked("interface ipv4 set address name=\"" + interfaceName + "\" source=dhcp");

			Thread.Sleep(SettleDelayMs);
			RestoreMissingStatics(interfaceName, staticAddresses, new List<Ipv4Gateway>(), result);
			return result;
		}

		/// <summary>
		/// Disables DHCP addressing on the interface by assigning the given static primary address.
		/// `netsh interface ipv4 set address source=static` replaces the interface's entire address
		/// configuration, so this method snapshots existing static addresses and gateways first and
		/// automatically re-adds any that disappeared.  The interface's current default gateway is
		/// preserved by passing it to the set address command (otherwise it would be lost along with
		/// the DHCP lease).  If the interface obtains DNS servers from DHCP, the current servers are
		/// preserved by converting them to static DNS configuration (otherwise name resolution would
		/// silently break, because the "automatic" DNS source yields no servers once DHCP is off).
		/// </summary>
		public static DhcpToggleResult DisableDhcp(string interfaceName, IPAddress primaryIp, IPAddress primaryMask)
		{
			if (primaryIp == null)
				throw new ArgumentNullException("primaryIp");
			if (primaryMask == null)
				throw new ArgumentNullException("primaryMask");
			DhcpToggleResult result = new DhcpToggleResult();
			ScanResult before = Scan();
			Ipv4Interface iface = before.Interfaces.FirstOrDefault(i => i.InterfaceName == interfaceName);
			if (iface == null)
				throw new NetshException("Interface \"" + interfaceName + "\" was not found.");
			List<Ipv4Address> staticAddresses = iface.IpAddresses.Where(a => a.IsStatic && !a.Ip.Equals(primaryIp)).ToList();
			IPAddress keepGateway = iface.DefaultGateway;
			List<Ipv4Gateway> staticGateways = iface.Gateways.Where(g => g.IsStatic && !g.Ip.Equals(keepGateway)).ToList();
			List<IPAddress> dnsToPreserve = null;
			if (iface.DnsFromDhcp && iface.DnsServers.Count > 0)
				dnsToPreserve = new List<IPAddress>(iface.DnsServers);

			string command = "interface ipv4 set address name=\"" + interfaceName + "\" source=static address=" + primaryIp + " mask=" + primaryMask;
			if (keepGateway != null)
				command += " gateway=" + keepGateway;
			Netsh.RunChecked(command);

			if (dnsToPreserve != null)
			{
				try
				{
					SetDnsServers(interfaceName, dnsToPreserve);
				}
				catch (Exception ex)
				{
					result.Errors.Add("Failed to preserve the DHCP-provided DNS servers ("
						+ string.Join(", ", dnsToPreserve.Select(d => d.ToString()))
						+ ") as static DNS configuration: " + ex.Message);
				}
			}

			Thread.Sleep(SettleDelayMs);
			RestoreMissingStatics(interfaceName, staticAddresses, staticGateways, result);
			return result;
		}

		/// <summary>
		/// Rescans and re-adds any of the expected static addresses and gateways which are no longer
		/// assigned to the interface.
		/// </summary>
		private static void RestoreMissingStatics(string interfaceName, List<Ipv4Address> expectedAddresses, List<Ipv4Gateway> expectedGateways, DhcpToggleResult result)
		{
			if (expectedAddresses.Count == 0 && expectedGateways.Count == 0)
				return;
			ScanResult after = Scan();
			Ipv4Interface iface = after.Interfaces.FirstOrDefault(i => i.InterfaceName == interfaceName);
			if (iface == null)
			{
				result.Errors.Add("Interface \"" + interfaceName + "\" was not found when attempting to restore static configuration.");
				return;
			}
			foreach (Ipv4Address addr in expectedAddresses)
			{
				if (iface.IpAddresses.Any(a => a.Ip.Equals(addr.Ip)))
					continue;
				try
				{
					AddStaticAddress(iface, addr.Ip, addr.Mask);
					result.RestoredAddresses.Add(addr.Mask != null ? addr.Ip + "/" + GetPrefixSizeOfMask(addr.Mask) : addr.Ip.ToString());
				}
				catch (Exception ex)
				{
					result.Errors.Add("Failed to restore static address " + addr.Ip + ": " + ex.Message);
				}
			}
			foreach (Ipv4Gateway gw in expectedGateways)
			{
				if (iface.Gateways.Any(g => g.Ip.Equals(gw.Ip)))
					continue;
				try
				{
					AddGateway(interfaceName, gw.Ip, gw.Metric > 0 ? gw.Metric : (int?)null);
					result.RestoredGateways.Add(gw.Ip.ToString());
				}
				catch (Exception ex)
				{
					result.Errors.Add("Failed to restore static gateway " + gw.Ip + ": " + ex.Message);
				}
			}
		}

		/// <summary>
		/// Replaces the interface's static DNS server list with the given ordered list.
		/// An empty list leaves the interface with static DNS source and no servers.
		/// "validate=no" is used because netsh's DNS validation is slow and rejects servers
		/// that are merely unreachable at the moment.
		/// </summary>
		public static void SetDnsServers(string interfaceName, IList<IPAddress> servers)
		{
			if (servers == null || servers.Count == 0)
			{
				Netsh.RunChecked("interface ipv4 set dnsservers name=\"" + interfaceName + "\" source=static address=none validate=no");
				return;
			}
			Netsh.RunChecked("interface ipv4 set dnsservers name=\"" + interfaceName + "\" source=static address=" + servers[0] + " validate=no");
			for (int i = 1; i < servers.Count; i++)
				Netsh.RunChecked("interface ipv4 add dnsservers name=\"" + interfaceName + "\" address=" + servers[i] + " index=" + (i + 1) + " validate=no");
		}

		/// <summary>
		/// Configures the interface to obtain DNS servers automatically from DHCP.
		/// </summary>
		public static void SetDnsServersDhcp(string interfaceName)
		{
			Netsh.RunChecked("interface ipv4 set dnsservers name=\"" + interfaceName + "\" source=dhcp");
		}
		#endregion

		#region Scan Interfaces
		/// <summary>
		/// Learns from `netsh interface ipv4 show addresses`
		/// </summary>
		private static void ScanStage1(ScanResult result)
		{
			string std = Netsh.RunChecked("interface ipv4 show addresses");

			string[] lines = std.Split('\n').Select(l => l.Trim()).ToArray();

			Ipv4Interface iface = null;
			Ipv4Address lastAddress = null;
			Ipv4Gateway lastGateway = null;
			foreach (string line in lines)
			{
				Match m = Regex.Match(line, "Configuration for interface \"([^\"]*)\"", RegexOptions.IgnoreCase);
				if (m.Success)
				{
					// New interface
					if (iface != null)
					{
						result.Interfaces.Add(iface);
						lastAddress = null;
						lastGateway = null;
					}
					iface = new Ipv4Interface();
					iface.InterfaceName = m.Groups[1].Value;
				}
				else if (iface != null)
				{
					m = Regex.Match(line, "([^:]*):(.*)");
					if (m.Success)
					{
						string key = m.Groups[1].Value.Trim();
						string value = m.Groups[2].Value.Trim();
						if (IEquals(key, "DHCP enabled"))
						{
							iface.DhcpEnabled = IEquals(value, "Yes");
						}
						else if (IEquals(key, "IP Address"))
						{
							if (IPAddress.TryParse(value, out IPAddress addr))
							{
								lastAddress = new Ipv4Address();
								lastAddress.Ip = addr;
								iface.IpAddresses.Add(lastAddress);
							}
							else
							{
								lastAddress = null;
								result.Errors.Add("Unable to parse IP Address record:"
									+ Environment.NewLine + line);
							}
						}
						else if (IEquals(key, "Subnet Prefix"))
						{
							if (lastAddress != null)
							{
								m = Regex.Match(value, "\\(mask (\\d+\\.\\d+\\.\\d+\\.\\d+)\\)", RegexOptions.IgnoreCase);
								if (m.Success)
								{
									if (IPAddress.TryParse(m.Groups[1].Value, out IPAddress addr))
									{
										lastAddress.Mask = addr;
									}
									else
									{
										result.Errors.Add("Unable to parse subnet mask:"
											+ Environment.NewLine + line);
									}
								}
								else
								{
									result.Errors.Add("Unable to parse Subnet Prefix record:"
										+ Environment.NewLine + line);
								}
							}
							else
								result.Errors.Add("Subnet Prefix key encountered but we don't know what IP Address it is associated with:"
									+ Environment.NewLine + line);
						}
						else if (IEquals(key, "Default Gateway"))
						{
							if (IPAddress.TryParse(value, out IPAddress addr))
							{
								lastGateway = new Ipv4Gateway();
								lastGateway.Ip = addr;
								iface.Gateways.Add(lastGateway);
							}
							else
							{
								lastGateway = null;
								result.Errors.Add("Unable to parse Default Gateway record:"
									+ Environment.NewLine + line);
							}
						}
						else if (IEquals(key, "Gateway Metric"))
						{
							if (lastGateway != null && int.TryParse(value, out int metric))
								lastGateway.Metric = metric;
						}
					}
				}
			}
			if (iface != null)
				result.Interfaces.Add(iface);
		}

		/// <summary>
		/// Learns from `netsh interface ipv4 dump` which addresses and gateways are static and which
		/// interfaces have the "dhcpstaticipcoexistence" option enabled.
		/// </summary>
		private static void ScanStage2(ScanResult result)
		{
			string std = Netsh.RunChecked("interface ipv4 dump");

			string[] lines = std.Split('\n').Select(l => l.Trim()).ToArray();

			foreach (string line in lines)
			{
				Match m = Regex.Match(line, "^set interface interface=\"([^\"]*)\" (.*)", RegexOptions.IgnoreCase);
				if (m.Success)
				{
					// This line defines interface options.
					string interfaceName = m.Groups[1].Value;
					Ipv4Interface iface = result.Interfaces.FirstOrDefault(i => i.InterfaceName == interfaceName);
					if (iface != null)
					{
						Dictionary<string, string> args = ParseArguments(m.Groups[2].Value);
						if (args.TryGetValue("dhcpstaticipcoexistence", out string dhcpstaticipcoexistence) && dhcpstaticipcoexistence == "enabled")
						{
							iface.DhcpStaticIpCoexistence = true;
						}
					}
				}
				else
				{
					m = Regex.Match(line, "^add address name=\"([^\"]*)\" (.*)", RegexOptions.IgnoreCase);
					if (m.Success)
					{
						// This line defines static addresses or static gateways.
						string interfaceName = m.Groups[1].Value;
						Ipv4Interface iface = result.Interfaces.FirstOrDefault(i => i.InterfaceName == interfaceName);
						if (iface != null)
						{
							Dictionary<string, string> args = ParseArguments(m.Groups[2].Value);
							if (args.TryGetValue("address", out string address) && args.TryGetValue("mask", out string mask))
							{
								Ipv4Address addr = iface.IpAddresses.FirstOrDefault(a => a.Ip.ToString() == address && a.Mask != null && a.Mask.ToString() == mask);
								if (addr != null)
									addr.IsStatic = true;
								else
								{
									// `netsh interface ipv4 dump` told us about an address that was not seen
									// in `netsh interface ipv4 show addresses`.
									if (IPAddress.TryParse(address, out IPAddress ipAddress) && IPAddress.TryParse(mask, out IPAddress ipMask))
									{
										addr = new Ipv4Address();
										addr.Ip = ipAddress;
										addr.Mask = ipMask;
										addr.IsStatic = true;
										iface.IpAddresses.Add(addr);
									}
									else
									{
										result.Errors.Add("Failed to parse address line from `netsh interface ipv4 dump`:"
											+ Environment.NewLine + line);
									}
								}
							}
							if (args.TryGetValue("gateway", out string gateway))
							{
								if (IPAddress.TryParse(gateway, out IPAddress gwIp))
								{
									Ipv4Gateway gw = iface.Gateways.FirstOrDefault(g => g.Ip.Equals(gwIp));
									if (gw == null)
									{
										gw = new Ipv4Gateway();
										gw.Ip = gwIp;
										iface.Gateways.Add(gw);
									}
									gw.IsStatic = true;
									if (args.TryGetValue("gwmetric", out string gwmetric) && int.TryParse(gwmetric, out int metric))
										gw.Metric = metric;
								}
								else
								{
									result.Errors.Add("Failed to parse gateway line from `netsh interface ipv4 dump`:"
										+ Environment.NewLine + line);
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Learns from `netsh interface ipv4 show dnsservers` which DNS servers each interface uses,
		/// their order, and whether they were configured statically or via DHCP.
		/// </summary>
		private static void ScanStage3Dns(ScanResult result)
		{
			string std = Netsh.RunChecked("interface ipv4 show dnsservers");

			string[] lines = std.Split('\n').Select(l => l.Trim()).ToArray();

			Ipv4Interface iface = null;
			bool inServerList = false;
			foreach (string line in lines)
			{
				Match m = Regex.Match(line, "Configuration for interface \"([^\"]*)\"", RegexOptions.IgnoreCase);
				if (m.Success)
				{
					iface = result.Interfaces.FirstOrDefault(i => i.InterfaceName == m.Groups[1].Value);
					inServerList = false;
					continue;
				}
				if (iface == null)
					continue;
				m = Regex.Match(line, "^(Statically Configured DNS Servers|DNS servers configured through DHCP)\\s*:\\s*(.*)$", RegexOptions.IgnoreCase);
				if (m.Success)
				{
					iface.DnsFromDhcp = m.Groups[1].Value.StartsWith("DNS servers", StringComparison.OrdinalIgnoreCase);
					inServerList = true;
					// The first server appears on the same line as the key.  "None" is also possible here.
					if (IPAddress.TryParse(m.Groups[2].Value.Trim(), out IPAddress dns) && dns.AddressFamily == AddressFamily.InterNetwork)
						iface.DnsServers.Add(dns);
					continue;
				}
				if (inServerList)
				{
					// Additional servers appear as bare addresses on their own lines.
					if (IPAddress.TryParse(line, out IPAddress dns))
					{
						if (dns.AddressFamily == AddressFamily.InterNetwork)
							iface.DnsServers.Add(dns);
					}
					else
						inServerList = false;
				}
			}
		}
		#endregion
		#region Parsing Helpers
		internal static Dictionary<string, string> ParseArguments(string command)
		{
			Dictionary<string, string> argsDict = new Dictionary<string, string>();
			MatchCollection matches = Regex.Matches(command, @"(""[^""]*""|[^""\s]*)=(""[^""]*""|[^""\s]*)");

			foreach (Match match in matches)
			{
				string key = match.Groups[1].Value.Replace("\"", "");
				string value = match.Groups[2].Value.Replace("\"", "");
				argsDict[key] = value;
			}

			return argsDict;
		}

		/// <summary>
		/// Parses user input in the form "192.168.1.2/24" (yielding an address and mask)
		/// or "192.168.1.2" (yielding an address and null mask).
		/// </summary>
		public static bool TryParseIpInput(string input, out IPAddress ip, out IPAddress mask)
		{
			ip = null;
			mask = null;
			if (string.IsNullOrWhiteSpace(input))
				return false;
			input = input.Trim();
			int idxSlash = input.IndexOf('/');
			if (idxSlash != -1)
			{
				if (IPAddress.TryParse(input.Substring(0, idxSlash).Trim(), out ip) && ip.AddressFamily == AddressFamily.InterNetwork
					&& int.TryParse(input.Substring(idxSlash + 1).Trim(), out int maskBits)
					&& maskBits >= 0 && maskBits <= 32)
				{
					mask = GenerateMaskFromPrefixSize(maskBits);
					return true;
				}
			}
			else
			{
				if (IPAddress.TryParse(input, out ip) && ip.AddressFamily == AddressFamily.InterNetwork)
					return true;
			}
			ip = null;
			mask = null;
			return false;
		}

		/// <summary>
		/// Generates an IPv4 subnet mask from a prefix size, e.g. 24 -> 255.255.255.0.
		/// </summary>
		public static IPAddress GenerateMaskFromPrefixSize(int prefixSize)
		{
			if (prefixSize < 0 || prefixSize > 32)
				throw new ArgumentOutOfRangeException("prefixSize", prefixSize, "IPv4 prefix size must be between 0 and 32.");
			uint mask = prefixSize == 0 ? 0u : uint.MaxValue << (32 - prefixSize);
			return new IPAddress(new byte[] { (byte)(mask >> 24), (byte)(mask >> 16), (byte)(mask >> 8), (byte)mask });
		}

		/// <summary>
		/// Counts the leading set bits of an IPv4 subnet mask, e.g. 255.255.255.0 -> 24.
		/// </summary>
		public static int GetPrefixSizeOfMask(IPAddress mask)
		{
			byte[] b = mask.GetAddressBytes();
			uint m = (uint)(b[0] << 24 | b[1] << 16 | b[2] << 8 | b[3]);
			int count = 0;
			while ((m & 0x80000000u) != 0)
			{
				count++;
				m <<= 1;
			}
			return count;
		}

		private static bool IEquals(string a, string b)
		{
			return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
		}
		#endregion
	}

	public class ScanResult
	{
		public List<Ipv4Interface> Interfaces = new List<Ipv4Interface>();
		public List<string> Errors = new List<string>();
	}

	/// <summary>
	/// Result of enabling or disabling DHCP, describing which static addresses and gateways were
	/// automatically re-added after Windows removed them, and any errors that occurred while restoring.
	/// </summary>
	public class DhcpToggleResult
	{
		public List<string> RestoredAddresses = new List<string>();
		public List<string> RestoredGateways = new List<string>();
		public List<string> Errors = new List<string>();
	}

	public class Ipv4Interface
	{
		public string InterfaceName;
		public bool DhcpEnabled;
		public List<Ipv4Address> IpAddresses = new List<Ipv4Address>();
		/// <summary>Default gateways in the order netsh reported them.  Windows prioritizes by metric.</summary>
		public List<Ipv4Gateway> Gateways = new List<Ipv4Gateway>();
		public bool DhcpStaticIpCoexistence;
		/// <summary>True if DNS servers are obtained automatically from DHCP; false if statically configured.</summary>
		public bool DnsFromDhcp;
		/// <summary>DNS servers in resolution order.</summary>
		public List<IPAddress> DnsServers = new List<IPAddress>();

		/// <summary>The first default gateway, or null if the interface has none.</summary>
		public IPAddress DefaultGateway
		{
			get { return Gateways.Count > 0 ? Gateways[0].Ip : null; }
		}

		/// <summary>
		/// Returns a one-line summary used in interface selection lists.
		/// </summary>
		public override string ToString()
		{
			string dhcpStr = "";
			if (DhcpEnabled)
				dhcpStr = " (DHCP enabled)";
			int dhcpIpCount = IpAddresses.Count(a => !a.IsStatic);
			if (dhcpIpCount > 0)
				dhcpStr += " [" + dhcpIpCount + " DHCP addr" + PluralSuffix(dhcpIpCount) + "]";

			string staticIpStr = "";
			int staticIpCount = IpAddresses.Count(a => a.IsStatic);
			if (staticIpCount > 0)
				staticIpStr += " [" + staticIpCount + " static addr" + PluralSuffix(staticIpCount) + "]";

			return InterfaceName + dhcpStr + staticIpStr;
		}

		private static string PluralSuffix(int count)
		{
			return count == 1 ? "" : "s";
		}
	}

	public class Ipv4Address
	{
		public IPAddress Ip;
		public IPAddress Mask;
		public bool IsStatic;
	}

	public class Ipv4Gateway
	{
		public IPAddress Ip;
		/// <summary>Gateway metric.  0 means automatic.</summary>
		public int Metric;
		public bool IsStatic;
	}
}
