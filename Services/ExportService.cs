using System.IO;
using System.Text;
using System.Text.Json;
using NetScan.Models;

namespace NetScan.Services;

public static class ExportService
{
    public static void ExportCsv(IEnumerable<DeviceInfo> devices, string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("IP Address,Hostname,MAC Address,Vendor,Ping (ms),Open Ports,Last Seen");

        foreach (var d in devices)
        {
            var ports = PortScanner.FormatPorts(d.OpenPorts).Replace(",", ";");
            sb.AppendLine($"\"{d.IpAddress}\",\"{d.Hostname}\",\"{d.MacAddress}\"," +
                          $"\"{d.Vendor}\",{d.ResponseTimeMs},\"{ports}\"," +
                          $"\"{d.LastSeen:yyyy-MM-dd HH:mm:ss}\"");
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    public static void ExportJson(IEnumerable<DeviceInfo> devices, string filePath)
    {
        var data = devices.Select(d => new
        {
            d.IpAddress,
            d.Hostname,
            d.MacAddress,
            d.Vendor,
            d.ResponseTimeMs,
            OpenPorts = d.OpenPorts.ToList(),
            LastSeen = d.LastSeen.ToString("O")
        });

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json, Encoding.UTF8);
    }
}
