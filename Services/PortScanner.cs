using System.Net.Sockets;

namespace NetScan.Services;

public class PortScanner
{
    public static readonly Dictionary<int, string> WellKnownPorts = new()
    {
        {21,   "FTP"},
        {22,   "SSH"},
        {23,   "Telnet"},
        {25,   "SMTP"},
        {53,   "DNS"},
        {80,   "HTTP"},
        {110,  "POP3"},
        {143,  "IMAP"},
        {443,  "HTTPS"},
        {445,  "SMB"},
        {548,  "AFP"},
        {554,  "RTSP"},
        {631,  "IPP (Print)"},
        {993,  "IMAPS"},
        {995,  "POP3S"},
        {1080, "SOCKS"},
        {1194, "OpenVPN"},
        {1433, "MSSQL"},
        {1883, "MQTT"},
        {3306, "MySQL"},
        {3389, "RDP"},
        {4434, "HTTPS Alt"},
        {5000, "UPnP/Dev"},
        {5357, "WSD"},
        {5900, "VNC"},
        {6881, "BitTorrent"},
        {8008, "HTTP Alt"},
        {8080, "HTTP Proxy"},
        {8443, "HTTPS Alt"},
        {8883, "MQTT TLS"},
        {9090, "WebAdmin"},
        {9100, "RAW Print"},
        {27017,"MongoDB"},
        {32400,"Plex"},
        {49152,"UPnP"},
        {51413,"Transmission"},
    };

    public static int[] DefaultPorts => WellKnownPorts.Keys.ToArray();

    /// <summary>Scan given ports on an IP. Returns list of open port numbers.</summary>
    public async Task<List<int>> ScanAsync(
        string ipAddress,
        int[]? ports = null,
        int timeoutMs = 800,
        CancellationToken ct = default)
    {
        ports ??= DefaultPorts;
        var open = new System.Collections.Concurrent.ConcurrentBag<int>();

        using var semaphore = new SemaphoreSlim(30, 30);
        var tasks = ports.Select(async port =>
        {
            await semaphore.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (await IsPortOpenAsync(ipAddress, port, timeoutMs, ct).ConfigureAwait(false))
                    open.Add(port);
            }
            finally { semaphore.Release(); }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);
        return open.OrderBy(p => p).ToList();
    }

    private static async Task<bool> IsPortOpenAsync(
        string ip, int port, int timeoutMs, CancellationToken ct)
    {
        try
        {
            using var tcp = new TcpClient();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeoutMs);
            await tcp.ConnectAsync(ip, port, cts.Token).ConfigureAwait(false);
            return true;
        }
        catch { return false; }
    }

    /// <summary>Returns a display string like "80 (HTTP), 443 (HTTPS)"</summary>
    public static string FormatPorts(IEnumerable<int> ports)
    {
        return string.Join(", ", ports.Select(p =>
            WellKnownPorts.TryGetValue(p, out var name) ? $"{p} ({name})" : p.ToString()));
    }
}
