namespace AdbInstaller.Forms;

/// <summary>Merged "Kết nối qua Wi-Fi" + "Kết nối theo IP" (owner's report, 2026-10-28: hai button này khá giống
/// nhau). The same IP/Port pair means two different things depending on whether <see cref="Code"/> is filled in:
/// empty -- the device was already paired before, this is just "adb connect ip:port" with today's connect port;
/// filled -- a brand-new pairing, "adb pair ip:port code", and MainForm.PairAndConnectAsync then runs a plain
/// connect afterward on its own so the device actually lands in "adb devices", not just marked as a trusted
/// pair.</summary>
public partial class WifiConnectDialog : Form
{
    public string Ip => _ip.Text.Trim();
    public string Port => _port.Text.Trim();
    public string Code => _code.Text.Trim();

    private readonly bool _requireCode;

    /// <param name="showCode">False for the one internal case (MainForm.ConnectByIpAsync, PairAndConnectAsync's
    /// fallback when mDNS can't auto-find the connect port right after a fresh pairing) where a code would never
    /// apply -- pairing already happened, that call only ever wants a plain connect.</param>
    /// <param name="requireCode">True when the caller already knows this device needs a fresh pairing (the mDNS
    /// scan's "pairing" service kind) -- an empty code there would just fail confusingly rather than silently
    /// falling back to a plain connect.</param>
    public WifiConnectDialog(string? ip = null, string? port = null, bool showCode = true, bool requireCode = false)
    {
        InitializeComponent();
        _requireCode = requireCode;
        if (ip != null) _ip.Text = ip;
        if (port != null) _port.Text = port;

        if (!showCode)
        {
            _codeLabel.Visible = _code.Visible = false;
            _okButton.Top = _cancelButton.Top = _codeLabel.Top;
            ClientSize = new Size(ClientSize.Width, _okButton.Bottom + 16);
            _hintLabel.Text = "Mở \"Gỡ lỗi không dây\" trên điện thoại, nhập đúng IP và Cổng hiện ở màn hình chính.";
            Text = "Kết nối theo IP";
            _okButton.Text = "Kết nối";
        }
        else if (requireCode)
        {
            _hintLabel.Text = "Thiết bị này đang ở chế độ ghép nối. Mở \"Ghép nối thiết bị bằng mã ghép nối\" trên điện thoại, nhập đúng IP, Cổng và Mã hiện ở đó.";
            _okButton.Text = "Ghép nối";
        }
        else
        {
            _code.TextChanged += (_, _) => _okButton.Text = Code.Length > 0 ? "Ghép nối && Kết nối" : "Kết nối";
        }
    }

    private void OkButton_Click(object? sender, EventArgs e)
    {
        bool missing = string.IsNullOrWhiteSpace(Ip) || string.IsNullOrWhiteSpace(Port) || (_requireCode && string.IsNullOrWhiteSpace(Code));
        if (missing)
        {
            MessageBox.Show(this, _requireCode ? "Nhập đủ IP, cổng và mã ghép nối." : "Nhập đủ IP và cổng.",
                "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None; // keep the dialog open
        }
    }
}
