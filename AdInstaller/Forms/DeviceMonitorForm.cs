using AdbInstaller.Models;
using AdbInstaller.Services;

namespace AdbInstaller.Forms;

/// <summary>Read-only "what's eating CPU/RAM/battery right now" view -- opened alongside MainForm (non-modal)
/// so the user can keep using the phone in hand while watching this on the PC. Phase 1 on purpose: no
/// force-stop/disable here yet, see the AdbInstaller conversation notes on why that's a separate, guarded
/// follow-up rather than bundled in from the start.</summary>
public partial class DeviceMonitorForm : Form
{
    private readonly DeviceMonitorService _monitor;
    private readonly AdbService _adb;
    private bool _isRefreshingProcesses;
    private List<ProcessInfo> _lastProcesses = [];
    private int _sortColumnIndex = 2; // %CPU
    private bool _sortAscending; // descending by default: highest CPU first, matching the old hardcoded sort

    private string? SelectedSerial => (_deviceCombo.SelectedItem as DeviceItem)?.Serial;

    /// <summary>Design-time only, same trick as <see cref="ScanDevicesDialog"/> -- the Designer needs a
    /// parameterless constructor to host the form; the placeholder AdbService is never actually invoked since
    /// the Designer never fires Load.</summary>
    public DeviceMonitorForm() : this(new AdbService(string.Empty))
    {
    }

    public DeviceMonitorForm(AdbService adb)
    {
        _adb = adb;
        _monitor = new DeviceMonitorService(adb.AdbPath);
        InitializeComponent();
    }

    private async void DeviceMonitorForm_Load(object? sender, EventArgs e) => await ReloadDevicesAsync();

    private void DeviceMonitorForm_FormClosed(object? sender, FormClosedEventArgs e) => _pollTimer.Stop();

    private async void RefreshDevicesButton_Click(object? sender, EventArgs e) => await ReloadDevicesAsync();

    private async Task ReloadDevicesAsync()
    {
        string? previouslySelected = SelectedSerial;
        List<DeviceItem> devices;
        try { devices = (await _adb.ListDevicesAsync()).Where(d => d.IsConnected).ToList(); }
        catch (Exception ex) { _status.Text = "Không lấy được danh sách thiết bị: " + ex.Message; return; }

        _deviceCombo.Items.Clear();
        foreach (DeviceItem device in devices) _deviceCombo.Items.Add(device);
        if (devices.Count == 0)
        {
            _status.Text = "Không có thiết bị nào đang kết nối.";
            return;
        }

        int restoreIndex = devices.FindIndex(d => d.Serial == previouslySelected);
        _deviceCombo.SelectedIndex = restoreIndex >= 0 ? restoreIndex : 0; // triggers DeviceCombo_SelectedIndexChanged
    }

    private async void DeviceCombo_SelectedIndexChanged(object? sender, EventArgs e)
    {
        _pollTimer.Stop();
        _toggleButton.Text = "Bắt đầu theo dõi real-time";
        _grid.Rows.Clear();
        if (SelectedSerial == null) return;
        await RefreshProcessesAsync();
        await RefreshStatsAsync();
    }

    private void ToggleButton_Click(object? sender, EventArgs e)
    {
        if (SelectedSerial == null) return;
        if (_pollTimer.Enabled)
        {
            _pollTimer.Stop();
            _toggleButton.Text = "Bắt đầu theo dõi real-time";
        }
        else
        {
            _pollTimer.Start();
            _toggleButton.Text = "Dừng theo dõi";
        }
    }

    private async void PollTimer_Tick(object? sender, EventArgs e) => await RefreshProcessesAsync();

    /// <summary>Clicking a header re-sorts the CURRENT snapshot immediately (no extra adb round-trip), and the
    /// chosen column/direction is remembered so the next real-time tick's refresh re-applies it too -- otherwise
    /// a manual sort would just get overwritten by the hardcoded CPU-descending order 1.5s later.</summary>
    private void Grid_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (_sortColumnIndex == e.ColumnIndex) _sortAscending = !_sortAscending;
        else { _sortColumnIndex = e.ColumnIndex; _sortAscending = true; }
        RenderGrid();
    }

    private async Task RefreshProcessesAsync()
    {
        // Timer tick can fire again before a slow (e.g. over Wi-Fi) previous call returns; skipping instead of
        // queuing keeps the grid from ever showing two snapshots applied out of order.
        if (_isRefreshingProcesses || SelectedSerial is not { } serial) return;
        _isRefreshingProcesses = true;
        try
        {
            _lastProcesses = await _monitor.ListTopProcessesAsync(serial);
            RenderGrid();
            _status.Text = $"Cập nhật lúc {DateTime.Now:HH:mm:ss} -- {_lastProcesses.Count} tiến trình.";
        }
        catch (Exception ex)
        {
            _status.Text = "Không lấy được danh sách tiến trình: " + ex.Message;
        }
        finally
        {
            _isRefreshingProcesses = false;
        }
    }

    private void RenderGrid()
    {
        IEnumerable<ProcessInfo> sorted = _sortColumnIndex switch
        {
            0 => _sortAscending ? _lastProcesses.OrderBy(p => p.Pid) : _lastProcesses.OrderByDescending(p => p.Pid),
            1 => _sortAscending ? _lastProcesses.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase) : _lastProcesses.OrderByDescending(p => p.Name, StringComparer.OrdinalIgnoreCase),
            2 => _sortAscending ? _lastProcesses.OrderBy(p => p.CpuPercent) : _lastProcesses.OrderByDescending(p => p.CpuPercent),
            3 => _sortAscending ? _lastProcesses.OrderBy(p => p.ResidentKb) : _lastProcesses.OrderByDescending(p => p.ResidentKb),
            _ => _lastProcesses,
        };

        _grid.Rows.Clear();
        foreach (ProcessInfo p in sorted)
            _grid.Rows.Add(p.Pid, p.Name, p.CpuPercent.ToString("0.0"), p.ResidentMb.ToString("0"));

        foreach (DataGridViewColumn column in _grid.Columns) column.HeaderCell.SortGlyphDirection = SortOrder.None;
        _grid.Columns[_sortColumnIndex].HeaderCell.SortGlyphDirection = _sortAscending ? SortOrder.Ascending : SortOrder.Descending;
    }

    private async void RefreshStatsButton_Click(object? sender, EventArgs e) => await RefreshStatsAsync();

    private async Task RefreshStatsAsync()
    {
        if (SelectedSerial is not { } serial) return;
        try
        {
            MemoryInfo? mem = await _monitor.GetMemoryInfoAsync(serial);
            _memLabel.Text = mem == null
                ? "RAM: không đọc được"
                : $"RAM: {mem.UsedMb:0} / {mem.TotalMb:0} MB ({mem.UsedPercent:0}% đang dùng)";

            BatteryInfo? battery = await _monitor.GetBatteryInfoAsync(serial);
            _batteryLabel.Text = battery == null
                ? "Pin: không đọc được"
                : $"Pin: {battery.LevelPercent}% -- {battery.TemperatureCelsius:0.0}°C{(battery.IsCharging ? " (đang sạc)" : "")}";
        }
        catch (Exception ex)
        {
            _status.Text = "Không lấy được RAM/Pin: " + ex.Message;
        }
    }
}
