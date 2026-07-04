using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WindowsNetTool.Tools.DnsLookup
{
	/// <summary>
	/// DNS record types offered by the DNS Lookup tool.  Values are QTYPE codes from the DNS
	/// protocol.
	/// </summary>
	public enum DnsRecordType : ushort
	{
		A = 1,
		NS = 2,
		CNAME = 5,
		SOA = 6,
		PTR = 12,
		MX = 15,
		TXT = 16,
		AAAA = 28,
		SRV = 33,
		ANY = 255,
		CAA = 257
	}

	/// <summary>One resource record from a DNS response, with its data pre-formatted as text.</summary>
	public class DnsRecord
	{
		public string Name;
		public ushort Type;
		public uint Ttl;
		public string Data;
	}

	/// <summary>A parsed DNS response.</summary>
	public class DnsResponse
	{
		/// <summary>
		/// RCODE from the response header (0 = NoError, 3 = NXDomain, ...), including the extended
		/// RCODE bits from the EDNS0 OPT record when the server sent one.
		/// </summary>
		public int ResponseCode;
		public bool AuthoritativeAnswer;
		public bool Truncated;
		public bool RecursionAvailable;
		/// <summary>True if the response was obtained over TCP because the UDP response was truncated.</summary>
		public bool UsedTcp;
		public long ElapsedMs;
		public List<DnsRecord> Answers = new List<DnsRecord>();
		public List<DnsRecord> Authority = new List<DnsRecord>();
		public List<DnsRecord> Additional = new List<DnsRecord>();
	}

	/// <summary>
	/// A minimal DNS client speaking the DNS wire protocol (RFC 1035) directly, written because
	/// System.Net.Dns can neither query a chosen DNS server nor look up record types other than
	/// address records, and parsing nslookup.exe output would break on non-English Windows.
	/// Queries go over UDP with an EDNS0 OPT record advertising a 4 KB receive buffer; truncated
	/// responses are automatically retried over TCP, and servers that reject EDNS0 with a FormErr
	/// are automatically retried without it.
	/// </summary>
	public static class DnsClient
	{
		private const int DnsPort = 53;
		private const int UdpPayloadSize = 4096;
		private const int UdpAttempts = 2;

		private static readonly Random idGenerator = new Random();

		/// <summary>
		/// Queries <paramref name="server"/> for records of the given type and returns the parsed
		/// response.  Throws with a user-presentable Message when the lookup cannot be completed
		/// (invalid name, timeout, network error, malformed response).
		/// </summary>
		public static DnsResponse Query(IPAddress server, string name, DnsRecordType type, int timeoutMs)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			DnsResponse response = QueryOnce(server, name, type, timeoutMs, true);
			// A FormErr response often means the server does not understand EDNS0; retry without it.
			if (response.ResponseCode == 1)
			{
				DnsResponse retry = QueryOnce(server, name, type, timeoutMs, false);
				if (retry.ResponseCode != 1)
					response = retry;
			}
			response.ElapsedMs = stopwatch.ElapsedMilliseconds;
			return response;
		}

		private static DnsResponse QueryOnce(IPAddress server, string name, DnsRecordType type, int timeoutMs, bool useEdns)
		{
			ushort id;
			lock (idGenerator)
				id = (ushort)idGenerator.Next(0x10000);
			byte[] query = BuildQuery(id, name, type, useEdns);
			DnsResponse response = Parse(ExchangeUdp(server, query, id, timeoutMs), id);
			if (response.Truncated)
			{
				// The full response did not fit in a UDP datagram; repeat the query over TCP.
				response = Parse(ExchangeTcp(server, query, timeoutMs), id);
				response.UsedTcp = true;
			}
			return response;
		}

		/// <summary>Builds the reverse-lookup (PTR) query name for an IP address, e.g. "4.3.2.1.in-addr.arpa".</summary>
		public static string ReverseName(IPAddress address)
		{
			byte[] bytes = address.GetAddressBytes();
			StringBuilder sb = new StringBuilder();
			if (address.AddressFamily == AddressFamily.InterNetworkV6)
			{
				// IPv6 reverse names list each hex nibble from least significant to most significant.
				for (int i = bytes.Length - 1; i >= 0; i--)
				{
					sb.Append((bytes[i] & 0x0F).ToString("x")).Append('.');
					sb.Append((bytes[i] >> 4).ToString("x")).Append('.');
				}
				sb.Append("ip6.arpa");
			}
			else
			{
				for (int i = bytes.Length - 1; i >= 0; i--)
					sb.Append(bytes[i]).Append('.');
				sb.Append("in-addr.arpa");
			}
			return sb.ToString();
		}

		/// <summary>Returns the standard mnemonic for a response code, e.g. "NXDomain" for 3.</summary>
		public static string ResponseCodeName(int rcode)
		{
			switch (rcode)
			{
				case 0: return "NoError";
				case 1: return "FormErr";
				case 2: return "ServFail";
				case 3: return "NXDomain";
				case 4: return "NotImp";
				case 5: return "Refused";
				case 16: return "BadVers";
				default: return "RCODE" + rcode;
			}
		}

		/// <summary>Returns a plain-English description of a response code.</summary>
		public static string ResponseCodeDescription(int rcode)
		{
			switch (rcode)
			{
				case 0: return "Success";
				case 1: return "The server could not interpret the query";
				case 2: return "Server failure";
				case 3: return "The name does not exist";
				case 4: return "The server does not support this kind of query";
				case 5: return "The server refused the query";
				default: return "Error";
			}
		}

		/// <summary>Returns the standard mnemonic for a record type, e.g. "MX" for 15.</summary>
		public static string TypeName(ushort type)
		{
			switch (type)
			{
				case 1: return "A";
				case 2: return "NS";
				case 5: return "CNAME";
				case 6: return "SOA";
				case 12: return "PTR";
				case 15: return "MX";
				case 16: return "TXT";
				case 28: return "AAAA";
				case 33: return "SRV";
				case 35: return "NAPTR";
				case 39: return "DNAME";
				case 41: return "OPT";
				case 43: return "DS";
				case 46: return "RRSIG";
				case 47: return "NSEC";
				case 48: return "DNSKEY";
				case 50: return "NSEC3";
				case 51: return "NSEC3PARAM";
				case 52: return "TLSA";
				case 64: return "SVCB";
				case 65: return "HTTPS";
				case 99: return "SPF";
				case 255: return "ANY";
				case 257: return "CAA";
				default: return "TYPE" + type;
			}
		}

		private static byte[] BuildQuery(ushort id, string name, DnsRecordType type, bool useEdns)
		{
			List<byte> buf = new List<byte>(64);
			buf.Add((byte)(id >> 8));
			buf.Add((byte)id);
			buf.Add(0x01); // flags: standard query, recursion desired
			buf.Add(0x00);
			buf.Add(0); // QDCOUNT = 1
			buf.Add(1);
			buf.Add(0); // ANCOUNT = 0
			buf.Add(0);
			buf.Add(0); // NSCOUNT = 0
			buf.Add(0);
			buf.Add(0); // ARCOUNT
			buf.Add((byte)(useEdns ? 1 : 0));
			WriteName(buf, name);
			buf.Add((byte)((ushort)type >> 8));
			buf.Add((byte)type);
			buf.Add(0); // QCLASS = IN
			buf.Add(1);
			if (useEdns)
			{
				// EDNS0 OPT pseudo-record: root name, type 41, class = advertised UDP payload size.
				buf.Add(0);
				buf.Add(0);
				buf.Add(41);
				buf.Add((byte)(UdpPayloadSize >> 8));
				buf.Add((byte)(UdpPayloadSize & 0xFF));
				buf.Add(0); // extended RCODE and flags
				buf.Add(0);
				buf.Add(0);
				buf.Add(0);
				buf.Add(0); // RDLENGTH = 0
				buf.Add(0);
			}
			return buf.ToArray();
		}

		/// <summary>Encodes a domain name in DNS wire format, converting international names to punycode.</summary>
		private static void WriteName(List<byte> buf, string name)
		{
			name = name.Trim().TrimEnd('.');
			if (ContainsNonAscii(name))
			{
				try
				{
					// International domain names go on the wire in their ASCII (punycode) form.
					name = new IdnMapping().GetAscii(name);
				}
				catch (ArgumentException)
				{
					throw new ArgumentException("\"" + name + "\" could not be converted to an ASCII (punycode) DNS name.");
				}
			}
			if (name.Length == 0)
			{
				buf.Add(0); // the DNS root
				return;
			}
			if (name.Length > 253)
				throw new ArgumentException("\"" + name + "\" is too long to be a DNS name.");
			foreach (string label in name.Split('.'))
			{
				if (label.Length == 0)
					throw new ArgumentException("\"" + name + "\" is not a valid DNS name because it contains an empty label (two dots in a row).");
				if (label.Length > 63)
					throw new ArgumentException("\"" + name + "\" is not a valid DNS name because a label is longer than 63 characters.");
				buf.Add((byte)label.Length);
				foreach (char c in label)
				{
					if (c < 0x21 || c > 0x7E)
						throw new ArgumentException("\"" + name + "\" contains characters that are not allowed in a DNS name.");
					buf.Add((byte)c);
				}
			}
			buf.Add(0);
		}

		private static bool ContainsNonAscii(string s)
		{
			foreach (char c in s)
				if (c > 0x7F)
					return true;
			return false;
		}

		private static byte[] ExchangeUdp(IPAddress server, byte[] query, ushort id, int timeoutMs)
		{
			using (Socket socket = new Socket(server.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
			{
				socket.Connect(server, DnsPort);
				byte[] buffer = new byte[UdpPayloadSize];
				for (int attempt = 0; attempt < UdpAttempts; attempt++)
				{
					socket.Send(query);
					Stopwatch stopwatch = Stopwatch.StartNew();
					while (true)
					{
						int remainingMs = timeoutMs - (int)stopwatch.ElapsedMilliseconds;
						if (remainingMs <= 0)
							break; // this attempt timed out; retransmit if attempts remain
						socket.ReceiveTimeout = remainingMs;
						int length;
						try
						{
							length = socket.Receive(buffer);
						}
						catch (SocketException ex)
						{
							if (ex.SocketErrorCode == SocketError.TimedOut)
								break;
							if (ex.SocketErrorCode == SocketError.ConnectionReset)
								// A connected UDP socket surfaces the server's ICMP "port unreachable" this way.
								throw new IOException("Server " + server + " is not listening for DNS queries on port 53.");
							throw;
						}
						// Ignore datagrams that are not a response to this query.
						if (length >= 12 && buffer[0] == (byte)(id >> 8) && buffer[1] == (byte)id && (buffer[2] & 0x80) != 0)
						{
							byte[] result = new byte[length];
							Array.Copy(buffer, result, length);
							return result;
						}
					}
				}
				throw new TimeoutException("No response from " + server + " (" + UdpAttempts + " attempts, " + timeoutMs + " ms each).");
			}
		}

		private static byte[] ExchangeTcp(IPAddress server, byte[] query, int timeoutMs)
		{
			using (TcpClient client = new TcpClient(server.AddressFamily))
			{
				client.SendTimeout = timeoutMs;
				client.ReceiveTimeout = timeoutMs;
				// TcpClient.Connect has no timeout parameter, so connect asynchronously and wait.
				IAsyncResult connectResult = client.BeginConnect(server, DnsPort, null, null);
				if (!connectResult.AsyncWaitHandle.WaitOne(timeoutMs))
					throw new TimeoutException("Timed out connecting to " + server + " over TCP.");
				client.EndConnect(connectResult);
				NetworkStream stream = client.GetStream();
				// TCP DNS messages are framed with a two-byte big-endian length prefix.
				byte[] framed = new byte[query.Length + 2];
				framed[0] = (byte)(query.Length >> 8);
				framed[1] = (byte)query.Length;
				Array.Copy(query, 0, framed, 2, query.Length);
				stream.Write(framed, 0, framed.Length);
				byte[] lengthPrefix = ReadExactly(stream, 2);
				return ReadExactly(stream, (lengthPrefix[0] << 8) | lengthPrefix[1]);
			}
		}

		private static byte[] ReadExactly(NetworkStream stream, int count)
		{
			byte[] buffer = new byte[count];
			int offset = 0;
			while (offset < count)
			{
				int read = stream.Read(buffer, offset, count - offset);
				if (read <= 0)
					throw new IOException("The TCP connection was closed before the full DNS response arrived.");
				offset += read;
			}
			return buffer;
		}

		private static DnsResponse Parse(byte[] msg, ushort expectedId)
		{
			try
			{
				if (ReadUInt16(msg, 0) != expectedId)
					throw new FormatException("response ID mismatch");
				ushort flags = ReadUInt16(msg, 2);
				DnsResponse response = new DnsResponse();
				response.AuthoritativeAnswer = (flags & 0x0400) != 0;
				response.Truncated = (flags & 0x0200) != 0;
				response.RecursionAvailable = (flags & 0x0080) != 0;
				response.ResponseCode = flags & 0x000F;
				int questionCount = ReadUInt16(msg, 4);
				int answerCount = ReadUInt16(msg, 6);
				int authorityCount = ReadUInt16(msg, 8);
				int additionalCount = ReadUInt16(msg, 10);
				int pos = 12;
				for (int i = 0; i < questionCount; i++)
				{
					ReadName(msg, ref pos);
					pos += 4; // QTYPE and QCLASS
				}
				ReadRecords(msg, ref pos, answerCount, response, response.Answers);
				ReadRecords(msg, ref pos, authorityCount, response, response.Authority);
				ReadRecords(msg, ref pos, additionalCount, response, response.Additional);
				return response;
			}
			catch (FormatException ex)
			{
				throw new FormatException("The DNS server returned a malformed response (" + ex.Message + ").");
			}
		}

		private static void ReadRecords(byte[] msg, ref int pos, int count, DnsResponse response, List<DnsRecord> destination)
		{
			for (int i = 0; i < count; i++)
			{
				string name = ReadName(msg, ref pos);
				ushort type = ReadUInt16(msg, pos);
				pos += 4; // TYPE and CLASS
				uint ttl = ReadUInt32(msg, pos);
				pos += 4;
				ushort rdataLength = ReadUInt16(msg, pos);
				pos += 2;
				if (pos + rdataLength > msg.Length)
					throw new FormatException("record data extends past the end of the response");
				int rdataStart = pos;
				pos += rdataLength;
				if (type == 41)
				{
					// OPT is EDNS0 protocol plumbing, not a real record.  Bits 24-31 of its TTL
					// field extend the header's 4-bit response code.
					response.ResponseCode |= (int)((ttl >> 24) & 0xFF) << 4;
					continue;
				}
				destination.Add(new DnsRecord { Name = name, Type = type, Ttl = ttl, Data = FormatRData(msg, rdataStart, rdataLength, type) });
			}
		}

		private static string FormatRData(byte[] msg, int start, int length, ushort type)
		{
			try
			{
				string formatted = FormatKnownRData(msg, start, length, type);
				if (formatted != null)
					return formatted;
			}
			catch (FormatException)
			{
				// Malformed data for a known type falls through to the generic hex format.
			}
			// Unknown and malformed record data use the generic format from RFC 3597.
			return "\\# " + length + (length > 0 ? " " + BitConverter.ToString(msg, start, length).Replace("-", "") : "");
		}

		/// <summary>Formats the RDATA of well-known record types, or returns null for unrecognized types.</summary>
		private static string FormatKnownRData(byte[] msg, int start, int length, ushort type)
		{
			int pos = start;
			int end = start + length;
			StringBuilder sb;
			switch (type)
			{
				case 1: // A
					if (length != 4)
						return null;
					return new IPAddress(SubArray(msg, start, 4)).ToString();
				case 28: // AAAA
					if (length != 16)
						return null;
					return new IPAddress(SubArray(msg, start, 16)).ToString();
				case 2: // NS
				case 5: // CNAME
				case 12: // PTR
				case 39: // DNAME
					return ReadName(msg, ref pos);
				case 15: // MX: preference, then the mail exchange host
					{
						ushort preference = ReadUInt16(msg, pos);
						pos += 2;
						return preference + " " + ReadName(msg, ref pos);
					}
				case 16: // TXT and SPF: a sequence of length-prefixed character strings
				case 99:
					sb = new StringBuilder();
					while (pos < end)
					{
						int stringLength = msg[pos++];
						if (pos + stringLength > end)
							throw new FormatException("text record data extends past the end of the record");
						if (sb.Length > 0)
							sb.Append(' ');
						sb.Append('"');
						for (int i = 0; i < stringLength; i++)
							AppendTextByte(sb, msg[pos + i], false);
						sb.Append('"');
						pos += stringLength;
					}
					return sb.ToString();
				case 6: // SOA
					{
						string primaryServer = ReadName(msg, ref pos);
						string responsible = ReadName(msg, ref pos);
						if (pos + 20 > end)
							throw new FormatException("SOA record data is too short");
						uint serial = ReadUInt32(msg, pos);
						uint refresh = ReadUInt32(msg, pos + 4);
						uint retry = ReadUInt32(msg, pos + 8);
						uint expire = ReadUInt32(msg, pos + 12);
						uint minimum = ReadUInt32(msg, pos + 16);
						return primaryServer + " " + responsible + " serial=" + serial + " refresh=" + refresh
							+ " retry=" + retry + " expire=" + expire + " minimum=" + minimum;
					}
				case 33: // SRV
					{
						if (pos + 6 > end)
							throw new FormatException("SRV record data is too short");
						ushort priority = ReadUInt16(msg, pos);
						ushort weight = ReadUInt16(msg, pos + 2);
						ushort port = ReadUInt16(msg, pos + 4);
						pos += 6;
						return "priority=" + priority + " weight=" + weight + " port=" + port + " " + ReadName(msg, ref pos);
					}
				case 257: // CAA: flags byte, tag, value
					{
						if (length < 2)
							throw new FormatException("CAA record data is too short");
						int caaFlags = msg[pos++];
						int tagLength = msg[pos++];
						if (pos + tagLength > end)
							throw new FormatException("CAA record data is too short");
						string tag = Encoding.ASCII.GetString(msg, pos, tagLength);
						pos += tagLength;
						sb = new StringBuilder();
						sb.Append(caaFlags).Append(' ').Append(tag).Append(" \"");
						while (pos < end)
							AppendTextByte(sb, msg[pos++], false);
						return sb.Append('"').ToString();
					}
				default:
					return null;
			}
		}

		/// <summary>
		/// Reads a (possibly compressed) domain name starting at <paramref name="pos"/>, advancing
		/// <paramref name="pos"/> past the name.  Unusual bytes in labels are escaped dig-style.
		/// </summary>
		private static string ReadName(byte[] msg, ref int pos)
		{
			StringBuilder sb = new StringBuilder();
			int p = pos;
			bool jumped = false;
			int jumps = 0;
			while (true)
			{
				if (p >= msg.Length)
					throw new FormatException("name extends past the end of the response");
				int len = msg[p];
				if ((len & 0xC0) == 0xC0)
				{
					if (p + 1 >= msg.Length)
						throw new FormatException("truncated compression pointer");
					if (++jumps > 128)
						throw new FormatException("compression pointer loop");
					int target = ((len & 0x3F) << 8) | msg[p + 1];
					if (!jumped)
						pos = p + 2;
					jumped = true;
					p = target;
				}
				else if ((len & 0xC0) != 0)
					throw new FormatException("unsupported label type");
				else if (len == 0)
				{
					if (!jumped)
						pos = p + 1;
					return sb.Length == 0 ? "." : sb.ToString();
				}
				else
				{
					if (p + 1 + len > msg.Length)
						throw new FormatException("label extends past the end of the response");
					if (sb.Length > 0)
						sb.Append('.');
					for (int i = 0; i < len; i++)
						AppendTextByte(sb, msg[p + 1 + i], true);
					p += 1 + len;
				}
			}
		}

		/// <summary>Appends one byte of text data, escaping non-printable bytes as \DDD like dig does.</summary>
		private static void AppendTextByte(StringBuilder sb, byte b, bool escapeDot)
		{
			char c = (char)b;
			if (c == '"' || c == '\\' || (escapeDot && c == '.'))
				sb.Append('\\').Append(c);
			else if (b < 0x20 || b > 0x7E)
				sb.Append('\\').Append(((int)b).ToString("D3"));
			else
				sb.Append(c);
		}

		private static ushort ReadUInt16(byte[] msg, int pos)
		{
			if (pos + 2 > msg.Length)
				throw new FormatException("response is truncated");
			return (ushort)((msg[pos] << 8) | msg[pos + 1]);
		}

		private static uint ReadUInt32(byte[] msg, int pos)
		{
			if (pos + 4 > msg.Length)
				throw new FormatException("response is truncated");
			return ((uint)msg[pos] << 24) | ((uint)msg[pos + 1] << 16) | ((uint)msg[pos + 2] << 8) | msg[pos + 3];
		}

		private static byte[] SubArray(byte[] source, int start, int length)
		{
			byte[] result = new byte[length];
			Array.Copy(source, start, result, 0, length);
			return result;
		}
	}
}
