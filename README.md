# NetScan — Network Inventory Tool

A Windows desktop app built with C# + WPF (.NET 8), inspired by Fing.

## Features

| Feature | Details |
|---|---|
| **Ping sweep** | Scans up to 254 hosts concurrently (50 at a time) |
| **MAC address** | Retrieved via Windows `SendARP` (same-subnet devices) |
| **Vendor lookup** | Built-in OUI database with 150+ common prefixes |
| **Hostname** | Async DNS reverse-lookup |
| **Port scanner** | 36 well-known ports; full scan or quick (10-port) scan |
| **Export** | CSV and JSON export with one click |

## Prerequisites

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Build & Run

```powershell
cd NetScan
dotnet run
```

Or build a release executable:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
# Output: bin\Release\net8.0-windows\win-x64\publish\NetScan.exe
```

## Usage

1. **Subnet** is auto-detected on launch — adjust if needed
2. Click **▶ Scan Network** to discover all live hosts
3. Click any row to open the **detail panel** at the bottom
4. Click **🔍 Scan Ports** or **⚡ Quick Scan** to probe that device
5. Right-click any row for Copy IP / Copy MAC / Re-ping
6. Use **⬇ CSV** or **⬇ JSON** to export the inventory

## Notes

- MAC addresses only work for devices on **the same subnet** (ARP limitation)
- Port scanning may trigger Windows Firewall notifications — this is normal
- No admin rights required for basic operation
- Vendor lookup is offline — unknown devices show "Unknown"

## Project Structure

```
NetScan/
├── NetScan.csproj
├── App.xaml / App.xaml.cs
├── MainWindow.xaml / MainWindow.xaml.cs
├── Models/
│   └── DeviceInfo.cs
└── Services/
    ├── NetworkScanner.cs   ← ping sweep + ARP + DNS
    ├── PortScanner.cs      ← TCP connect scanner
    ├── MacVendorLookup.cs  ← OUI → vendor name
    └── ExportService.cs    ← CSV + JSON export
```
