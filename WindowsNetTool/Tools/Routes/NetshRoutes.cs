using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using WindowsNetTool.Tools.IpConfig;

namespace WindowsNetTool.Tools.Routes
{
	/// <summary>
	/// Reads and modifies persistent IPv4 static routes using the `netsh` command line tool.
	/// Routes added here use store=persistent, so they survive reboots.
	/// </summary>
	public static class NetshRoutes
	{
		/// <summary>
		/// Returns the persistent static routes found in `netsh interface ipv4 dump`.
		/// </summary>
		public static List<StaticRoute> GetPersistentRoutes()
		{
			List<StaticRoute> result = new List<StaticRoute>();
			string std = Netsh.RunChecked("interface ipv4 dump");
			foreach (string rawLine in std.Split('\n'))
			{
				string line = rawLine.Trim();
				Match m = Regex.Match(line, "^add route (.*)$", RegexOptions.IgnoreCase);
				if (!m.Success)
					continue;
				Dictionary<string, string> args = NetshIpv4.ParseArguments(m.Groups[1].Value);
				if (!args.TryGetValue("prefix", out string prefix) || !args.TryGetValue("interface", out string interfaceName))
					continue;
				if (args.TryGetValue("store", out string store) && !string.Equals(store, "persistent", StringComparison.OrdinalIgnoreCase))
					continue;
				StaticRoute route = new StaticRoute();
				route.Prefix = prefix;
				route.InterfaceName = interfaceName;
				if (args.TryGetValue("nexthop", out string nexthop) && IPAddress.TryParse(nexthop, out IPAddress nextHopIp) && !nextHopIp.Equals(IPAddress.Any))
					route.NextHop = nextHopIp;
				if (args.TryGetValue("metric", out string metric) && int.TryParse(metric, out int metricValue))
					route.Metric = metricValue;
				result.Add(route);
			}
			return result;
		}

		/// <summary>
		/// Adds a persistent static route.  A null NextHop creates an on-link route.
		/// A null Metric means automatic.
		/// </summary>
		public static void AddRoute(StaticRoute route)
		{
			string command = "interface ipv4 add route prefix=" + route.Prefix + " interface=\"" + route.InterfaceName + "\"";
			if (route.NextHop != null)
				command += " nexthop=" + route.NextHop;
			if (route.Metric.HasValue)
				command += " metric=" + route.Metric.Value;
			command += " store=persistent";
			Netsh.RunChecked(command);
		}

		/// <summary>
		/// Deletes a persistent static route.
		/// </summary>
		public static void DeleteRoute(StaticRoute route)
		{
			string command = "interface ipv4 delete route prefix=" + route.Prefix + " interface=\"" + route.InterfaceName + "\"";
			if (route.NextHop != null)
				command += " nexthop=" + route.NextHop;
			Netsh.RunChecked(command);
		}

		/// <summary>
		/// Parses route prefix input in the form "10.0.0.0/8".  A bare IPv4 address is treated
		/// as a host route ("/32").
		/// </summary>
		public static bool TryParsePrefix(string input, out string prefix)
		{
			prefix = null;
			if (string.IsNullOrWhiteSpace(input))
				return false;
			input = input.Trim();
			int idxSlash = input.IndexOf('/');
			if (idxSlash != -1)
			{
				if (IPAddress.TryParse(input.Substring(0, idxSlash).Trim(), out IPAddress ip) && ip.AddressFamily == AddressFamily.InterNetwork
					&& int.TryParse(input.Substring(idxSlash + 1).Trim(), out int bits)
					&& bits >= 0 && bits <= 32)
				{
					prefix = ip + "/" + bits;
					return true;
				}
			}
			else
			{
				if (IPAddress.TryParse(input, out IPAddress ip) && ip.AddressFamily == AddressFamily.InterNetwork)
				{
					prefix = ip + "/32";
					return true;
				}
			}
			return false;
		}
	}

	public class StaticRoute
	{
		/// <summary>Destination prefix in CIDR form, e.g. "10.0.0.0/8".</summary>
		public string Prefix;
		public string InterfaceName;
		/// <summary>Gateway for the route, or null for an on-link route.</summary>
		public IPAddress NextHop;
		/// <summary>Route metric, or null for automatic.</summary>
		public int? Metric;
	}
}
