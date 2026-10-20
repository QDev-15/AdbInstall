using AdbInstaller.Models;
using AdbInstaller.Services;

namespace AdbInstaller.Forms;

public sealed class MainForm : Form
{
	private readonly ListBox _fileList = new() { Dock = DockStyle.Fill, AllowDrop = true };
	private readonly DataGridView _deviceGrid = new()
	{
		Dock = DockStyle.Fill,
		AllowUserToAddRows = false,
		AllowUserToDeleteRows = false,
		AllowUserToResizeRows = false,
		ReadOnly = true,
		RowHeadersVisible = false,
		SelectionMode = DataGridViewSelectionMode.FullRowSelect,
		MultiSelect = false,
		AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
		BackgroundColor = SystemColors.Window,
	};
	private readonly Button _addButton = new() { Text = "Thêm file..." };
	private readonly Button _removeButton = new() { Text = "Xoá" };
	private readonly Button _refreshButton = new() { Text = "Làm mới" };
	private readonly Button _pairWifiButton = new() { Text = "Kết nối qua Wi-Fi..." };
	private readonly Button _installButton = new() { Text = "Cài đặt", Visible = false, Enabled = false, Height = 36 };
	private readonly ProgressBar _progress = new() { Dock = DockStyle.Bottom, Style = ProgressBarStyle.Continuous, Minimum = 0, Maximum = 100, Visible = false, Height = 10 };
	private readonly Label _status = new() { Dock = DockStyle.Bottom, Height = 22, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(4, 0, 0, 0) };
	private readonly System.Windows.Forms.Timer _devicePoll = new() { Interval = 2000 };

	private readonly List<PackageFile> _files = [];
	private AdbService? _adb;
	private bool _operationRunning;
	private int _progressPercent;
	private const int ActionColumnIndex = 3;

	public MainForm()
	{
		Text = "APK/AAB Installer";
		StartPosition = FormStartPosition.CenterScreen;
		ClientSize = new Size(820, 480);
		MinimumSize = new Size(640, 360);
		Font = new Font("Segoe UI", 9.5f);
		// ApplicationIcon in the csproj only sets the compiled .exe's own file icon (what Explorer shows); the
		// running window's title bar / taskbar button is a separate Form.Icon that stays the generic WinForms
		// default unless set here too -- reuse the already-embedded exe icon rather than shipping app.ico twice.
		try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { /* best-effort */ }

		BuildLayout();
		WireEvents();

		Load += async (_, _) => await InitializeAdbAsync();
		_devicePoll.Tick += async (_, _) => await RefreshDevicesAsync();
	}

	private void BuildLayout()
	{
		_deviceGrid.Columns.Add("Name", "Thiết bị");
		_deviceGrid.Columns.Add("Serial", "Serial / Địa chỉ");
		_deviceGrid.Columns.Add("Status", "Trạng thái");
		// UseColumnTextForButtonValue=false: each row's own cell Value decides its button text (blank for an
		// already-connected device, "Kết nối" for one that needs it) -- true would force every row's button to
		// show the same column-level Text regardless of that row's own state.
		var actionColumn = new DataGridViewButtonColumn { Name = "Action", HeaderText = "", UseColumnTextForButtonValue = false, FillWeight = 60 };
		_deviceGrid.Columns.Add(actionColumn);

		// Left: file list + Add/Remove. Right: device grid + Refresh/Connect by Wi-Fi.
		var left = new Panel { Dock = DockStyle.Fill };
		var leftButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.LeftToRight, Height = 36, Padding = new Padding(0, 4, 0, 0) };
		leftButtons.Controls.AddRange([_addButton, _removeButton]);
		left.Controls.Add(_fileList);
		left.Controls.Add(leftButtons);
		var leftGroup = new GroupBox { Text = "File (kéo-thả .apk / .aab vào đây, hoặc bấm Thêm file)", Dock = DockStyle.Fill, Padding = new Padding(8) };
		leftGroup.Controls.Add(left);

		var right = new Panel { Dock = DockStyle.Fill };
		var rightButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.LeftToRight, Height = 36, Padding = new Padding(0, 4, 0, 0) };
		rightButtons.Controls.AddRange([_refreshButton, _pairWifiButton]);
		right.Controls.Add(_deviceGrid);
		right.Controls.Add(rightButtons);
		var rightGroup = new GroupBox { Text = "Thiết bị (bấm chọn một dòng \"Đã kết nối\" để cài lên đó)", Dock = DockStyle.Fill, Padding = new Padding(8) };
		rightGroup.Controls.Add(right);

		// SplitterDistance/MinSize are NOT set in the initializer: a freshly-constructed SplitContainer has no
		// real Width yet (it is only realized once added to a parent that already has a size), so validating
		// SplitterDistance=380 against that default tiny Width throws "SplitterDistance must be between
		// Panel1MinSize and Width - Panel2MinSize". Adding it to this Form (which already has ClientSize set in
		// the constructor) first gives it its real Width; only then is it safe to set these.
		var split = new SplitContainer { Dock = DockStyle.Fill };
		split.Panel1.Controls.Add(leftGroup);
		split.Panel2.Controls.Add(rightGroup);

		var bottom = new Panel { Dock = DockStyle.Bottom, Height = 46, Padding = new Padding(8, 6, 8, 6) };
		_installButton.Dock = DockStyle.Right;
		_installButton.Width = 140;
		bottom.Controls.Add(_installButton);

		Controls.Add(split);
		split.Panel1MinSize = 260;
		split.Panel2MinSize = 260;
		split.SplitterDistance = 380;
		Controls.Add(bottom);
		Controls.Add(_status);
		Controls.Add(_progress);
	}

	private void WireEvents()
	{
		// DisplayMember left blank on purpose: ListBox falls back to each item's own ToString() (already
		// overridden on PackageFile) when it is empty. Setting it to a method name like "ToString" would make
		// WinForms look for a PROPERTY of that name via reflection and fail to find one.
		_fileList.SelectedIndexChanged += (_, _) => UpdateInstallButtonVisibility();
		_fileList.AllowDrop = true;
		_fileList.DragEnter += (_, e) => e.Effect = e.Data?.GetDataPresent(DataFormats.FileDrop) == true ? DragDropEffects.Copy : DragDropEffects.None;
		_fileList.DragDrop += (_, e) =>
		{
			if (e.Data?.GetData(DataFormats.FileDrop) is not string[] paths) return;
			AddFiles(paths);
		};

		_addButton.Click += (_, _) =>
		{
			using var dialog = new OpenFileDialog { Filter = "Gói ứng dụng Android (*.apk;*.aab)|*.apk;*.aab", Multiselect = true, Title = "Chọn file APK/AAB" };
			if (dialog.ShowDialog(this) == DialogResult.OK) AddFiles(dialog.FileNames);
		};
		_removeButton.Click += (_, _) =>
		{
			if (_fileList.SelectedIndex is int i and >= 0)
			{
				_files.RemoveAt(i);
				_fileList.Items.RemoveAt(i);
			}
		};

		_deviceGrid.SelectionChanged += (_, _) => UpdateInstallButtonVisibility();
		_deviceGrid.CellContentClick += async (_, e) =>
		{
			if (e.RowIndex < 0 || e.ColumnIndex != ActionColumnIndex || _adb == null) return;
			DataGridViewRow row = _deviceGrid.Rows[e.RowIndex];
			if (row.Cells["Status"].Value as string == "Đã kết nối") return; // empty button on an already-connected row: no-op
			if (row.Cells["Serial"].Value is not string serial) return;
			SetBusy(true, $"Đang kết nối lại {serial}...");
			(bool ok, string message) = await _adb.ReconnectAsync(serial);
			SetBusy(false, ok ? "Đã kết nối." : "Không kết nối được: " + message);
			await RefreshDevicesAsync();
		};

		_refreshButton.Click += async (_, _) => await RefreshDevicesAsync();
		_pairWifiButton.Click += async (_, _) => await PairByWifiAsync();
		_installButton.Click += async (_, _) => await InstallAsync();
	}

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

	private async Task PairByWifiAsync()
	{
		if (_adb == null) return;
		using var dialog = new PairDeviceDialog();
		if (dialog.ShowDialog(this) != DialogResult.OK) return;

		SetBusy(true, "Đang ghép nối qua Wi-Fi...");
		(bool ok, string message) = await _adb.PairAsync(dialog.Ip, dialog.Port, dialog.Code);
		SetBusy(false, message);
		if (!ok)
		{
			MessageBox.Show(this, message, "Ghép nối thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		MessageBox.Show(this,
			"Ghép nối thành công. Mở lại màn hình \"Gỡ lỗi không dây\" trên điện thoại, lấy đúng IP:Cổng hiện ở đó (có thể khác cổng ghép nối vừa nhập), rồi bấm \"Kết nối\" ở dòng thiết bị tương ứng.",
			"Đã ghép nối", MessageBoxButtons.OK, MessageBoxIcon.Information);
		await RefreshDevicesAsync();
	}

	private void UpdateInstallButtonVisibility()
	{
		bool fileSelected = _fileList.SelectedItem != null;
		bool deviceSelected = _deviceGrid.CurrentRow != null && _deviceGrid.CurrentRow.Cells["Status"].Value as string == "Đã kết nối";
		_installButton.Visible = fileSelected && deviceSelected;
		_installButton.Enabled = _installButton.Visible && !_operationRunning;
	}

	private async Task InstallAsync()
	{
		if (_adb == null || _fileList.SelectedItem is not PackageFile file || _deviceGrid.CurrentRow == null) return;
		if (_deviceGrid.CurrentRow.Cells["Serial"].Value is not string serial) return;

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
		_addButton.Enabled = _removeButton.Enabled = _refreshButton.Enabled = _pairWifiButton.Enabled = !busy;
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
