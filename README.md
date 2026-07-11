# WindowsNetTool

WindowsNetTool is a collection of network configuration and diagnostic tools for Windows.  Many of the capabilities offered here are difficult to find natively in the OS and some of them would otherwise require the use of command-line tools.

Many of Windows' built-in IP address management interfaces are decades old, a little buggy, and do not properly support DHCP and static IP coexistence. WindowsNetTool is intended as a third-party alternative that solves those issues, and as a home for additional small networking utilities.

The app requires administrator privileges (UAC elevation is requested automatically) and is built with .NET Framework 4.8, which is preinstalled on Windows 10 v1903+ and Windows 11. The Release build is a small, portable, single-file exe with no runtime installation and no dependencies.

## Included Tools

### IP Configuration

A general-purpose IPv4 configuration interface for network interfaces, built on `netsh`.

* Lists each interface's IPv4 addresses with subnet mask and source (static or DHCP).
* Adds and deletes static IP addresses, including on interfaces that have DHCP enabled. DHCP+static coexistence is achieved by automatically enabling the interface's `dhcpstaticipcoexistence` option, which Windows' own GUI does not support.
* Toggles DHCP on and off per interface. Windows removes static addresses when the DHCP setting changes, so the tool snapshots them first and automatically restores any that disappear. When disabling DHCP, the current default gateway is preserved, DHCP-provided DNS servers are preserved by converting them to static DNS configuration, and the current DHCP-assigned address can be converted to a static address in one step.
* Safety checks when adding an address: input validation, and detection of addresses already assigned to the same interface or to a different interface (which would create an address conflict) before any change is made.
* Manages default gateways: add (with optional metric), remove, and reorder. Since Windows prioritizes gateways by metric rather than list order, reordering rewrites the static gateways with explicit metrics 1, 2, 3, ...
* Manages IPv4 DNS servers: ordered list with add, remove, and move up/down, plus switching between static DNS and DHCP-provided DNS.
* Shows diagnostic information for the selected interface: the name of the Windows network that owns the connection (matching the Network Category tool), connection duration, operational status, link speed, MAC address, and adapter description.
* Renames interfaces (e.g. "Ethernet 3" → "LAN").
* Opens Windows' native connection Status window (duration, speed, Details button, etc.) for the selected interface.

### Network Category

Lists the currently connected networks and lets you view and set the firewall category of each between "Public", "Private", and "Domain" (even in cases where Windows' own settings UI does not offer the choice!). Networks can also be renamed here. Uses the Windows Network List Manager COM API directly.

### Static Routes

Manages persistent IPv4 static routes (routes that survive reboots, via `netsh` with `store=persistent`). Lists existing persistent routes with destination prefix, interface, next hop, and metric; adds new routes (blank next hop = on-link route, blank metric = automatic) and deletes existing ones. Duplicate routes are detected before adding.

### Ping

A continuous ping monitor with output styled after Windows' `ping` command. Enter a host name or IP address and press Start; each reply or timeout is appended to an auto-scrolling log with a timestamp, and running totals (sent / received / lost, min / avg / max round-trip time) are shown below the log. A slider adjusts the ping rate from 1 ping per 10 seconds up to 10 pings per second (default: 1 per second), and can be moved while pinging. Pings are sent concurrently, so a slow or unresponsive host does not reduce the configured ping rate. A Windows-style statistics summary is printed whenever pinging stops, and pinging stops automatically when you switch to a different tool. Other tools can link into the Ping tool; the ARP Viewer, NDP Viewer, IP Scanner, Device List, and Traceroute tools use this to begin ping monitoring of a selected address with one click.

### Traceroute

An asynchronous/concurrent traceroute, far faster than traditional traceroute tools. Instead of probing one hop at a time and waiting for each reply, every TTL from 1 to 30 is probed simultaneously, so the full route to a destination is typically discovered in about one round-trip time. To ride out packet loss, each unanswered hop is re-probed up to 15 times spread evenly across 3 seconds; a hop that has answered is not probed again, and the trace ends as soon as every hop up to the destination has answered. Echo requests carry the same 32-byte payload as Windows' `ping` command. Host names are filled in with concurrent reverse DNS lookups as hops are discovered, and routers that report the destination unreachable are labeled with the reason. A "Prefer IPv4" checkbox (checked by default) selects which address family to use when a host name resolves to both, falling back to the other family when the preferred one is unavailable. Double-clicking a hop jumps to the Ping tool and begins ping monitoring of that router.

### DNS Lookup

Looks up DNS records for a domain name and performs reverse (PTR) lookups when an IP address is entered. The default "Auto" mode queries A and AAAA records together (or PTR for an IP address), and a dropdown selects other record types: ANY, CAA, CNAME, MX, NS, PTR, SOA, SRV, and TXT. Because .NET's built-in resolver can neither query a chosen DNS server nor return record types other than address records, the tool implements the DNS wire protocol directly over UDP (with EDNS0, automatic TCP fallback for truncated responses, and automatic non-EDNS retry for old servers) — no third-party libraries and no locale-dependent `nslookup` output parsing. Results include record TTLs, the answer/authority/additional sections, and response metadata (status, response time, transport, authoritative flag). The DNS server to query is chosen from an editable dropdown listing the system's currently registered DNS servers alongside popular public resolvers (Cloudflare, Google, Quad9, OpenDNS), deduplicated, and a custom server IP address can be typed in directly. International (non-ASCII) domain names are converted to punycode automatically.

### TCP Connection Test

Tests TCP connectivity to any host and port, in two modes. In "Connect only" mode, a connection is opened and immediately closed to show whether the port is open, with the failure reason distinguishing a closed port (connection actively refused) from an unreachable host (timeout, suggesting the host is down or a firewall is dropping packets). In "HTTP GET" mode, an HTTP request for a URL is sent over the connection — with TLS when the URL is `https` — and the log narrates every phase with timing: DNS resolution (each resolved address is tried until one connects), TCP connect, TLS handshake (negotiated protocol version and cipher, plus the server certificate's subject, issuer, validity dates, and subject alternative names), the exact request sent, and the response status line, headers, and body (chunked responses are decoded; binary bodies are summarized rather than dumped). Because the connection target and the requested URL are independent, a website can be tested on a specific server — behind a load balancer, or before a DNS change takes effect — by entering that server's address as the Host while the URL and Host header name the site. Redirects are deliberately not followed, and certificate problems (expired, wrong name, untrusted chain) are reported in detail without stopping the test. The HTTP client is implemented directly on `TcpClient`/`SslStream` — no third-party libraries.

### ARP Viewer

Displays the system's ARP table (ARP = address resolution protocol; a way of discovering which MAC addresses own which IP addresses within your LAN). Each entry shows the IP address, MAC address, owning interface, and entry state (Reachable, Stale, Permanent, etc. — more informative than the static/dynamic distinction shown by `arp -a`). Entries can be filtered live by IP address and by MAC address; filters match any part of the address, and the MAC filter accepts `-`, `:`, `.`, or no separators interchangeably. Columns sort with standard header behavior: click to sort ascending (IP addresses sort numerically), click again to reverse, and Shift+click to sort by additional columns. Double-clicking an entry (or pressing the Ping Selected button) jumps to the Ping tool and begins ping monitoring of that address. The table is read through the IP Helper API (`GetIpNetTable2`) rather than by parsing `arp -a` output, so this tool works regardless of the system's display language.

### NDP Viewer

The IPv6 counterpart of the ARP Viewer: displays the system's NDP table (NDP = Neighbor Discovery Protocol; IPv6's equivalent to ARP). Each entry shows the IPv6 address, MAC address, owning interface, entry state, and whether the neighbor has announced itself as a router. Entries filter and sort exactly like the ARP Viewer's (live IP and separator-insensitive MAC filters; click, click-again, and Shift+click column sorting). Double-clicking an entry (or pressing the Ping Selected button) jumps to the Ping tool and begins ping monitoring of that address — link-local addresses are passed with their required zone index (e.g. `fe80::1%5`) so pinging them just works. Like the ARP Viewer, the table is read through the IP Helper API (`GetIpNetTable2`) rather than by parsing command-line output, so this tool works regardless of the system's display language.

### IP Scanner

Discovers the hosts on a subnet. Every address in the subnet is pinged asynchronously with a tunable limit on the number of pings in flight (default 256), and scanning repeats in continuous waves until stopped, so hosts that miss one wave are caught by a later one. The system ARP table is merged into the results each wave, which fills in MAC addresses and also surfaces hosts that don't answer pings (shown with a ping time of "N/A"). Each discovered host is resolved to a name with reverse DNS — but private addresses are only ever looked up on a local DNS server, never sent to a public resolver. Results show the IP address, latest ping round-trip time, host name, MAC address, and the time of the last reply, updated in place without flicker; columns sort with the same header behavior as the ARP Viewer (click to sort, click again to reverse, Shift+click for multi-column). The subnet dropdown is pre-filled with the subnets of the machine's network interfaces, and any subnet down to /16 in size can be typed in CIDR notation. Double-clicking a host jumps to the Ping tool.

### Device List

Builds an inventory of the machines on a network by combining the discovery methods of the IP Scanner, ARP Viewer, and NDP Viewer. Every IPv4 address in the selected subnet is pinged in repeating waves, the system's ARP (IPv4) and NDP (IPv6) neighbor tables are merged into the results each wave, and everything is correlated by MAC address so each physical device gets exactly one row listing all of its known IPv4 addresses, IPv6 addresses, and reverse-DNS host names, along with its MAC address, latest ping round-trip time, and the time of its last reply. Discovered IPv6 addresses are pinged each wave too, keeping IPv6 reachability current. The local machine's own adapter is included and labeled "This PC", the default gateway is labeled "Gateway", and devices that advertise themselves as IPv6 routers are labeled "Router". To scale down to small windows, the list shows one compact row per device (extra addresses are summarized as e.g. "192.168.0.10 (+2)") and a detail pane below shows everything known about the selected device as selectable, copyable text. A single filter box live-matches against host names, every address, the labels, and the MAC address (ignoring separators), and columns sort with the same header behavior as the other list tools. Reverse DNS follows the IP Scanner's privacy rule: private addresses are only ever looked up on a local DNS server, never sent to a public resolver. Double-clicking a device jumps to the Ping tool.

### Hosts File Editor

A notepad-like embedded editor for the system hosts file (`C:\Windows\System32\drivers\etc\hosts`). Edits, saves, and reloads the file with unsaved-change warnings, offers to clear the file's read-only attribute when it blocks saving, preserves the file's original text encoding, and includes a one-click DNS cache flush (`ipconfig /flushdns`) so old entries don't linger after editing.

### Links / Shortcuts

One-click launchers for many of Windows' built-in networking panels, and links to this repository and the [PingTracer](https://github.com/bp2008/PingTracer) repository.

## Building

Open `WindowsNetTool.sln` in Visual Studio 2026, or run `dotnet build` / `msbuild` from the repository root. The distributable exe is produced by the Release configuration at `WindowsNetTool\bin\Release\net48\WindowsNetTool.exe`.

## Limitations

* IPv4 focused.  IPv6 is covered by the diagnostic tools (NDP Viewer, Device List, Ping, Traceroute, DNS Lookup) but not yet by the configuration tools.
* Reads network configuration by parsing `netsh` output, which is only produced in English on English-language Windows installations. Non-English systems are not currently supported by some integrated tools.

## Future Plans

* IPv6 configuration support.
* Listening Ports - equivalent to Resource Monitor > Network > Listening Ports, but with added filter support.
