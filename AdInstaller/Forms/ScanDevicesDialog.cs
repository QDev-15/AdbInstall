using AdbInstaller.Models;
using AdbInstaller.Services;

namespace AdbInstaller.Forms;

/// <summary>Lists Android 11+ wireless-debugging devices currently broadcasting on the local Wi-Fi network via
/// mDNS ("adb mdns services"). Only shows a device that is itself advertising right now; see
/// <see cref="AdbService.ScanMdnsAsync"/> for exactly what that does and doesn't catch.</summary>
public partial class ScanDevicesDialog : Form
{
    private readonly AdbService _adb;

    public MdnsServiceItem? Selected => _list.SelectedItem as MdnsServiceItem;

    /// <summary>Design-time only (the Designer needs a parameterless constructor to host the form); the
    /// placeholder AdbService is never actually invoked since the Designer never fires Load.</summary>
    public ScanDevicesDialog() : this(new AdbService(string.Empty))
    {
    }

    public ScanDevicesDialog(AdbService adb)
    {
        _adb = adb;
        InitializeComponent();
    }

    private void List_DoubleClick(object? sender, EventArgs e)
    {
        if (_list.SelectedItem != null) { DialogResult = DialogResult.OK; Close(); }
    }

    private void List_SelectedIndexChanged(object? sender, EventArgs e) => _ok.Enabled = _list.SelectedItem != null;

    private async void Rescan_Click(object? sender, EventArgs e) => await ScanAsync();

    private async void ScanDevicesDialog_Load(object? sender, EventArgs e) => await ScanAsync();

    /// <summary>"Sửa tường lửa": opens inbound UDP 5353 (mDNS) in Windows Firewall -- the usual reason nothing
    /// shows up on a network Windows has classified as "Public". One UAC prompt via <see cref="FirewallService"/>;
    /// a user-visible, somewhat hard-to-reverse system change, so this confirms with the user before triggering
    /// it, same as the UAC prompt itself confirms with Windows.</summary>
    private async void FixFirewall_Click(object? sender, EventArgs e)
    {
        _fixFirewall.Enabled = false;
        try
        {
            if (await FirewallService.IsMdnsRuleEnabledAsync())
            {
                MessageBox.Show(this,
                    "Tường lửa đã cho phép mDNS (UDP 5353) rồi. Nếu vẫn không dò được, kiểm tra điện thoại đang mở màn hình \"Gỡ lỗi không dây\" và cùng một mạng Wi-Fi với máy tính này.",
                    "Đã mở sẵn", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirm = MessageBox.Show(this,
                "Thêm một quy tắc Windows Firewall cho phép nhận gói mDNS (UDP cổng 5353) -- cần để dò máy Android mở gỡ lỗi qua Wi-Fi trên mạng \"Public\". Windows sẽ hỏi quyền Quản trị viên. Tiếp tục?",
                "Mở cổng mDNS trong tường lửa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            _status.Text = "Đang xin quyền quản trị...";
            (bool ok, string message) = await FirewallService.EnableMdnsRuleAsync();
            _status.Text = message;
            if (!ok)
            {
                MessageBox.Show(this, message, "Không mở được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            await ScanAsync();
        }
        finally
        {
            _fixFirewall.Enabled = true;
        }
    }

    private async Task ScanAsync()
    {
        _rescan.Enabled = false;
        _list.Items.Clear();
        _ok.Enabled = false;
        _status.Text = "Đang dò mạng...";
        try
        {
            List<MdnsServiceItem> services = await _adb.ScanMdnsAsync();
            foreach (MdnsServiceItem service in services) _list.Items.Add(service);
            _status.Text = services.Count == 0 ? "Không thấy máy nào đang mở gỡ lỗi qua Wi-Fi." : $"Tìm thấy {services.Count} máy.";
        }
        catch (Exception ex)
        {
            _status.Text = "Dò mạng thất bại: " + ex.Message;
        }
        finally
        {
            _rescan.Enabled = true;
        }
    }
}
