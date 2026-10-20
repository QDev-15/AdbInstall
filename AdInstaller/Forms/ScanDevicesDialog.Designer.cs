namespace AdbInstaller.Forms;

partial class ScanDevicesDialog
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
        this._bodyPanel = new System.Windows.Forms.Panel();
        this._list = new System.Windows.Forms.ListBox();
        this._fixRowPanel = new System.Windows.Forms.FlowLayoutPanel();
        this._fixFirewall = new System.Windows.Forms.Button();
        this._buttonsPanel = new System.Windows.Forms.FlowLayoutPanel();
        this._cancel = new System.Windows.Forms.Button();
        this._ok = new System.Windows.Forms.Button();
        this._rescan = new System.Windows.Forms.Button();
        this._status = new System.Windows.Forms.Label();
        this._bodyPanel.SuspendLayout();
        this._fixRowPanel.SuspendLayout();
        this._buttonsPanel.SuspendLayout();
        this.SuspendLayout();
        //
        // _hintLabel
        //
        this._hintLabel.AutoSize = false;
        this._hintLabel.Dock = System.Windows.Forms.DockStyle.Top;
        this._hintLabel.Height = 56;
        this._hintLabel.Name = "_hintLabel";
        this._hintLabel.Padding = new System.Windows.Forms.Padding(8, 8, 8, 0);
        this._hintLabel.Text = "Chỉ thấy được máy đang mở sẵn màn hình \"Gỡ lỗi không dây\" (hoặc màn hình ghép nối bằng mã) trên cùng mạng Wi-Fi. Máy dùng kiểu \"adb tcpip\" cũ, hoặc mạng chặn mDNS, sẽ không hiện ở đây.";
        //
        // _bodyPanel
        //
        this._bodyPanel.Controls.Add(this._list);
        this._bodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this._bodyPanel.Name = "_bodyPanel";
        this._bodyPanel.Padding = new System.Windows.Forms.Padding(8, 0, 8, 8);
        //
        // _list
        //
        this._list.Dock = System.Windows.Forms.DockStyle.Fill;
        this._list.IntegralHeight = false;
        this._list.Name = "_list";
        this._list.DoubleClick += new System.EventHandler(this.List_DoubleClick);
        this._list.SelectedIndexChanged += new System.EventHandler(this.List_SelectedIndexChanged);
        //
        // _fixRowPanel
        //
        this._fixRowPanel.AutoSize = false;
        this._fixRowPanel.Controls.Add(this._fixFirewall);
        this._fixRowPanel.Dock = System.Windows.Forms.DockStyle.Top;
        this._fixRowPanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._fixRowPanel.Height = 36;
        this._fixRowPanel.Name = "_fixRowPanel";
        this._fixRowPanel.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
        //
        // _fixFirewall
        //
        this._fixFirewall.AutoSize = true;
        this._fixFirewall.Name = "_fixFirewall";
        this._fixFirewall.Text = "Sửa tường lửa (không dò được?)";
        this._fixFirewall.Click += new System.EventHandler(this.FixFirewall_Click);
        //
        // _buttonsPanel
        //
        this._buttonsPanel.Controls.Add(this._cancel);
        this._buttonsPanel.Controls.Add(this._ok);
        this._buttonsPanel.Controls.Add(this._rescan);
        this._buttonsPanel.Dock = System.Windows.Forms.DockStyle.Top;
        this._buttonsPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this._buttonsPanel.Height = 40;
        this._buttonsPanel.Name = "_buttonsPanel";
        this._buttonsPanel.Padding = new System.Windows.Forms.Padding(8, 8, 8, 0);
        //
        // _cancel
        //
        this._cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this._cancel.Name = "_cancel";
        this._cancel.Text = "Huỷ";
        //
        // _ok
        //
        this._ok.DialogResult = System.Windows.Forms.DialogResult.OK;
        this._ok.Enabled = false;
        this._ok.Name = "_ok";
        this._ok.Text = "Chọn";
        //
        // _rescan
        //
        this._rescan.Name = "_rescan";
        this._rescan.Text = "Dò lại";
        this._rescan.Click += new System.EventHandler(this.Rescan_Click);
        //
        // _status
        //
        this._status.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._status.Height = 22;
        this._status.Name = "_status";
        this._status.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
        this._status.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // ScanDevicesDialog
        //
        this.AcceptButton = this._ok;
        this.CancelButton = this._cancel;
        this.ClientSize = new System.Drawing.Size(440, 370);
        this.Controls.Add(this._bodyPanel);
        this.Controls.Add(this._fixRowPanel);
        this.Controls.Add(this._buttonsPanel);
        this.Controls.Add(this._hintLabel);
        this.Controls.Add(this._status);
        this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "ScanDevicesDialog";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Dò máy mở gỡ lỗi qua Wi-Fi";
        this.Load += new System.EventHandler(this.ScanDevicesDialog_Load);
        this._bodyPanel.ResumeLayout(false);
        this._fixRowPanel.ResumeLayout(false);
        this._fixRowPanel.PerformLayout();
        this._buttonsPanel.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.Label _hintLabel;
    private System.Windows.Forms.Panel _bodyPanel;
    private System.Windows.Forms.ListBox _list;
    private System.Windows.Forms.FlowLayoutPanel _fixRowPanel;
    private System.Windows.Forms.Button _fixFirewall;
    private System.Windows.Forms.FlowLayoutPanel _buttonsPanel;
    private System.Windows.Forms.Button _cancel;
    private System.Windows.Forms.Button _ok;
    private System.Windows.Forms.Button _rescan;
    private System.Windows.Forms.Label _status;
}
