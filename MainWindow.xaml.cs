using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using NetScan.Models;
using NetScan.Services;

namespace NetScan;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<DeviceInfo> _devices = new();
    private CancellationTokenSource? _scanCts;
    private DeviceInfo? _selectedDevice;
    private readonly PortScanner _portScanner = new();

    public MainWindow()
    {
        InitializeComponent();
        DevicesGrid.ItemsSource = _devices;
        DetectAndFillSubnet();
    }

    // ── Init ────────────────────────────────────────────────────────────────

    private void DetectAndFillSubnet()
    {
        var (baseIp, start, end) = NetworkScanner.DetectLocalSubnet();
        IpBaseBox.Text = baseIp;
        StartRangeBox.Text = start.ToString();
        EndRangeBox.Text = end.ToString();
        NetworkInfoText.Text = $"Local subnet: {baseIp}.{start}–{end}";
    }

    // ── Scan ────────────────────────────────────────────────────────────────

    private async void ScanButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateInputs(out var baseIp, out int start, out int end)) return;

        // Reset UI
        _devices.Clear();
        SetScanningState(true);
        StatusText.Text = $"Scanning {baseIp}.{start}–{end} …";
        DeviceCountText.Text = "Scanning…";
        ScanProgressBar.Value = 0;
        ProgressText.Text = "";
        DetailPanel.Visibility = Visibility.Collapsed;

        _scanCts = new CancellationTokenSource();
        var ct = _scanCts.Token;

        var progress = new Progress<ScanProgress>(p =>
        {
            ScanProgressBar.Value = (double)p.Current / p.Total * 100;
            ProgressText.Text = $"Probing {p.CurrentIp}   ({p.Current}/{p.Total})";
        });

        var scanner = new NetworkScanner();
        List<DeviceInfo> found;

        try
        {
            found = await Task.Run(
                () => scanner.ScanAsync(baseIp, start, end, progress, ct), ct);
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "Scan cancelled.";
            SetScanningState(false);
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Scan error: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            SetScanningState(false);
            return;
        }

        foreach (var d in found) _devices.Add(d);

        ScanProgressBar.Value = 100;
        ProgressText.Text = "Complete";
        StatusText.Text = $"Scan finished. {found.Count} device(s) found.";
        DeviceCountText.Text = found.Count == 0
            ? "No devices responded."
            : $"{found.Count} device(s) online on {baseIp}.0/24";

        bool hasDevices = found.Count > 0;
        ExportCsvBtn.IsEnabled = hasDevices;
        ExportJsonBtn.IsEnabled = hasDevices;
        SetScanningState(false);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        _scanCts?.Cancel();
        StatusText.Text = "Cancelling scan…";
    }

    // ── Port scanning ────────────────────────────────────────────────────────

    private async void ScanPortsBtn_Click(object sender, RoutedEventArgs e)
        => await RunPortScan(_selectedDevice, PortScanner.DefaultPorts);

    private async void QuickScanPortsBtn_Click(object sender, RoutedEventArgs e)
    {
        int[] quickPorts = { 22, 80, 443, 445, 3389, 5900, 8080, 8443, 554, 21 };
        await RunPortScan(_selectedDevice, quickPorts);
    }

    private async void CtxScanPorts_Click(object sender, RoutedEventArgs e)
        => await RunPortScan(DevicesGrid.SelectedItem as DeviceInfo, PortScanner.DefaultPorts);

    private async Task RunPortScan(DeviceInfo? device, int[] ports)
    {
        if (device == null) return;

        device.IsPortScanning = true;
        device.OpenPorts.Clear();
        ScanPortsBtn.IsEnabled = false;
        QuickScanPortsBtn.IsEnabled = false;
        StatusText.Text = $"Port-scanning {device.IpAddress} ({ports.Length} ports)…";

        try
        {
            var openPorts = await _portScanner.ScanAsync(device.IpAddress, ports);
            device.OpenPorts = new System.Collections.ObjectModel.ObservableCollection<int>(openPorts);
            device.IsPortScanning = false;

            string result = openPorts.Count > 0
                ? PortScanner.FormatPorts(openPorts)
                : "No open ports found";

            StatusText.Text = $"Port scan complete for {device.IpAddress}: {result}";
            UpdateDetailPanel(device);
        }
        catch (Exception ex)
        {
            device.IsPortScanning = false;
            StatusText.Text = $"Port scan error: {ex.Message}";
        }
        finally
        {
            ScanPortsBtn.IsEnabled = true;
            QuickScanPortsBtn.IsEnabled = true;
        }
    }

    // ── Selection / detail panel ─────────────────────────────────────────────

    private void DevicesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedDevice = DevicesGrid.SelectedItem as DeviceInfo;
        if (_selectedDevice == null)
        {
            DetailPanel.Visibility = Visibility.Collapsed;
            return;
        }
        UpdateDetailPanel(_selectedDevice);
        DetailPanel.Visibility = Visibility.Visible;
    }

    private void UpdateDetailPanel(DeviceInfo d)
    {
        DetailIp.Text = d.IpAddress;
        DetailHostname.Text = d.Hostname;
        DetailMac.Text = d.MacAddress;
        DetailVendor.Text = d.Vendor;
        DetailPorts.Text = d.OpenPorts.Count > 0
            ? PortScanner.FormatPorts(d.OpenPorts)
            : d.IsPortScanning ? "Scanning…" : "Not yet scanned — click Scan Ports";
    }

    // ── Context menu ─────────────────────────────────────────────────────────

    private void CtxCopyIp_Click(object sender, RoutedEventArgs e)
    {
        if (DevicesGrid.SelectedItem is DeviceInfo d)
            Clipboard.SetText(d.IpAddress);
    }

    private void CtxCopyMac_Click(object sender, RoutedEventArgs e)
    {
        if (DevicesGrid.SelectedItem is DeviceInfo d)
            Clipboard.SetText(d.MacAddress);
    }

    private async void CtxReping_Click(object sender, RoutedEventArgs e)
    {
        if (DevicesGrid.SelectedItem is not DeviceInfo d) return;
        StatusText.Text = $"Re-pinging {d.IpAddress}…";
        using var ping = new System.Net.NetworkInformation.Ping();
        var reply = await ping.SendPingAsync(d.IpAddress, 1500);
        if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
        {
            d.ResponseTimeMs = reply.RoundtripTime;
            d.LastSeen = DateTime.Now;
            StatusText.Text = $"{d.IpAddress} responded in {reply.RoundtripTime} ms.";
        }
        else
        {
            StatusText.Text = $"{d.IpAddress} did not respond ({reply.Status}).";
        }
    }

    // ── Export ────────────────────────────────────────────────────────────────

    private void ExportCsvBtn_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new SaveFileDialog
        {
            Title = "Export device inventory as CSV",
            Filter = "CSV files (*.csv)|*.csv",
            FileName = $"netscan_{DateTime.Now:yyyyMMdd_HHmm}.csv"
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            ExportService.ExportCsv(_devices, dlg.FileName);
            StatusText.Text = $"Exported {_devices.Count} devices → {Path.GetFileName(dlg.FileName)}";
            if (MessageBox.Show("Export complete. Open file?", "NetScan",
                    MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                System.Diagnostics.Process.Start("explorer.exe", dlg.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Export failed: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExportJsonBtn_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new SaveFileDialog
        {
            Title = "Export device inventory as JSON",
            Filter = "JSON files (*.json)|*.json",
            FileName = $"netscan_{DateTime.Now:yyyyMMdd_HHmm}.json"
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            ExportService.ExportJson(_devices, dlg.FileName);
            StatusText.Text = $"Exported {_devices.Count} devices → {Path.GetFileName(dlg.FileName)}";
            if (MessageBox.Show("Export complete. Open file?", "NetScan",
                    MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                System.Diagnostics.Process.Start("explorer.exe", dlg.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Export failed: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private bool ValidateInputs(out string baseIp, out int start, out int end)
    {
        baseIp = IpBaseBox.Text.Trim();
        start = 0; end = 0;
        var parts = baseIp.Split('.');
        if (parts.Length != 3 || parts.Any(p => !int.TryParse(p, out _)))
        {
            MessageBox.Show("Enter a valid subnet base (e.g. 192.168.1)",
                "Invalid input", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        if (!int.TryParse(StartRangeBox.Text, out start) ||
            !int.TryParse(EndRangeBox.Text, out end) ||
            start < 1 || end > 254 || start > end)
        {
            MessageBox.Show("Start must be 1–254 and ≤ End.",
                "Invalid range", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        return true;
    }

    private void SetScanningState(bool scanning)
    {
        ScanButton.IsEnabled = !scanning;
        CancelButton.IsEnabled = scanning;
        IpBaseBox.IsEnabled = !scanning;
        StartRangeBox.IsEnabled = !scanning;
        EndRangeBox.IsEnabled = !scanning;
    }
}
