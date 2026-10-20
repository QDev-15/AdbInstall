namespace AdbInstaller.Forms;

partial class ConnectDeviceDialog
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        this._hintLabel = new System.Windows.Forms.Label();
        this._ipLabel = new System.Windows.Forms.Label();
        this._ip = new System.Windows.Forms.TextBox();
        this._portLabel = new System.Windows.Forms.Label();
        this._port = new System.Windows.Forms.TextBox();
        this._okButton = new System.Windows.Forms.Button();
        this._cancelButton = new System.Windows.Forms.Button();
        this.SuspendLayout();
        //
        // _hintLabel
        //
        this._hintLabel.AutoSize = false;
        this._hintLabel.Location = new System.Drawing.Point(20, 15);
        this._hintLabel.Name = "_hintLabel";
        this._hintLabel.Size = new System.Drawing.Size(300, 60);
        this._hintLabel.Text = "Mở Cài đặt > Tuỳ chọn nhà phát triển > Gỡ lỗi không dây, lấy đúng IP và Cổng hiện ở màn hình chính (khác cổng dùng lúc ghép nối bằng mã).";
        //
        // _ipLabel
        //
        this._ipLabel.AutoSize = true;
        this._ipLabel.Location = new System.Drawing.Point(20, 85);
        this._ipLabel.Name = "_ipLabel";
        this._ipLabel.Text = "Địa chỉ IP:";
        //
        // _ip
        //
        this._ip.Location = new System.Drawing.Point(140, 82);
        this._ip.Name = "_ip";
        this._ip.PlaceholderText = "192.168.1.50";
        this._ip.Size = new System.Drawing.Size(180, 23);
        //
        // _portLabel
        //
        this._portLabel.AutoSize = true;
        this._portLabel.Location = new System.Drawing.Point(20, 115);
        this._portLabel.Name = "_portLabel";
        this._portLabel.Text = "Cổng kết nối:";
        //
        // _port
        //
        this._port.Location = new System.Drawing.Point(140, 112);
        this._port.Name = "_port";
        this._port.PlaceholderText = "40000";
        this._port.Size = new System.Drawing.Size(180, 23);
        //
        // _okButton
        //
        this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        this._okButton.Location = new System.Drawing.Point(140, 145);
        this._okButton.Name = "_okButton";
        this._okButton.Size = new System.Drawing.Size(85, 29);
        this._okButton.Text = "Kết nối";
        this._okButton.Click += new System.EventHandler(this.OkButton_Click);
        //
        // _cancelButton
        //
        this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this._cancelButton.Location = new System.Drawing.Point(235, 145);
        this._cancelButton.Name = "_cancelButton";
        this._cancelButton.Size = new System.Drawing.Size(85, 29);
        this._cancelButton.Text = "Huỷ";
        //
        // ConnectDeviceDialog
        //
        this.AcceptButton = this._okButton;
        this.CancelButton = this._cancelButton;
        this.ClientSize = new System.Drawing.Size(340, 190);
        this.Controls.Add(this._hintLabel);
        this.Controls.Add(this._ipLabel);
        this.Controls.Add(this._ip);
        this.Controls.Add(this._portLabel);
        this.Controls.Add(this._port);
        this.Controls.Add(this._okButton);
        this.Controls.Add(this._cancelButton);
        this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "ConnectDeviceDialog";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Kết nối theo IP:Cổng";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label _hintLabel;
    private System.Windows.Forms.Label _ipLabel;
    private System.Windows.Forms.TextBox _ip;
    private System.Windows.Forms.Label _portLabel;
    private System.Windows.Forms.TextBox _port;
    private System.Windows.Forms.Button _okButton;
    private System.Windows.Forms.Button _cancelButton;
}
