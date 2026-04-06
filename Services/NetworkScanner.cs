using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using NetScan.Models;

namespace NetScan.Services;

public record ScanProgress(int Current, int Total, string CurrentIp);

public class NetworkScanner
{
    [DllImport("iphlpapi.dll", ExactSpelling = true)]
    private static extern int SendARP(int destIP, int srcIP, byte[] pMacAddr, ref int phyAddrLen);

    public static (string BaseIp, int Start, int End) DetectLocalSubnet()
    {
        try
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                if (ni.NetworkInterfaceType is NetworkInterfaceType.Loopback
                    or NetworkInterfaceType.Tunnel) continue;

                foreach (var addr in ni.GetIPProperties().UnicastAddresses)
                {
                    if (addr.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                    var ip = addr.Address.ToString();
                    var parts = ip.Split('.');
                    if (parts.Length == 4 && parts[0] != "169") // skip APIPA
                    {
                        var baseIp = $"{parts[0]}.{parts[1]}.{parts[2]}";
                        return (baseIp, 1, 254);
                    }
                }
            }
        }
        catch { }
        return ("192.168.1", 1, 254);
    }

    public async Task<List<DeviceInfo>> ScanAsync(
        string baseIp, int start, int end,
        IProgress<ScanProgress>? progress,
        CancellationToken ct)
    {
        var results = new ConcurrentBag<DeviceInfo>();
        int total = end - start + 1;
        int completed = 0;

        // Use 50 concurrent threads
        using var semaphore = new SemaphoreSlim(50, 50);
        var tasks = new List<Task>();

        for (int i = start; i <= end; i++)
        {
            if (ct.IsCancellationRequested) break;
            string ip = $"{baseIp}.{i}";

            await semaphore.WaitAsync(ct).ConfigureAwait(false);
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var device = await ProbHostAsync(ip, ct).ConfigureAwait(false);
                    if (device != null) results.Add(device);
                }
                catch (OperationCanceledException) { }
                finally
                {
                    semaphore.Release();
                    int c = Interlocked.Increment(ref completed);
                    progress?.Report(new ScanProgress(c, total, ip));
                }
            }, ct));
        }

        try { await Task.WhenAll(tasks).ConfigureAwait(false); }
        catch (OperationCanceledException) { }

        return results
            .OrderBy(d => d.IpAddress.Split('.').Select(int.Parse).Last())
            .ToList();
    }

    private static async Task<DeviceInfo?> ProbHostAsync(string ip, CancellationToken ct)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(ip, 800).ConfigureAwait(false);
            if (reply.Status != IPStatus.Success) return null;

            var device = new DeviceInfo
            {
                IpAddress = ip,
                ResponseTimeMs = reply.RoundtripTime,
                LastSeen = DateTime.Now
            };

            // Get MAC via ARP (works on same subnet)
            device.MacAddress = GetMacAddress(ip);

            // Vendor lookup from OUI
            device.Vendor = MacVendorLookup.Lookup(device.MacAddress);

            // Resolve hostname (fire-and-forget style to not block)
            _ = Task.Run(async () =>
            {
                try
                {
                    var entry = await Dns.GetHostEntryAsync(ip).ConfigureAwait(false);
                    device.Hostname = entry.HostName;
                }
                catch
                {
                    device.Hostname = ip;
                }
            }, ct);

            return device;
        }
        catch
        {
            return null;
        }
    }

    private static string GetMacAddress(string ipAddress)
    {
        try
        {
            // SendARP only works for hosts on the same subnet
            var bytes = IPAddress.Parse(ipAddress).GetAddressBytes();
            int destIp = BitConverter.ToInt32(bytes, 0);

            byte[] mac = new byte[6];
            int len = mac.Length;
            int result = SendARP(destIp, 0, mac, ref len);
            if (result != 0 || len == 0) return "N/A";

            return string.Join(":", mac.Take(len).Select(b => b.ToString("X2")));
        }
        catch
        {
            return "N/A";
        }
    }
}
