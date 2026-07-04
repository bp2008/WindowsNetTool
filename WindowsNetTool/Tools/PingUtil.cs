using System.Net.NetworkInformation;

namespace WindowsNetTool.Tools
{
	/// <summary>
	/// Shared helpers for the tools that send ICMP echo requests (Ping, Traceroute, IP Scanner).
	/// </summary>
	public static class PingUtil
	{
		/// <summary>The number of payload bytes carried by every echo request the app sends.</summary>
		public const int PayloadSize = 32;

		/// <summary>
		/// The payload carried by every echo request the app sends: <see cref="PayloadSize"/> bytes
		/// cycling 'a'..'w', byte-identical to the payload of Windows' ping.exe.
		/// </summary>
		public static byte[] PingPayload { get; } = CreatePayload();

		private static byte[] CreatePayload()
		{
			byte[] payload = new byte[PayloadSize];
			for (int i = 0; i < payload.Length; i++)
				payload[i] = (byte)('a' + i % 23);
			return payload;
		}

		/// <summary>Returns ping reply statuses phrased like the Windows ping command's messages.</summary>
		public static string DescribeStatus(IPStatus status)
		{
			switch (status)
			{
				case IPStatus.DestinationHostUnreachable: return "Destination host unreachable";
				case IPStatus.DestinationNetworkUnreachable: return "Destination net unreachable";
				case IPStatus.DestinationPortUnreachable: return "Destination port unreachable";
				case IPStatus.DestinationProtocolUnreachable: return "Destination protocol unreachable";
				case IPStatus.DestinationUnreachable: return "Destination unreachable";
				case IPStatus.DestinationScopeMismatch: return "Destination scope mismatch";
				case IPStatus.TtlExpired:
				case IPStatus.TimeExceeded: return "TTL expired in transit";
				case IPStatus.ParameterProblem: return "Parameter problem";
				case IPStatus.SourceQuench: return "Source quench received";
				case IPStatus.BadDestination: return "Bad destination";
				default: return status.ToString();
			}
		}
	}
}
