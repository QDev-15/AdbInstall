using AdbInstaller.Models;
using AdbInstaller.Services;

namespace AdbInstaller.Forms;

public partial class MainForm : Form
{
    private readonly List<PackageFile> _files = [];
    private AdbService? _adb;
    private bool _operationRunning;
    private int _progressPercent;
    private const int ActionColumnIndex = 3;

    public MainForm()
    {
        InitializeComponent();
        // ApplicationIcon in the csproj only sets the compiled .exe's own file icon (what Explorer shows); the
        // running window's title bar / taskbar button is a separate Form.Icon that stays the generic WinForms
        // default unless set here too -- reuse the already-embedded exe icon rather than shipping app.ico twice.
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { /* best-effort */ }
    }

    private async void MainForm_Load(object? sender, EventArgs e) => await InitializeAdbAsync();

    private async void DevicePoll_Tick(object? sender, EventArgs e) => await RefreshDevicesAsync();

    private void FileList_SelectedIndexChanged(object? sender, EventArgs e) => UpdateInstallButtonVisibility();

    private void FileList_DragEnter(object? sender, DragEventArgs e) =>
        e.Effect = e.Data?.GetDataPresent(DataFormats.FileDrop) == true ? DragDropEffects.Copy : DragDropEffects.None;

    private void FileList_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is not string[] paths) return;
        AddFiles(paths);
    }

    private void AddButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog { Filter = "Gói ứng dụng Android (*.apk;*.aab)|*.apk;*.aab", Multiselect = true, Title = "Chọn file APK/AAB" };
        if (dialog.ShowDialog(this) == DialogResult.OK) AddFiles(dialog.FileNames);
    }

    private void RemoveButton_Click(object? sender, EventArgs e)
    {
        if (_fileList.SelectedIndex is int i and >= 0)
        {
            _files.RemoveAt(i);
            _fileList.Items.RemoveAt(i);
        }
    }

    private void DeviceGrid_SelectionChanged(object? sender, EventArgs e) => UpdateInstallButtonVisibility();

    private async void DeviceGrid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != ActionColumnIndex || _adb == null) return;
        DataGridViewRow row = _deviceGrid.Rows[e.RowIndex];
        if (row.Cells["Status"].Value as string == "Đã kết nối") return; // empty button on an already-connected row: no-op
        if (row.Cells["Serial"].Value is not string serial) return;
        SetBusy(true, $"Đang kết nối lại {serial}...");
        (bool ok, string message) = await _adb.ReconnectAsync(serial);
        SetBusy(false, ok ? "Đã kết nối." : "Không kết nối được: " + message);
        await RefreshDevicesAsync();
    }

    private async void RefreshButton_Click(object? sender, EventArgs e) => await RefreshDevicesAsync();

    private async void PairWifiButton_Click(object? sender, EventArgs e) => await ConnectWifiAsync();

    private async void ScanWifiButton_Click(object? sender, EventArgs e) => await ScanWifiAsync();

    /// <summary>"Giám sát thiết bị...": opens non-modally (Show, not ShowDialog) so the user can keep installing
    /// / reconnecting in MainForm while watching live CPU/RAM/battery in the monitor window.</summary>
    private void MonitorButton_Click(object? sender, EventArgs e)
    {
        if (_adb == null) return;
        new DeviceMonitorForm(_adb).Show(this);
    }

    private async void InstallButton_Click(object? sender, EventArgs e) => await InstallAsync();

    private void AddFiles(IEnumerable<string> paths)
    {
        foreach (string path in paths)
        {
            PackageFile? file = PackageFile.TryCreate(path);
            if (file == null) continue; // not .apk/.aab: silently skipped, matches "drop files in, we only care about packages"
            if (_files.Any(f => string.Equals(f.FullPath, file.FullPath, StringComparison.OrdinalIgnoreCase))) continue;
            _files.Add(file);
            _fileList.Items.Add(file);
        }
    }

    private async Task InitializeAdbAsync()
    {
        AppSettings settings = SettingsStore.Load();
        string? path = settings.AdbPath is { } p && File.Exists(p) ? p : AdbService.FindAdb();
        while (path == null)
        {
            MessageBox.Show(this, "Không tìm thấy adb.exe tự động. Hãy chọn file adb.exe (thường nằm trong platform-tools của Android SDK).",
                "Cần adb.exe", MessageBoxButtons.OK, MessageBoxIcon.Information);
            using var dialog = new OpenFileDialog { Filter = "adb.exe|adb.exe", Title = "Chọn adb.exe" };
            if (dialog.ShowDialog(this) != DialogResult.OK) { Close(); return; }
            path = dialog.FileName;
        }
        settings.AdbPath = path;
        SettingsStore.Save(settings);
        _adb = new AdbService(path);

        await RefreshDevicesAsync();
        _devicePoll.Start();
    }

    private async Task RefreshDevicesAsync()
    {
        if (_adb == null || _operationRunning) return;
        List<DeviceItem> devices;
        try { devices = await _adb.ListDevicesAsync(); }
        catch { return; } // a transient adb hiccup mid-poll should not spam the user with error popups

        string? previouslySelected = _deviceGrid.CurrentRow?.Cells["Serial"].Value as string;
        _deviceGrid.Rows.Clear();
        foreach (DeviceItem device in devices)
        {
            int rowIndex = _deviceGrid.Rows.Add(device.DisplayName, device.Serial, StatusText(device), device.IsConnected ? "" : "Kết nối");
            DataGridViewRow row = _deviceGrid.Rows[rowIndex];
            row.Cells["Status"].Style.ForeColor = device.IsConnected ? Color.FromArgb(0, 120, 0) : Color.FromArgb(160, 90, 0);
            if (device.IsConnected) row.Cells[ActionColumnIndex].ReadOnly = true; // nothing to do for an already-connected device
            if (device.Serial == previouslySelected) row.Selected = true;
        }
        UpdateInstallButtonVisibility();
    }

    private static string StatusText(DeviceItem device) => device.State switch
    {
        "device" => "Đã kết nối",
        "offline" => "Mất kết nối",
        "unauthorized" => "Chưa cấp quyền (kiểm tra màn hình điện thoại)",
        _ => device.State,
    };

    /// <summary>"Kết nối qua Wi-Fi...": merged entry point for both pairing and plain reconnect (owner's report,
    /// 2026-10-28, "2 button ... khá giống nhau") -- an empty <see cref="WifiConnectDialog.Code"/> means the
    /// device was already paired before, so this just runs the IP connect command directly; a filled-in code
    /// means a brand-new pairing, which <see cref="PairAndConnectAsync"/> runs first before its own follow-up
    /// connect.</summary>
    private async Task ConnectWifiAsync()
    {
        if (_adb == null) return;
        using var dialog = new WifiConnectDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        if (dialog.Code.Length == 0) await ConnectAsync(dialog.Ip, dialog.Port);
        else await PairAndConnectAsync(dialog.Ip, dialog.Port, dialog.Code);
    }

    /// <summary>Pairs (needs the one-time code from the phone's "Pair device with pairing code" screen), then
    /// automatically finds and uses the phone's separate "connect" port via a short mDNS re-scan. "adb pair"
    /// alone only registers trust -- it does NOT add the device to "adb devices"; that needs a further
    /// "adb connect" to a DIFFERENT port than the one just paired with, which only the phone's main "Wireless
    /// debugging" screen shows. Without this, the user sees "paired successfully" and then nothing in the
    /// device list, with no obvious next step.</summary>
    private async Task PairAndConnectAsync(string ip, string port, string code)
    {
        if (_adb == null) return;

        SetBusy(true, "Đang ghép nối qua Wi-Fi...");
        (bool pairOk, string pairMessage) = await _adb.PairAsync(ip, port, code);
        if (!pairOk)
        {
            SetBusy(false, pairMessage);
            MessageBox.Show(this, pairMessage, "Ghép nối thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        SetBusy(true, "Đã ghép nối, đang tìm cổng kết nối...");
        string? connectPort = await FindConnectPortAsync(ip);
        if (connectPort == null)
        {
            SetBusy(false, "Đã ghép nối.");
            DialogResult answer = MessageBox.Show(this,
                "Ghép nối thành công, nhưng không tự dò được cổng kết nối qua mDNS (mạng có thể đang chặn mDNS -- xem nút \"Dò mạng Wi-Fi...\" > \"Sửa tường lửa\"). " +
                "Mở màn hình \"Gỡ lỗi không dây\" chính trên điện thoại, xem đúng IP:Cổng hiện ở đó (khác cổng ghép nối vừa nhập), rồi nhập thủ công ngay bây giờ?",
                "Nhập IP:Cổng kết nối", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (answer == DialogResult.Yes) await ConnectByIpAsync(ip);
            return;
        }

        (bool connectOk, string connectMessage) = await _adb.ConnectAsync(ip, connectPort);
        SetBusy(false, connectOk ? "Đã ghép nối và kết nối thành công." : "Đã ghép nối nhưng kết nối thất bại: " + connectMessage);
        if (!connectOk) MessageBox.Show(this, connectMessage, "Kết nối thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        await RefreshDevicesAsync();
    }

    /// <summary>Plain "adb connect ip:port", no mDNS and no dialog -- the shared tail end of both ConnectWifiAsync
    /// (code field left blank, already paired before) and ConnectByIpAsync below.</summary>
    private async Task ConnectAsync(string ip, string port)
    {
        if (_adb == null) return;
        SetBusy(true, $"Đang kết nối {ip}:{port}...");
        (bool ok, string message) = await _adb.ConnectAsync(ip, port);
        SetBusy(false, ok ? "Đã kết nối." : "Không kết nối được: " + message);
        if (!ok) MessageBox.Show(this, message, "Kết nối thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
        await RefreshDevicesAsync();
    }

    /// <summary>Prompts for IP:Port with the Code field hidden (pairing, if any, already happened by the time
    /// this runs) -- only reached now from PairAndConnectAsync's own mDNS-lookup-failed fallback, since the
    /// toolbar's standalone "Kết nối theo IP" button was merged into "Kết nối qua Wi-Fi..." (2026-10-28).</summary>
    private async Task ConnectByIpAsync(string? prefillIp = null)
    {
        if (_adb == null) return;
        using var dialog = new WifiConnectDialog(prefillIp, showCode: false);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        await ConnectAsync(dialog.Ip, dialog.Port);
    }

    /// <summary>Retries a few times (the phone's connect-port mDNS advertisement can take a moment to show up
    /// right after pairing) looking for an "_adb-tls-connect._tcp" entry from the SAME ip -- that's the stable
    /// correlation: the pairing and connect services run on the same phone/ip, just different ports, and the
    /// pairing one disappears once its screen closes.</summary>
    private async Task<string?> FindConnectPortAsync(string ip)
    {
        if (_adb == null) return null;
        for (int attempt = 0; attempt < 5; attempt++)
        {
            List<MdnsServiceItem> services;
            try { services = await _adb.ScanMdnsAsync(); }
            catch { return null; }
            MdnsServiceItem? match = services.FirstOrDefault(s => !s.IsPairing && s.Ip == ip);
            if (match != null) return match.Port;
            await Task.Delay(800);
        }
        return null;
    }

    /// <summary>"Dò mạng Wi-Fi...": picks from devices currently broadcasting wireless debugging on the LAN
    /// (<see cref="AdbService.ScanMdnsAsync"/>) instead of the user having to read IP/port off the phone by
    /// hand. A pairing-type result still needs the 6-digit code, so it just pre-fills <see cref="WifiConnectDialog"/>
    /// with its code field required; a connect-type result (already paired) can connect straight away.</summary>
    private async Task ScanWifiAsync()
    {
        if (_adb == null) return;
        using var scan = new ScanDevicesDialog(_adb);
        if (scan.ShowDialog(this) != DialogResult.OK || scan.Selected is not MdnsServiceItem selected) return;

        if (selected.IsPairing)
        {
            using var dialog = new WifiConnectDialog(selected.Ip, selected.Port, requireCode: true);
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            await PairAndConnectAsync(dialog.Ip, dialog.Port, dialog.Code);
            return;
        }

        await ConnectAsync(selected.Ip, selected.Port);
    }

    private void UpdateInstallButtonVisibility()
    {
        bool fileSelected = _fileList.SelectedItem != null;
        bool deviceSelected = _deviceGrid.CurrentRow != null && _deviceGrid.CurrentRow.Cells[2].Value as string == "Đã kết nối";
        _installButton.Visible = fileSelected && deviceSelected;
        _installButton.Enabled = _installButton.Visible && !_operationRunning;
    }

    private async Task InstallAsync()
    {
        if (_adb == null || _fileList.SelectedItem is not PackageFile file || _deviceGrid.CurrentRow == null) return;
        if (_deviceGrid.CurrentRow.Cells[1].Value is not string serial) return;

        SetBusy(true, "Đang chuẩn bị cài đặt...");
        // Raw adb/bundletool output lines, appended with whatever the last known step percent was -- the step
        // milestones below (SetProgress) already give a stage label, this just surfaces the tool's own detail
        // (e.g. "Success", a device error) between those milestones instead of the status line going quiet.
        void OnLine(string line) => BeginInvoke(() => _status.Text = $"{line} ({_progressPercent}%)");
        try
        {
            if (file.Kind == PackageKind.Apk)
            {
                SetProgress(15, $"Đang cài đặt {file.FileName} lên {serial} (adb install)");
                Services.ProcessResult result = await _adb.InstallApkAsync(serial, file.FullPath, OnLine);
                if (result.ExitCode != 0 || !result.Output.Contains("Success", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(result.Output);
                SetProgress(100, "Đã cài đặt xong");
            }
            else
            {
                if (!await BundletoolService.IsJavaAvailableAsync())
                    throw new InvalidOperationException("Cần cài Java (JRE/JDK) để cài file .aab qua bundletool -- không tìm thấy lệnh \"java\".");
                SetProgress(0, "Đang chuẩn bị bundletool");
                await new BundletoolService(_adb.AdbPath).InstallAabAsync(file.FullPath, serial, OnLine,
                    (percent, stage) => BeginInvoke(() => SetProgress(percent, stage)));
            }
            SetBusy(false, $"Đã cài {file.FileName} lên {serial}.");
        }
        catch (Exception ex)
        {
            SetBusy(false, "Cài đặt thất bại.");
            string message = ex.Message.Contains("INSTALL_FAILED_UPDATE_INCOMPATIBLE", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("signatures do not match", StringComparison.OrdinalIgnoreCase)
                ? "Ứng dụng đã cài trên máy được ký bằng key khác với file này (ví dụ: bản tải từ Play Store, hoặc build release/debug khác nhau). " +
                  "Hãy gỡ cài đặt ứng dụng cũ trên thiết bị rồi cài lại.\n\n" + ex.Message
                : ex.Message;
            MessageBox.Show(this, message, "Cài đặt thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SetBusy(bool busy, string statusText)
    {
        _operationRunning = busy;
        _progress.Visible = busy;
        _progressPercent = 0;
        _progress.Value = 0;
        _status.Text = statusText;
        _addButton.Enabled = _removeButton.Enabled = _refreshButton.Enabled = _pairWifiButton.Enabled = _scanWifiButton.Enabled = !busy;
        _fileList.Enabled = _deviceGrid.Enabled = !busy;
        UpdateInstallButtonVisibility();
    }

    private void SetProgress(int percent, string stageText)
    {
        _progressPercent = Math.Clamp(percent, 0, 100);
        _progress.Value = _progressPercent;
        _status.Text = $"{stageText} ({_progressPercent}%)";
    }
}
