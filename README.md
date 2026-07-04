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

A continuous ping monitor with output styled after Windows' `ping` command. Enter a host name or IP address and press Start; each reply or timeout is appended to an auto-scrolling log with a timestamp, and running totals (sent / received / lost, min / avg / max round-trip time) are shown below the log. A slider adjusts the ping rate from 1 ping per 10 seconds up to 10 pings per second (default: 1 per second), and can be moved while pinging. Pings are sent concurrently, so a slow or unresponsive host does not reduce the configured ping rate. A Windows-style statistics summary is printed whenever pinging stops, and pinging stops automatically when you switch to a different tool. Other tools can link into the Ping tool, so a future ARP viewer or IP scanner can begin ping monitoring of a selected address with one click.

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
* Integrate an IP scanner.
* Integrate a filterable ARP viewer (ARP = address resolution protocol; a way of discovering which MAC addresses own which IP addresses within your LAN).
* Integrate a simple asynchronous/concurrent traceroute tool (far, far faster than traditional traceroute tools).
* Integrate simple DNS lookup tools (will be somewhat limited by .NET 4.8 APIs).
