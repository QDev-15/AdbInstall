namespace AdbInstaller.Forms;

/// <summary>Plain "adb connect ip:port" -- no pairing code. For a device already paired at some point (pairing
/// persists across the phone's reboots), only the connect PORT tends to change each time "Wireless debugging"
/// is toggled off/on; this is the manual fallback when mDNS auto-discovery (<see cref="ScanDevicesDialog"/>,
/// or the auto-lookup in MainForm right after pairing) can't find that port itself -- e.g. the network blocks
/// mDNS multicast and the user hasn't fixed the firewall yet, but can still read the IP:port straight off the
/// phone's own "Wireless debugging" screen.</summary>
public partial class ConnectDeviceDialog : Form
{
    public string Ip => _ip.Text.Trim();
    public string Port => _port.Text.Trim();

    public ConnectDeviceDialog(string? ip = null)
    {
        InitializeComponent();
        if (ip != null) _ip.Text = ip;
    }

    private void OkButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Ip) || string.IsNullOrWhiteSpace(Port))
        {
            MessageBox.Show(this, "Nhập đủ IP và cổng.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None; // keep the dialog open
        }
    }
}
