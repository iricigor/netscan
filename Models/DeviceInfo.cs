using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NetScan.Models;

public class DeviceInfo : INotifyPropertyChanged
{
    private string _hostname = "Resolving...";
    private string _vendor = "Looking up...";
    private string _status = "Online";
    private ObservableCollection<int> _openPorts = new();
    private bool _isPortScanning;

    public string IpAddress { get; set; } = "";
    public string MacAddress { get; set; } = "N/A";
    public long ResponseTimeMs { get; set; }
    public System.DateTime LastSeen { get; set; } = System.DateTime.Now;

    public string Hostname
    {
        get => _hostname;
        set { _hostname = value; OnPropertyChanged(); }
    }

    public string Vendor
    {
        get => _vendor;
        set { _vendor = value; OnPropertyChanged(); }
    }

    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    public bool IsPortScanning
    {
        get => _isPortScanning;
        set { _isPortScanning = value; OnPropertyChanged(); OnPropertyChanged(nameof(OpenPortsDisplay)); }
    }

    public ObservableCollection<int> OpenPorts
    {
        get => _openPorts;
        set
        {
            _openPorts = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(OpenPortsDisplay));
        }
    }

    public string OpenPortsDisplay
    {
        get
        {
            if (IsPortScanning) return "Scanning...";
            if (OpenPorts.Count == 0) return "-";
            return string.Join(", ", OpenPorts);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
