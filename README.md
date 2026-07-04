# WindowsNetTool

A small, light-weight GUI application for Windows offering easy access to advanced network configuration capabilities.  Many of the capabilities offered here are difficult to find and some of them would otherwise require the use of command-line tools.

Windows' built-in IP address management interfaces are decades old, a little buggy, and do not properly support DHCP and static IP coexistence. WindowsNetTool is intended as a third-party alternative that solves those issues, and as a home for additional small networking utilities over time.

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

A continuous ping monitor with output styled after Windows' `ping` command. Enter a host name or IP address and press Start; each reply or timeout is appended to an auto-scrolling log with a timestamp, and running totals (sent / received / lost, min / avg / max round-trip time) are shown below the log. A slider adjusts the ping rate from 1 ping per 10 seconds up to 10 pings per second (default: 1 per second), and can be moved while pinging. Pings are sent concurrently, so a slow or unresponsive host does not reduce the configured ping rate. A Windows-style statistics summary is printed whenever pinging stops, and pinging stops automatically when you switch to a different tool. Other tools can link into the Ping tool; the ARP Viewer uses this to begin ping monitoring of a selected address with one click.

### DNS Lookup

Looks up DNS records for a domain name and performs reverse (PTR) lookups when an IP address is entered. The default "Auto" mode queries A and AAAA records together (or PTR for an IP address), and a dropdown selects other record types: ANY, CAA, CNAME, MX, NS, PTR, SOA, SRV, and TXT. Because .NET's built-in resolver can neither query a chosen DNS server nor return record types other than address records, the tool implements the DNS wire protocol directly over UDP (with EDNS0, automatic TCP fallback for truncated responses, and automatic non-EDNS retry for old servers) — no third-party libraries and no locale-dependent `nslookup` output parsing. Results include record TTLs, the answer/authority/additional sections, and response metadata (status, response time, transport, authoritative flag). The DNS server to query is chosen from an editable dropdown listing the system's currently registered DNS servers alongside popular public resolvers (Cloudflare, Google, Quad9, OpenDNS), deduplicated, and a custom server IP address can be typed in directly. International (non-ASCII) domain names are converted to punycode automatically.

### ARP Viewer

Displays the system's ARP table (ARP = address resolution protocol; a way of discovering which MAC addresses own which IP addresses within your LAN). Each entry shows the IP address, MAC address, owning interface, and entry state (Reachable, Stale, Permanent, etc. — more informative than the static/dynamic distinction shown by `arp -a`). Entries can be filtered live by IP address and by MAC address; filters match any part of the address, and the MAC filter accepts `-`, `:`, `.`, or no separators interchangeably. Columns sort with standard header behavior: click to sort ascending (IP addresses sort numerically), click again to reverse, and Shift+click to sort by additional columns. Double-clicking an entry (or pressing the Ping Selected button) jumps to the Ping tool and begins ping monitoring of that address. The table is read through the IP Helper API (`GetIpNetTable2`) rather than by parsing `arp -a` output, so this tool works regardless of the system's display language.

### IP Scanner

Discovers the hosts on a subnet. Every address in the subnet is pinged asynchronously with a tunable limit on the number of pings in flight (default 256), and scanning repeats in continuous waves until stopped, so hosts that miss one wave are caught by a later one. The system ARP table is merged into the results each wave, which fills in MAC addresses and also surfaces hosts that don't answer pings (shown with a ping time of "N/A"). Each discovered host is resolved to a name with reverse DNS — but private addresses are only ever looked up on a local DNS server, never sent to a public resolver. Results show the IP address, latest ping round-trip time, host name, MAC address, and the time of the last reply, updated in place without flicker; columns sort with the same header behavior as the ARP Viewer (click to sort, click again to reverse, Shift+click for multi-column). The subnet dropdown is pre-filled with the subnets of the machine's network interfaces, and any subnet down to /16 in size can be typed in CIDR notation. Double-clicking a host jumps to the Ping tool.

### Hosts File Editor

A notepad-like embedded editor for the system hosts file (`C:\Windows\System32\drivers\etc\hosts`). Edits, saves, and reloads the file with unsaved-change warnings, offers to clear the file's read-only attribute when it blocks saving, preserves the file's original text encoding, and includes a one-click DNS cache flush (`ipconfig /flushdns`) so old entries don't linger after editing.

### Windows Tools

One-click launchers for Windows' built-in networking panels: Network Connections (ncpa.cpl), Network and Sharing Center, Internet Options, Windows Defender Firewall (basic and Advanced Security), and the Settings app pages for Network Status, Ethernet, Wi-Fi, VPN, Proxy, and Advanced Network Settings.

## Building

Open `WindowsNetTool.sln` in Visual Studio 2026, or run `dotnet build` / `msbuild` from the repository root. The distributable exe is produced by the Release configuration at `WindowsNetTool\bin\Release\net48\WindowsNetTool.exe`.

## Limitations

* IPv4 only (for now).
* Reads network configuration by parsing `netsh` output, which is only produced in English on English-language Windows installations. Non-English systems are not currently supported by some integrated tools.

## Future Plans

* IPv6 support.
* Integrate a simple asynchronous/concurrent traceroute tool (far, far faster than traditional traceroute tools).
