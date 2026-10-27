namespace AdbInstaller.Forms;

partial class DeviceMonitorForm
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
        this.components = new System.ComponentModel.Container();
        this._topPanel = new System.Windows.Forms.FlowLayoutPanel();
        this._deviceLabel = new System.Windows.Forms.Label();
        this._deviceCombo = new System.Windows.Forms.ComboBox();
        this._refreshDevicesButton = new System.Windows.Forms.Button();
        this._toggleButton = new System.Windows.Forms.Button();
        this._grid = new System.Windows.Forms.DataGridView();
        this._bottomPanel = new System.Windows.Forms.FlowLayoutPanel();
        this._memLabel = new System.Windows.Forms.Label();
        this._batteryLabel = new System.Windows.Forms.Label();
        this._refreshStatsButton = new System.Windows.Forms.Button();
        this._status = new System.Windows.Forms.Label();
        this._pollTimer = new System.Windows.Forms.Timer(this.components);
        ((System.ComponentModel.ISupportInitialize)(this._grid)).BeginInit();
        this._topPanel.SuspendLayout();
        this._bottomPanel.SuspendLayout();
        this.SuspendLayout();
        //
        // _topPanel
        //
        this._topPanel.Controls.Add(this._deviceLabel);
        this._topPanel.Controls.Add(this._deviceCombo);
        this._topPanel.Controls.Add(this._refreshDevicesButton);
        this._topPanel.Controls.Add(this._toggleButton);
        this._topPanel.Dock = System.Windows.Forms.DockStyle.Top;
        this._topPanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._topPanel.Height = 40;
        this._topPanel.Name = "_topPanel";
        this._topPanel.Padding = new System.Windows.Forms.Padding(8, 8, 8, 0);
        this._topPanel.WrapContents = false;
        //
        // _deviceLabel
        //
        this._deviceLabel.AutoSize = true;
        this._deviceLabel.Margin = new System.Windows.Forms.Padding(0, 6, 4, 0);
        this._deviceLabel.Name = "_deviceLabel";
        this._deviceLabel.Text = "Thiết bị:";
        //
        // _deviceCombo
        //
        this._deviceCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this._deviceCombo.Name = "_deviceCombo";
        this._deviceCombo.Width = 260;
        this._deviceCombo.SelectedIndexChanged += new System.EventHandler(this.DeviceCombo_SelectedIndexChanged);
        //
        // _refreshDevicesButton
        //
        this._refreshDevicesButton.Name = "_refreshDevicesButton";
        this._refreshDevicesButton.Text = "Làm mới danh sách";
        this._refreshDevicesButton.Click += new System.EventHandler(this.RefreshDevicesButton_Click);
        //
        // _toggleButton
        //
        this._toggleButton.Name = "_toggleButton";
        this._toggleButton.Text = "Bắt đầu theo dõi real-time";
        this._toggleButton.Width = 180;
        this._toggleButton.Click += new System.EventHandler(this.ToggleButton_Click);
        //
        // _grid
        //
        this._grid.AllowUserToAddRows = false;
        this._grid.AllowUserToDeleteRows = false;
        this._grid.AllowUserToResizeRows = false;
        this._grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._grid.BackgroundColor = System.Drawing.SystemColors.Window;
        this._grid.Dock = System.Windows.Forms.DockStyle.Fill;
        this._grid.MultiSelect = false;
        this._grid.Name = "_grid";
        this._grid.ReadOnly = true;
        this._grid.RowHeadersVisible = false;
        this._grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this._grid.Columns.Add("Pid", "PID");
        this._grid.Columns.Add("Name", "Name");
        this._grid.Columns.Add("Cpu", "%CPU");
        this._grid.Columns.Add("Mem", "RAM (MB)");
        // Programmatic, not the default Automatic: Automatic would sort the displayed (string) cell text --
        // wrong order for %CPU/RAM once double digits show up ("20" sorting before "9"). Grid_ColumnHeaderMouseClick
        // in the .cs file does a proper numeric/string sort on the underlying ProcessInfo list instead.
        foreach (System.Windows.Forms.DataGridViewColumn column in this._grid.Columns)
            column.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
        this._grid.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.Grid_ColumnHeaderMouseClick);
        //
        // _bottomPanel
        //
        this._bottomPanel.Controls.Add(this._memLabel);
        this._bottomPanel.Controls.Add(this._batteryLabel);
        this._bottomPanel.Controls.Add(this._refreshStatsButton);
        this._bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._bottomPanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._bottomPanel.Height = 40;
        this._bottomPanel.Name = "_bottomPanel";
        this._bottomPanel.Padding = new System.Windows.Forms.Padding(8, 8, 8, 0);
        this._bottomPanel.WrapContents = false;
        //
        // _memLabel
        //
        this._memLabel.AutoSize = true;
        this._memLabel.Margin = new System.Windows.Forms.Padding(0, 6, 24, 0);
        this._memLabel.Name = "_memLabel";
        this._memLabel.Text = "RAM: --";
        //
        // _batteryLabel
        //
        this._batteryLabel.AutoSize = true;
        this._batteryLabel.Margin = new System.Windows.Forms.Padding(0, 6, 24, 0);
        this._batteryLabel.Name = "_batteryLabel";
        this._batteryLabel.Text = "Pin: --";
        //
        // _refreshStatsButton
        //
        this._refreshStatsButton.Name = "_refreshStatsButton";
        this._refreshStatsButton.Text = "Làm mới RAM/Pin";
        this._refreshStatsButton.Click += new System.EventHandler(this.RefreshStatsButton_Click);
        //
        // _status
        //
        this._status.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._status.Height = 22;
        this._status.Name = "_status";
        this._status.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
        this._status.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // _pollTimer
        //
        this._pollTimer.Interval = 1500;
        this._pollTimer.Tick += new System.EventHandler(this.PollTimer_Tick);
        //
        // DeviceMonitorForm
        //
        this.ClientSize = new System.Drawing.Size(640, 480);
        this.Controls.Add(this._grid);
        this.Controls.Add(this._bottomPanel);
        this.Controls.Add(this._topPanel);
        this.Controls.Add(this._status);
        this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
        this.MinimumSize = new System.Drawing.Size(520, 360);
        this.Name = "DeviceMonitorForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Giám sát thiết bị";
        this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DeviceMonitorForm_FormClosed);
        this.Load += new System.EventHandler(this.DeviceMonitorForm_Load);
        ((System.ComponentModel.ISupportInitialize)(this._grid)).EndInit();
        this._topPanel.ResumeLayout(false);
        this._topPanel.PerformLayout();
        this._bottomPanel.ResumeLayout(false);
        this._bottomPanel.PerformLayout();
        this.ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.FlowLayoutPanel _topPanel;
    private System.Windows.Forms.Label _deviceLabel;
    private System.Windows.Forms.ComboBox _deviceCombo;
    private System.Windows.Forms.Button _refreshDevicesButton;
    private System.Windows.Forms.Button _toggleButton;
    private System.Windows.Forms.DataGridView _grid;
    private System.Windows.Forms.FlowLayoutPanel _bottomPanel;
    private System.Windows.Forms.Label _memLabel;
    private System.Windows.Forms.Label _batteryLabel;
    private System.Windows.Forms.Button _refreshStatsButton;
    private System.Windows.Forms.Label _status;
    private System.Windows.Forms.Timer _pollTimer;
}
