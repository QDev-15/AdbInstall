namespace AdbInstaller.Forms;

/// <summary>Popup for Android 11+ wireless debugging pairing: IP, port and the 6-digit code all come from the
/// device's own "Pair device with pairing code" screen (Settings &gt; Developer options &gt; Wireless debugging).</summary>
public partial class PairDeviceDialog : Form
{
    public string Ip => _ip.Text.Trim();
    public string Port => _port.Text.Trim();
    public string Code => _code.Text.Trim();

    public PairDeviceDialog(string? ip = null, string? port = null)
    {
        InitializeComponent();
        if (ip != null) _ip.Text = ip;
        if (port != null) _port.Text = port;
    }

    private void OkButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Ip) || string.IsNullOrWhiteSpace(Port) || string.IsNullOrWhiteSpace(Code))
        {
            MessageBox.Show(this, "Nhập đủ IP, cổng và mã ghép nối.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None; // keep the dialog open
        }
    }
}
