namespace AdbInstaller.Forms;

/// <summary>Popup for Android 11+ wireless debugging pairing: IP, port and the 6-digit code all come from the
/// device's own "Pair device with pairing code" screen (Settings &gt; Developer options &gt; Wireless debugging).</summary>
public sealed class PairDeviceDialog : Form
{
	private readonly TextBox _ip = new() { PlaceholderText = "192.168.1.50" };
	private readonly TextBox _port = new() { PlaceholderText = "37251" };
	private readonly TextBox _code = new() { PlaceholderText = "123456" };

	public string Ip => _ip.Text.Trim();
	public string Port => _port.Text.Trim();
	public string Code => _code.Text.Trim();

	public PairDeviceDialog()
	{
		Text = "Kết nối qua Wi-Fi (ghép nối)";
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterParent;
		MaximizeBox = false;
		MinimizeBox = false;
		ClientSize = new Size(340, 230);
		Font = new Font("Segoe UI", 9.5f);

		var hint = new Label
		{
			Text = "Mở Cài đặt > Tuỳ chọn nhà phát triển > Gỡ lỗi không dây > \"Ghép nối thiết bị bằng mã ghép nối\", rồi nhập đúng IP, cổng và mã hiện trên máy.",
			AutoSize = false,
			Size = new Size(300, 60),
			Location = new Point(20, 15),
		};

		Label MakeLabel(string text, int y) => new() { Text = text, Location = new Point(20, y), AutoSize = true };
		_ip.Location = new Point(140, 82); _ip.Width = 180;
		_port.Location = new Point(140, 112); _port.Width = 180;
		_code.Location = new Point(140, 142); _code.Width = 180;

		var ok = new Button { Text = "Ghép nối", DialogResult = DialogResult.OK, Location = new Point(140, 185), Width = 85 };
		var cancel = new Button { Text = "Huỷ", DialogResult = DialogResult.Cancel, Location = new Point(235, 185), Width = 85 };
		ok.Click += (_, _) =>
		{
			if (string.IsNullOrWhiteSpace(Ip) || string.IsNullOrWhiteSpace(Port) || string.IsNullOrWhiteSpace(Code))
			{
				MessageBox.Show(this, "Nhập đủ IP, cổng và mã ghép nối.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				DialogResult = DialogResult.None; // keep the dialog open
			}
		};

		Controls.AddRange([
			hint,
			MakeLabel("Địa chỉ IP:", 85), _ip,
			MakeLabel("Cổng ghép nối:", 115), _port,
			MakeLabel("Mã ghép nối:", 145), _code,
			ok, cancel,
		]);
		AcceptButton = ok;
		CancelButton = cancel;
	}
}
