using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsNetTool.Tools.TcpTest
{
	/// <summary>
	/// Tests TCP connectivity to a host and port.  In "connect only" mode a connection is opened
	/// and immediately closed to show whether the port is open, distinguishing a closed port
	/// (connection refused) from an unreachable host (timeout).  In HTTP GET mode a request for a
	/// URL is sent over the connection (with TLS when the URL is https) and the log shows each
	/// phase with timing: DNS resolution, TCP connect, TLS handshake and certificate details, the
	/// exact request sent, and the response status line, headers, and body.  Because the
	/// connection target and the requested URL are independent, a site can be tested on a
	/// specific server (behind a load balancer, or before a DNS change) by entering that server's
	/// address as the Host while the URL and Host header name the site.  The HTTP client is
	/// implemented directly on TcpClient/SslStream rather than HttpWebRequest so the tool can
	/// connect to an arbitrary address, time each phase, show the raw response without following
	/// redirects, and report certificate problems without aborting the request.  Everything runs
	/// as async continuations on the UI thread; no background threads are created.
	/// </summary>
	public partial class TcpTestTool : UserControl
	{
		/// <summary>Time allowed for each address's TCP connection attempt.</summary>
		private const int ConnectTimeoutMs = 10000;

		/// <summary>Time allowed for the TLS handshake.</summary>
		private const int TlsTimeoutMs = 10000;

		/// <summary>Time allowed for the complete HTTP response, measured from the request being sent.</summary>
		private const int ResponseTimeoutMs = 30000;

		/// <summary>Reading stops after this much response body has arrived.</summary>
		private const int MaxBodyBytes = 512 * 1024;

		/// <summary>Longest body text shown in the log; larger bodies are cut off with a note.</summary>
		private const int MaxBodyDisplayChars = 32000;

		/// <summary>Keep the log around this size; the older half is dropped when it grows past this.</summary>
		private const int MaxLogLength = 400000;

		private bool running = false;
		/// <summary>Incremented whenever a test starts or is canceled so callbacks from an earlier session are ignored.</summary>
		private int session = 0;
		/// <summary>The connection belonging to the current test, closed to abort in-flight operations when canceling.</summary>
		private TcpClient currentClient;

		public TcpTestTool()
		{
			InitializeComponent();
			UpdateModeControls();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			// MainForm hides this control when the user switches to a different tool; the test
			// should not continue in the background.
			if (!Visible && running)
				CancelTest("Test canceled because the TCP Connection Test tool was deactivated.");
		}

		private void radioHttpGet_CheckedChanged(object sender, EventArgs e)
		{
			UpdateModeControls();
		}

		private void UpdateModeControls()
		{
			bool http = radioHttpGet.Checked;
			lblUrl.Enabled = txtUrl.Enabled = lblHostHeader.Enabled = txtHostHeader.Enabled = lblHostHeaderNote.Enabled = http;
		}

		private void btnTest_Click(object sender, EventArgs e)
		{
			if (running)
				CancelTest("Test canceled.");
			else
				BeginTest();
		}

		private void btnClear_Click(object sender, EventArgs e)
		{
			txtLog.Clear();
		}

		private void input_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
				if (!running)
					BeginTest();
			}
		}

		/// <summary>
		/// Validates the inputs and runs one test.  In HTTP GET mode, a blank Host or Port is
		/// filled in from the URL so that pasting a URL and pressing Test just works.
		/// </summary>
		private async void BeginTest()
		{
			string host = txtHost.Text.Trim();
			string portText = txtPort.Text.Trim();
			bool httpMode = radioHttpGet.Checked;
			Uri url = null;
			string hostHeader = null;
			if (httpMode)
			{
				string urlText = txtUrl.Text.Trim();
				if (urlText.Length == 0)
				{
					MessageBox.Show(this, "Enter the URL to request, e.g. https://example.com/index.html", "TCP Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}
				// A URL typed without a scheme ("example.com/path") defaults to https.
				if (urlText.IndexOf("://", StringComparison.Ordinal) < 0)
					urlText = "https://" + urlText;
				if (!Uri.TryCreate(urlText, UriKind.Absolute, out url)
					|| (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps)
					|| url.IdnHost.Length == 0)
				{
					MessageBox.Show(this, "Enter a valid http:// or https:// URL.", "TCP Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}
				hostHeader = txtHostHeader.Text.Trim();
				if (hostHeader.Length == 0)
					hostHeader = (url.HostNameType == UriHostNameType.IPv6 ? "[" + url.IdnHost + "]" : url.IdnHost)
						+ (url.IsDefaultPort ? "" : ":" + url.Port);
				if (host.Length == 0)
					host = url.IdnHost;
				if (portText.Length == 0)
					portText = url.Port.ToString();
			}
			if (host.Length == 0)
			{
				MessageBox.Show(this, "Enter a host name or IP address to connect to.", "TCP Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			int port;
			if (!int.TryParse(portText, NumberStyles.None, CultureInfo.InvariantCulture, out port) || port < 1 || port > 65535)
			{
				MessageBox.Show(this, "Enter a TCP port number between 1 and 65535.", "TCP Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			session++;
			int mySession = session;
			SetRunning(true);
			try
			{
				if (txtLog.TextLength > 0)
					AppendLog("");
				if (httpMode)
					await RunHttpTest(host, port, url, hostHeader, mySession);
				else
					await RunPortCheck(host, port, mySession);
			}
			catch (Exception ex)
			{
				// Unexpected failures (everything routine is caught closer to its source).
				if (session == mySession && !IsDisposed)
					AppendLog("Test failed: " + Describe(ex));
			}
			finally
			{
				// When the test was canceled, CancelTest already restored the UI (and a newer
				// test may even be running), so only the session that is still current cleans up.
				if (session == mySession && !IsDisposed)
				{
					SetRunning(false);
					currentClient = null;
				}
			}
		}

		/// <summary>
		/// Stops the current test.  Incrementing the session makes in-flight continuations
		/// discard their results, and closing the socket makes hung operations complete quickly.
		/// </summary>
		private void CancelTest(string message)
		{
			if (!running)
				return;
			session++;
			TcpClient client = currentClient;
			currentClient = null;
			if (client != null)
				client.Close();
			SetRunning(false);
			AppendLog(message);
		}

		private void SetRunning(bool value)
		{
			running = value;
			btnTest.Text = value ? "Cancel" : "Test";
			txtHost.ReadOnly = txtPort.ReadOnly = txtUrl.ReadOnly = txtHostHeader.ReadOnly = value;
			radioConnectOnly.Enabled = radioHttpGet.Enabled = !value;
		}

		private async Task RunPortCheck(string host, int port, int mySession)
		{
			AppendLog("[" + DateTime.Now.ToString("HH:mm:ss") + "] Port check  " + host + ":" + port);
			TcpClient client = await OpenConnection(host, port, mySession);
			if (session != mySession || IsDisposed)
				return;
			if (client == null)
			{
				AppendLog("Result: FAILED — no connection could be established.");
				return;
			}
			IPAddress remoteAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
			client.Close();
			AppendLog("Result: SUCCESS — TCP port " + port + " is open on " + remoteAddress + ".  The connection has been closed.");
		}

		private async Task RunHttpTest(string host, int port, Uri url, string hostHeader, int mySession)
		{
			AppendLog("[" + DateTime.Now.ToString("HH:mm:ss") + "] HTTP GET  " + url + "  (connect to " + host + ":" + port + ", Host header \"" + hostHeader + "\")");
			Stopwatch total = Stopwatch.StartNew();
			TcpClient client = await OpenConnection(host, port, mySession);
			if (session != mySession || IsDisposed)
				return;
			if (client == null)
			{
				AppendLog("Result: FAILED — no connection could be established.");
				return;
			}
			try
			{
				Stream stream = client.GetStream();
				if (url.Scheme == Uri.UriSchemeHttps)
				{
					stream = await EstablishTls(client, stream, HostFromHostHeader(hostHeader), mySession);
					if (stream == null)
						return;
				}

				// Send the request.  "Connection: close" makes the server end the response by
				// closing the connection, so no persistent-connection bookkeeping is needed.
				// No Accept-Encoding header is sent, so the body arrives uncompressed.
				string requestText = "GET " + url.PathAndQuery + " HTTP/1.1\r\n"
					+ "Host: " + hostHeader + "\r\n"
					+ "User-Agent: WindowsNetTool/" + Application.ProductVersion + "\r\n"
					+ "Accept: */*\r\n"
					+ "Connection: close\r\n"
					+ "\r\n";
				byte[] requestBytes = Encoding.UTF8.GetBytes(requestText);
				Stopwatch responseTimer = Stopwatch.StartNew();
				try
				{
					await WithTimeout(stream.WriteAsync(requestBytes, 0, requestBytes.Length), ResponseTimeoutMs, client);
				}
				catch (Exception ex)
				{
					if (session != mySession || IsDisposed)
						return;
					AppendLog("Sending the request failed: " + Describe(ex));
					AppendLog("Result: FAILED — the request could not be sent.");
					return;
				}
				if (session != mySession || IsDisposed)
					return;
				AppendLog("Request sent:");
				foreach (string line in requestText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
					AppendLog("  " + line);

				// Read the response until the server closes the connection, the body is provably
				// complete, the size cap is reached, or the timeout expires.
				MemoryStream received = new MemoryStream();
				byte[] buffer = new byte[16384];
				int bodyStart = -1;
				List<string> headerLines = null;
				long contentLength = -1;
				bool chunked = false;
				long headersMs = -1;
				bool timedOut = false;
				bool truncated = false;
				string readError = null;
				while (true)
				{
					int remainingMs = ResponseTimeoutMs - (int)responseTimer.ElapsedMilliseconds;
					if (remainingMs <= 0)
					{
						timedOut = true;
						break;
					}
					int read;
					try
					{
						read = await WithTimeout(stream.ReadAsync(buffer, 0, buffer.Length), remainingMs, client);
					}
					catch (TimeoutException)
					{
						if (session != mySession || IsDisposed)
							return;
						timedOut = true;
						break;
					}
					catch (Exception ex)
					{
						if (session != mySession || IsDisposed)
							return;
						// A connection reset after data has arrived still leaves something to
						// show; the error is reported alongside whatever was received.
						readError = Describe(ex);
						break;
					}
					if (session != mySession || IsDisposed)
						return;
					if (read <= 0)
						break; // The server closed the connection: the normal end of the response.
					received.Write(buffer, 0, read);
					if (bodyStart < 0)
					{
						bodyStart = FindHeaderEnd(received);
						if (bodyStart >= 0)
						{
							headersMs = responseTimer.ElapsedMilliseconds;
							// Header bytes are ISO-8859-1 per the HTTP specification.
							string headerText = Encoding.GetEncoding("ISO-8859-1").GetString(received.GetBuffer(), 0, bodyStart).TrimEnd('\r', '\n');
							headerLines = new List<string>(headerText.Split(new string[] { "\r\n" }, StringSplitOptions.None));
							string te = GetHeader(headerLines, "Transfer-Encoding");
							chunked = te != null && te.ToLowerInvariant().Contains("chunked");
							long parsedLength;
							string cl = GetHeader(headerLines, "Content-Length");
							if (cl != null && long.TryParse(cl, NumberStyles.None, CultureInfo.InvariantCulture, out parsedLength))
								contentLength = parsedLength;
						}
					}
					if (bodyStart >= 0 && ResponseComplete(received, bodyStart, contentLength, chunked))
						break;
					if (received.Length - Math.Max(bodyStart, 0) > MaxBodyBytes)
					{
						truncated = true;
						break;
					}
				}

				if (readError != null)
					AppendLog("The connection ended with an error while reading: " + readError);

				if (bodyStart < 0)
				{
					// The connection ended (or timed out) before a complete HTTP header arrived.
					if (received.Length == 0)
					{
						if (timedOut)
							AppendLog("No response was received within " + (ResponseTimeoutMs / 1000) + " seconds.");
						else if (readError == null)
							AppendLog("The server closed the connection without sending a response.");
						AppendLog("Result: FAILED — connected, but no HTTP response was received.");
					}
					else
					{
						AppendLog("The server sent " + received.Length.ToString("N0") + " bytes but no complete HTTP response header.  Raw data received:");
						LogBodyText(ToArraySegment(received, 0), null);
						AppendLog("Result: FAILED — the service on this port did not answer with HTTP.");
					}
					return;
				}

				AppendLog("Response headers received after " + headersMs + " ms:");
				foreach (string line in headerLines)
					AppendLog("  " + line);

				byte[] body = ToArraySegment(received, bodyStart);
				bool chunkedIncomplete = false;
				if (chunked)
				{
					bool complete;
					body = DecodeChunkedBody(body, out complete);
					chunkedIncomplete = !complete;
				}
				if (timedOut)
					AppendLog("The response did not finish within " + (ResponseTimeoutMs / 1000) + " seconds; showing what was received.");
				else if (truncated)
					AppendLog("The response body exceeds " + (MaxBodyBytes / 1024) + " KB; reading stopped there.");
				else if (chunkedIncomplete)
					AppendLog("The chunked response ended before its final chunk; showing what was received.");
				string contentType = GetHeader(headerLines, "Content-Type");
				if (body.Length == 0)
					AppendLog("Response body: empty.");
				else if (LooksTextual(contentType, body))
				{
					AppendLog("Response body: " + body.Length.ToString("N0") + " bytes" + (contentType == null ? "" : " of " + contentType) + ":");
					LogBodyText(body, contentType);
				}
				else
					AppendLog("Response body: " + body.Length.ToString("N0") + " bytes" + (contentType == null ? "" : " of " + contentType) + " (binary content is not shown).");
				AppendLog("Result: " + headerLines[0] + " — " + total.ElapsedMilliseconds + " ms total, response " + responseTimer.ElapsedMilliseconds + " ms after the request was sent.");
			}
			finally
			{
				client.Close();
			}
		}

		/// <summary>
		/// Resolves the host (unless it is already an IP address) and tries each resolved address
		/// in turn until a connection succeeds, logging every attempt.  Returns a connected
		/// TcpClient, or null when no connection could be made (already logged) or the test was
		/// canceled (check the session).
		/// </summary>
		private async Task<TcpClient> OpenConnection(string host, int port, int mySession)
		{
			IPAddress[] addresses;
			IPAddress single;
			if (IPAddress.TryParse(host, out single))
				addresses = new IPAddress[] { single };
			else
			{
				Stopwatch dnsTimer = Stopwatch.StartNew();
				try
				{
					addresses = await Dns.GetHostAddressesAsync(host);
				}
				catch (Exception ex)
				{
					if (session != mySession || IsDisposed)
						return null;
					AppendLog("Unable to resolve " + host + ": " + Describe(ex));
					return null;
				}
				if (session != mySession || IsDisposed)
					return null;
				if (addresses.Length == 0)
				{
					AppendLog("Unable to resolve " + host + ": the name resolved to no addresses.");
					return null;
				}
				AppendLog("Resolved " + host + " in " + dnsTimer.ElapsedMilliseconds + " ms: " + string.Join(", ", addresses.Select(a => a.ToString())));
			}
			foreach (IPAddress address in addresses)
			{
				AppendLog("Connecting to " + FormatEndPoint(address, port) + " ...");
				TcpClient client = new TcpClient(address.AddressFamily);
				currentClient = client;
				Stopwatch timer = Stopwatch.StartNew();
				try
				{
					await WithTimeout(client.ConnectAsync(address, port), ConnectTimeoutMs, client);
					if (session != mySession || IsDisposed)
					{
						client.Close();
						return null;
					}
					AppendLog("Connected in " + timer.ElapsedMilliseconds + " ms (local port " + ((IPEndPoint)client.Client.LocalEndPoint).Port + ").");
					return client;
				}
				catch (Exception ex)
				{
					client.Close();
					if (session != mySession || IsDisposed)
						return null;
					AppendLog("Failed after " + timer.ElapsedMilliseconds + " ms: " + DescribeConnectError(ex));
				}
			}
			return null;
		}

		/// <summary>
		/// Performs the TLS handshake and logs the negotiated parameters and the server's
		/// certificate.  Returns the encrypted stream, or null when the handshake failed (already
		/// logged) or the test was canceled (check the session).
		/// </summary>
		private async Task<Stream> EstablishTls(TcpClient client, Stream innerStream, string sniHost, int mySession)
		{
			SslPolicyErrors certErrors = SslPolicyErrors.None;
			List<string> chainProblems = new List<string>();
			SslStream ssl = new SslStream(innerStream, false, (s, cert, chain, errors) =>
			{
				// The handshake is always allowed to complete; certificate problems are reported
				// in the log instead, because seeing the server's response despite a bad
				// certificate is the point of a diagnostic tool.  The chain details are captured
				// here because the chain object is only valid during the callback.
				certErrors = errors;
				if (chain != null)
					foreach (X509ChainStatus status in chain.ChainStatus)
						chainProblems.Add(status.StatusInformation.Trim().Length > 0
							? status.StatusInformation.Trim() + " (" + status.Status + ")"
							: status.Status.ToString());
				return true;
			});
			Stopwatch tlsTimer = Stopwatch.StartNew();
			try
			{
				// SslProtocols.None selects the operating system's default protocol versions
				// (TLS 1.2, and TLS 1.3 where supported).
				await WithTimeout(ssl.AuthenticateAsClientAsync(sniHost, null, SslProtocols.None, false), TlsTimeoutMs, client);
			}
			catch (Exception ex)
			{
				if (session != mySession || IsDisposed)
					return null;
				AppendLog("TLS handshake failed after " + tlsTimer.ElapsedMilliseconds + " ms: " + Describe(ex));
				AppendLog("Result: FAILED — the port is open, but a TLS session could not be established.");
				return null;
			}
			if (session != mySession || IsDisposed)
				return null;
			// Schannel does not report the key exchange algorithm for TLS 1.3 through this API.
			string keyExchange = ssl.KeyExchangeAlgorithm == ExchangeAlgorithmType.None ? "" : ", key exchange: " + DescribeKeyExchange(ssl.KeyExchangeAlgorithm);
			AppendLog(DescribeSslProtocol(ssl.SslProtocol) + " established in " + tlsTimer.ElapsedMilliseconds + " ms.  Cipher: " + ssl.CipherAlgorithm
				+ " (" + ssl.CipherStrength + "-bit)" + keyExchange + ", hash: " + ssl.HashAlgorithm);
			LogCertificate(ssl, sniHost, certErrors, chainProblems);
			return ssl;
		}

		private void LogCertificate(SslStream ssl, string sniHost, SslPolicyErrors certErrors, List<string> chainProblems)
		{
			if (ssl.RemoteCertificate == null)
			{
				AppendLog("The server presented no certificate.");
				return;
			}
			X509Certificate2 cert = new X509Certificate2(ssl.RemoteCertificate);
			AppendLog("Certificate: " + cert.Subject);
			AppendLog("  Issued by: " + cert.Issuer);
			AppendLog("  Valid " + cert.NotBefore.ToString("yyyy-MM-dd") + " to " + cert.NotAfter.ToString("yyyy-MM-dd") + DescribeValidity(cert));
			string san = SubjectAlternativeNames(cert);
			if (san.Length > 0)
				AppendLog("  Subject alternative names: " + san);
			if (certErrors == SslPolicyErrors.None)
				AppendLog("  The certificate is valid for " + sniHost + ".");
			else
			{
				if ((certErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
					AppendLog("  PROBLEM: The certificate is not valid for the name \"" + sniHost + "\".");
				if ((certErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0)
				{
					AppendLog("  PROBLEM: The certificate chain could not be validated:");
					foreach (string problem in chainProblems)
						AppendLog("    " + problem);
				}
				if ((certErrors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0)
					AppendLog("  PROBLEM: No certificate was available for validation.");
				AppendLog("  (Certificate problems are reported but do not stop this test.)");
			}
		}

		private static string DescribeValidity(X509Certificate2 cert)
		{
			DateTime now = DateTime.Now;
			if (now < cert.NotBefore)
				return "  (NOT YET VALID)";
			if (now > cert.NotAfter)
				return "  (EXPIRED)";
			int days = (int)(cert.NotAfter - now).TotalDays;
			return "  (" + days + " day" + (days == 1 ? "" : "s") + " remaining)";
		}

		private static string SubjectAlternativeNames(X509Certificate2 cert)
		{
			foreach (X509Extension ext in cert.Extensions)
				if (ext.Oid != null && ext.Oid.Value == "2.5.29.17")
					return ext.Format(false);
			return "";
		}

		/// <summary>
		/// Returns the host portion of a Host header value ("example.com:8443" → "example.com",
		/// "[::1]:8080" → "::1") for use as the TLS server name (SNI).
		/// </summary>
		private static string HostFromHostHeader(string hostHeader)
		{
			string value = hostHeader.Trim();
			if (value.StartsWith("[", StringComparison.Ordinal))
			{
				int end = value.IndexOf(']');
				if (end > 1)
					return value.Substring(1, end - 1);
			}
			int colon = value.IndexOf(':');
			// A value with more than one colon is an unbracketed IPv6 address, not host:port.
			if (colon >= 0 && colon == value.LastIndexOf(':'))
				return value.Substring(0, colon);
			return value;
		}

		/// <summary>
		/// Waits for the task to complete, closing the client and throwing TimeoutException if it
		/// does not finish in time.  Closing the socket makes the hung operation complete quickly
		/// with an exception, which is observed by a continuation so it cannot surface later as
		/// an unobserved task fault.
		/// </summary>
		private static async Task WithTimeout(Task task, int timeoutMs, TcpClient client)
		{
			if (await Task.WhenAny(task, Task.Delay(timeoutMs)) != task)
			{
				client.Close();
				_ = task.ContinueWith(t => { _ = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
				throw new TimeoutException();
			}
			await task; // Propagates the task's exception if it faulted.
		}

		private static async Task<T> WithTimeout<T>(Task<T> task, int timeoutMs, TcpClient client)
		{
			await WithTimeout((Task)task, timeoutMs, client);
			return task.Result;
		}

		/// <summary>Returns the index of the first body byte (just past the header-terminating blank line), or -1.</summary>
		private static int FindHeaderEnd(MemoryStream received)
		{
			byte[] data = received.GetBuffer();
			int length = (int)received.Length;
			for (int i = 3; i < length; i++)
				if (data[i] == '\n' && data[i - 1] == '\r' && data[i - 2] == '\n' && data[i - 3] == '\r')
					return i + 1;
			return -1;
		}

		/// <summary>
		/// True once the response body is provably complete.  "Connection: close" means the
		/// response normally ends with the server closing the connection, but a server that keeps
		/// the connection open anyway would leave the read loop waiting out the full timeout, so
		/// a satisfied Content-Length or a chunked body that plainly ends with the terminating
		/// zero-size chunk is recognized as complete.
		/// </summary>
		private static bool ResponseComplete(MemoryStream received, int bodyStart, long contentLength, bool chunked)
		{
			long bodyLength = received.Length - bodyStart;
			if (!chunked)
				return contentLength >= 0 && bodyLength >= contentLength;
			if (bodyLength < 5)
				return false;
			byte[] data = received.GetBuffer();
			long end = received.Length;
			return data[end - 5] == '0' && data[end - 4] == '\r' && data[end - 3] == '\n' && data[end - 2] == '\r' && data[end - 1] == '\n'
				&& (bodyLength == 5 || data[end - 6] == '\n');
		}

		/// <summary>Copies the stream's contents from the given offset into a new array.</summary>
		private static byte[] ToArraySegment(MemoryStream stream, int offset)
		{
			int length = (int)stream.Length - offset;
			byte[] result = new byte[length];
			Array.Copy(stream.GetBuffer(), offset, result, 0, length);
			return result;
		}

		/// <summary>
		/// Decodes a chunked transfer-encoded body.  Because the response is read until the
		/// connection closes, the whole body is available up front and can be decoded in one
		/// pass.  Decoding is best-effort: if the data ends mid-chunk or is malformed, whatever
		/// was decoded so far is returned with complete = false.
		/// </summary>
		private static byte[] DecodeChunkedBody(byte[] data, out bool complete)
		{
			MemoryStream output = new MemoryStream();
			int pos = 0;
			complete = false;
			while (true)
			{
				int lineEnd = IndexOfCrlf(data, pos);
				if (lineEnd < 0)
					break;
				string sizeLine = Encoding.ASCII.GetString(data, pos, lineEnd - pos);
				// Chunk extensions (after ';') are ignored.
				int semicolon = sizeLine.IndexOf(';');
				if (semicolon >= 0)
					sizeLine = sizeLine.Substring(0, semicolon);
				int size;
				if (!int.TryParse(sizeLine.Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out size) || size < 0)
					break;
				pos = lineEnd + 2;
				if (size == 0)
				{
					complete = true; // Trailer headers, if any, are ignored.
					break;
				}
				int available = Math.Min(size, data.Length - pos);
				output.Write(data, pos, available);
				if (available < size)
					break; // The data ends mid-chunk.
				pos += size + 2; // Skip the chunk data and its trailing CRLF.
			}
			return output.ToArray();
		}

		private static int IndexOfCrlf(byte[] data, int from)
		{
			for (int i = Math.Max(from, 0); i + 1 < data.Length; i++)
				if (data[i] == '\r' && data[i + 1] == '\n')
					return i;
			return -1;
		}

		/// <summary>Returns the value of the named response header, or null.  The first line is the status line and is skipped.</summary>
		private static string GetHeader(List<string> headerLines, string name)
		{
			for (int i = 1; i < headerLines.Count; i++)
			{
				int colon = headerLines[i].IndexOf(':');
				if (colon > 0 && string.Equals(headerLines[i].Substring(0, colon).Trim(), name, StringComparison.OrdinalIgnoreCase))
					return headerLines[i].Substring(colon + 1).Trim();
			}
			return null;
		}

		/// <summary>
		/// True when the body should be shown as text: a textual Content-Type, or (when no
		/// Content-Type was sent) no NUL bytes near the start of the data.
		/// </summary>
		private static bool LooksTextual(string contentType, byte[] body)
		{
			if (contentType != null)
			{
				string ct = contentType.ToLowerInvariant();
				return ct.StartsWith("text/", StringComparison.Ordinal)
					|| ct.Contains("json") || ct.Contains("xml") || ct.Contains("html")
					|| ct.Contains("javascript") || ct.Contains("urlencoded") || ct.Contains("csv");
			}
			int n = Math.Min(body.Length, 512);
			for (int i = 0; i < n; i++)
				if (body[i] == 0)
					return false;
			return true;
		}

		private void LogBodyText(byte[] body, string contentType)
		{
			Encoding encoding = EncodingFromContentType(contentType);
			string text;
			try
			{
				text = encoding.GetString(body);
			}
			catch (Exception)
			{
				// Some encodings throw on malformed input; UTF-8 substitutes replacement characters instead.
				text = Encoding.UTF8.GetString(body);
			}
			if (text.Length > 0 && text[0] == '\uFEFF')
				text = text.Substring(1); // Strip the byte order mark; it renders as a stray box.
			bool cutOff = text.Length > MaxBodyDisplayChars;
			if (cutOff)
				text = text.Substring(0, MaxBodyDisplayChars);
			AppendLog(NormalizeForLog(text));
			if (cutOff)
				AppendLog("… (body display truncated)");
		}

		private static Encoding EncodingFromContentType(string contentType)
		{
			if (contentType != null)
			{
				int idx = contentType.IndexOf("charset=", StringComparison.OrdinalIgnoreCase);
				if (idx >= 0)
				{
					string charset = contentType.Substring(idx + "charset=".Length).Trim();
					int end = charset.IndexOfAny(new char[] { ';', ' ' });
					if (end >= 0)
						charset = charset.Substring(0, end);
					charset = charset.Trim('"', '\'');
					try
					{
						return Encoding.GetEncoding(charset);
					}
					catch (ArgumentException)
					{
					}
				}
			}
			return Encoding.UTF8;
		}

		/// <summary>
		/// Prepares body text for a TextBox: line endings become CRLF and other control
		/// characters (which render as boxes) are dropped.
		/// </summary>
		private static string NormalizeForLog(string text)
		{
			StringBuilder sb = new StringBuilder(text.Length + 128);
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (c == '\r')
				{
					if (i + 1 < text.Length && text[i + 1] == '\n')
						i++;
					sb.Append("\r\n");
				}
				else if (c == '\n')
					sb.Append("\r\n");
				else if (c >= ' ' || c == '\t')
					sb.Append(c);
			}
			return sb.ToString();
		}

		private static string FormatEndPoint(IPAddress address, int port)
		{
			return (address.AddressFamily == AddressFamily.InterNetworkV6 ? "[" + address + "]" : address.ToString()) + ":" + port;
		}

		private static string DescribeSslProtocol(SslProtocols protocol)
		{
			switch (protocol)
			{
				case SslProtocols.Tls: return "TLS 1.0";
				case SslProtocols.Tls11: return "TLS 1.1";
				case SslProtocols.Tls12: return "TLS 1.2";
				case SslProtocols.Tls13: return "TLS 1.3";
				case SslProtocols.Ssl3: return "SSL 3.0";
				default: return protocol.ToString();
			}
		}

		private static string DescribeKeyExchange(ExchangeAlgorithmType algorithm)
		{
			// Schannel reports ECDH with a value the enum does not define.
			if ((int)algorithm == 44550)
				return "ECDH";
			return algorithm.ToString();
		}

		/// <summary>Explains a failed connection attempt, turning the two most common outcomes into plain language.</summary>
		private static string DescribeConnectError(Exception ex)
		{
			SocketException se = ex as SocketException ?? ex.InnerException as SocketException;
			if (se != null)
			{
				if (se.SocketErrorCode == SocketError.ConnectionRefused)
					return "the connection was actively refused — the host is reachable, but nothing is listening on this port.";
				if (se.SocketErrorCode == SocketError.TimedOut)
					return "the connection attempt timed out — the host may be down, or a firewall may be silently dropping packets.";
				return se.Message;
			}
			if (ex is TimeoutException)
				return "no response within " + (ConnectTimeoutMs / 1000) + " seconds — the host may be down, or a firewall may be silently dropping packets.";
			return ex.Message;
		}

		/// <summary>The exception's message, with the innermost exception's message appended when it adds detail.</summary>
		private static string Describe(Exception ex)
		{
			Exception inner = ex;
			while (inner.InnerException != null)
				inner = inner.InnerException;
			return inner == ex ? ex.Message : ex.Message + "  (" + inner.Message + ")";
		}

		private void AppendLog(string line)
		{
			if (IsDisposed || Disposing)
				return;
			if (txtLog.TextLength > MaxLogLength)
			{
				// Drop the older half of the log at a line boundary so long sessions do not
				// degrade TextBox performance.
				string text = txtLog.Text;
				int cut = text.IndexOf("\r\n", text.Length / 2, StringComparison.Ordinal);
				txtLog.Text = cut >= 0 ? text.Substring(cut + 2) : "";
			}
			// AppendText moves the caret to the end, which keeps the log scrolled to the bottom.
			txtLog.AppendText(line + Environment.NewLine);
		}
	}
}
