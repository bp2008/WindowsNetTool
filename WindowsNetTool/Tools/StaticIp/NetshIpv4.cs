using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WindowsNetTool.Tools.StaticIp
{
	/// <summary>
	/// Reads and modifies IPv4 address configuration using the `netsh` command line tool.
	/// Static addresses can coexist with DHCP thanks to the "dhcpstaticipcoexistence" interface option.
	/// </summary>
	public static class NetshIpv4
	{
		/// <summary>
		/// Scans all network interfaces via `netsh interface ipv4 show addresses` and `netsh interface ipv4 dump`.
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

			return result;
		}

		/// <summary>
		/// Adds a static IPv4 address to the given interface, first enabling the "dhcpstaticipcoexistence"
		/// interface option if it is not already enabled.  If mask is null, netsh chooses a default mask.
		/// </summary>
		public static void AddStaticAddress(Ipv4Interface iface, IPAddress ip, IPAddress mask)
		{
			if (!iface.DhcpStaticIpCoexistence)
			{
				RunNetshChecked("interface ipv4 set interface interface=\"" + iface.InterfaceName + "\" dhcpstaticipcoexistence=enabled");
				iface.DhcpStaticIpCoexistence = true;
			}
			string command = "interface ipv4 add address \"" + iface.InterfaceName + "\" address=" + ip;
			if (mask != null)
				command += " mask=" + mask;
			RunNetshChecked(command);
		}

		/// <summary>
		/// Deletes a static IPv4 address from the given interface.
		/// </summary>
		public static void DeleteStaticAddress(string interfaceName, IPAddress ip)
		{
			RunNetshChecked("interface ipv4 delete address \"" + interfaceName + "\" address=" + ip);
		}

		#region Scan Interfaces
		/// <summary>
		/// Learns from `netsh interface ipv4 show addresses`
		/// </summary>
		private static void ScanStage1(ScanResult result)
		{
			string std = RunNetshChecked("interface ipv4 show addresses");

			string[] lines = std.Split('\n').Select(l => l.Trim()).ToArray();

			Ipv4Interface iface = null;
			Ipv4Address lastAddress = null;
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
								iface.DefaultGateway = addr;
							}
							else
							{
								result.Errors.Add("Unable to parse Default Gateway record:"
									+ Environment.NewLine + line);
							}
						}
					}
				}
			}
			if (iface != null)
				result.Interfaces.Add(iface);
		}

		/// <summary>
		/// Learns from `netsh interface ipv4 dump` which addresses are static and which interfaces
		/// have the "dhcpstaticipcoexistence" option enabled.
		/// </summary>
		private static void ScanStage2(ScanResult result)
		{
			string std = RunNetshChecked("interface ipv4 dump");

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
						// This line defines static addresses.
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
						}
					}
				}
			}
		}
		#endregion
		#region Parsing Helpers
		private static Dictionary<string, string> ParseArguments(string command)
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
		/// <summary>
		/// Runs `netsh` with the given arguments and returns standard output.
		/// Throws NetshException if the exit code is nonzero.
		/// </summary>
		private static string RunNetshChecked(string arguments)
		{
			using (Process p = new Process())
			{
				p.StartInfo.FileName = "netsh";
				p.StartInfo.Arguments = arguments;
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.CreateNoWindow = true;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.RedirectStandardError = true;
				p.Start();
				Task<string> errTask = p.StandardError.ReadToEndAsync();
				string std = p.StandardOutput.ReadToEnd();
				string err = errTask.Result;
				p.WaitForExit();
				if (p.ExitCode != 0)
				{
					string message = "`netsh " + arguments + "` exited with code " + p.ExitCode + ".";
					if (!string.IsNullOrWhiteSpace(std))
						message += Environment.NewLine + std.Trim();
					if (!string.IsNullOrWhiteSpace(err))
						message += Environment.NewLine + err.Trim();
					throw new NetshException(message);
				}
				return std;
			}
		}
	}

	public class NetshException : Exception
	{
		public NetshException(string message) : base(message) { }
	}

	public class ScanResult
	{
		public List<Ipv4Interface> Interfaces = new List<Ipv4Interface>();
		public List<string> Errors = new List<string>();
	}

	public class Ipv4Interface
	{
		public string InterfaceName;
		public bool DhcpEnabled;
		public List<Ipv4Address> IpAddresses = new List<Ipv4Address>();
		public IPAddress DefaultGateway;
		public bool DhcpStaticIpCoexistence;

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
}
