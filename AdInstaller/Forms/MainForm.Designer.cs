namespace AdbInstaller.Forms;

partial class MainForm
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
        components = new System.ComponentModel.Container();
        _fileList = new ListBox();
        _deviceGrid = new DataGridView();
        _addButton = new Button();
        _removeButton = new Button();
        _refreshButton = new Button();
        _pairWifiButton = new Button();
        _scanWifiButton = new Button();
        _connectIpButton = new Button();
        _installButton = new Button();
        _progress = new ProgressBar();
        _status = new Label();
        _devicePoll = new System.Windows.Forms.Timer(components);
        _leftPanel = new Panel();
        _leftButtonsPanel = new FlowLayoutPanel();
        _leftGroup = new GroupBox();
        _rightPanel = new Panel();
        _rightButtonsPanel = new FlowLayoutPanel();
        _rightGroup = new GroupBox();
        _splitContainer = new SplitContainer();
        _bottomPanel = new Panel();
        dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
        dataGridViewButtonColumn1 = new DataGridViewButtonColumn();
        ((System.ComponentModel.ISupportInitialize)_deviceGrid).BeginInit();
        _leftPanel.SuspendLayout();
        _leftButtonsPanel.SuspendLayout();
        _leftGroup.SuspendLayout();
        _rightPanel.SuspendLayout();
        _rightButtonsPanel.SuspendLayout();
        _rightGroup.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_splitContainer).BeginInit();
        _splitContainer.Panel1.SuspendLayout();
        _splitContainer.Panel2.SuspendLayout();
        _splitContainer.SuspendLayout();
        _bottomPanel.SuspendLayout();
        SuspendLayout();
        // 
        // _fileList
        // 
        _fileList.AllowDrop = true;
        _fileList.Dock = DockStyle.Fill;
        _fileList.Location = new Point(0, 0);
        _fileList.Name = "_fileList";
        _fileList.Size = new Size(350, 333);
        _fileList.TabIndex = 0;
        _fileList.SelectedIndexChanged += FileList_SelectedIndexChanged;
        _fileList.DragDrop += FileList_DragDrop;
        _fileList.DragEnter += FileList_DragEnter;
        // 
        // _deviceGrid
        // 
        _deviceGrid.AllowUserToAddRows = false;
        _deviceGrid.AllowUserToDeleteRows = false;
        _deviceGrid.AllowUserToResizeRows = false;
        _deviceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _deviceGrid.BackgroundColor = SystemColors.Window;
        _deviceGrid.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3, dataGridViewButtonColumn1 });
        _deviceGrid.Dock = DockStyle.Fill;
        _deviceGrid.Location = new Point(0, 0);
        _deviceGrid.MultiSelect = false;
        _deviceGrid.Name = "_deviceGrid";
        _deviceGrid.ReadOnly = true;
        _deviceGrid.RowHeadersVisible = false;
        _deviceGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _deviceGrid.Size = new Size(434, 307);
        _deviceGrid.TabIndex = 0;
        _deviceGrid.CellContentClick += DeviceGrid_CellContentClick;
        _deviceGrid.SelectionChanged += DeviceGrid_SelectionChanged;
        // 
        // _addButton
        // 
        _addButton.Location = new Point(3, 7);
        _addButton.Name = "_addButton";
        _addButton.Size = new Size(75, 23);
        _addButton.TabIndex = 0;
        _addButton.Text = "Thêm file...";
        _addButton.Click += AddButton_Click;
        // 
        // _removeButton
        // 
        _removeButton.Location = new Point(84, 7);
        _removeButton.Name = "_removeButton";
        _removeButton.Size = new Size(75, 23);
        _removeButton.TabIndex = 1;
        _removeButton.Text = "Xoá";
        _removeButton.Click += RemoveButton_Click;
        // 
        // _refreshButton
        // 
        _refreshButton.Location = new Point(3, 7);
        _refreshButton.Name = "_refreshButton";
        _refreshButton.Size = new Size(97, 49);
        _refreshButton.TabIndex = 0;
        _refreshButton.Text = "Làm mới";
        _refreshButton.Click += RefreshButton_Click;
        // 
        // _pairWifiButton
        // 
        _pairWifiButton.Location = new Point(106, 7);
        _pairWifiButton.Name = "_pairWifiButton";
        _pairWifiButton.Size = new Size(97, 49);
        _pairWifiButton.TabIndex = 1;
        _pairWifiButton.Text = "Kết nối qua Wi-Fi...";
        _pairWifiButton.Click += PairWifiButton_Click;
        // 
        // _scanWifiButton
        // 
        _scanWifiButton.Location = new Point(209, 7);
        _scanWifiButton.Name = "_scanWifiButton";
        _scanWifiButton.Size = new Size(97, 49);
        _scanWifiButton.TabIndex = 2;
        _scanWifiButton.Text = "Dò mạng Wi-Fi...";
        _scanWifiButton.Click += ScanWifiButton_Click;
        // 
        // _connectIpButton
        // 
        _connectIpButton.Location = new Point(312, 7);
        _connectIpButton.Name = "_connectIpButton";
        _connectIpButton.Size = new Size(97, 49);
        _connectIpButton.TabIndex = 3;
        _connectIpButton.Text = "Kết nối theo IP...";
        _connectIpButton.Click += ConnectIpButton_Click;
        // 
        // _installButton
        // 
        _installButton.Dock = DockStyle.Right;
        _installButton.Enabled = false;
        _installButton.Location = new Point(672, 6);
        _installButton.Name = "_installButton";
        _installButton.Size = new Size(140, 34);
        _installButton.TabIndex = 0;
        _installButton.Text = "Cài đặt";
        _installButton.Visible = false;
        _installButton.Click += InstallButton_Click;
        // 
        // _progress
        // 
        _progress.Dock = DockStyle.Bottom;
        _progress.Location = new Point(0, 470);
        _progress.Name = "_progress";
        _progress.Size = new Size(820, 10);
        _progress.Style = ProgressBarStyle.Continuous;
        _progress.TabIndex = 3;
        _progress.Visible = false;
        // 
        // _status
        // 
        _status.Dock = DockStyle.Bottom;
        _status.Location = new Point(0, 448);
        _status.Name = "_status";
        _status.Padding = new Padding(4, 0, 0, 0);
        _status.Size = new Size(820, 22);
        _status.TabIndex = 2;
        _status.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _devicePoll
        // 
        _devicePoll.Interval = 2000;
        _devicePoll.Tick += DevicePoll_Tick;
        // 
        // _leftPanel
        // 
        _leftPanel.Controls.Add(_fileList);
        _leftPanel.Controls.Add(_leftButtonsPanel);
        _leftPanel.Dock = DockStyle.Fill;
        _leftPanel.Location = new Point(8, 25);
        _leftPanel.Name = "_leftPanel";
        _leftPanel.Size = new Size(350, 369);
        _leftPanel.TabIndex = 0;
        // 
        // _leftButtonsPanel
        // 
        _leftButtonsPanel.Controls.Add(_addButton);
        _leftButtonsPanel.Controls.Add(_removeButton);
        _leftButtonsPanel.Dock = DockStyle.Bottom;
        _leftButtonsPanel.Location = new Point(0, 333);
        _leftButtonsPanel.Name = "_leftButtonsPanel";
        _leftButtonsPanel.Padding = new Padding(0, 4, 0, 0);
        _leftButtonsPanel.Size = new Size(350, 36);
        _leftButtonsPanel.TabIndex = 1;
        // 
        // _leftGroup
        // 
        _leftGroup.Controls.Add(_leftPanel);
        _leftGroup.Dock = DockStyle.Fill;
        _leftGroup.Location = new Point(0, 0);
        _leftGroup.Name = "_leftGroup";
        _leftGroup.Padding = new Padding(8);
        _leftGroup.Size = new Size(366, 402);
        _leftGroup.TabIndex = 0;
        _leftGroup.TabStop = false;
        _leftGroup.Text = "File (kéo-thả .apk / .aab vào đây, hoặc bấm Thêm file)";
        // 
        // _rightPanel
        // 
        _rightPanel.Controls.Add(_deviceGrid);
        _rightPanel.Controls.Add(_rightButtonsPanel);
        _rightPanel.Dock = DockStyle.Fill;
        _rightPanel.Location = new Point(8, 25);
        _rightPanel.Name = "_rightPanel";
        _rightPanel.Size = new Size(434, 369);
        _rightPanel.TabIndex = 0;
        // 
        // _rightButtonsPanel
        // 
        _rightButtonsPanel.Controls.Add(_refreshButton);
        _rightButtonsPanel.Controls.Add(_pairWifiButton);
        _rightButtonsPanel.Controls.Add(_scanWifiButton);
        _rightButtonsPanel.Controls.Add(_connectIpButton);
        _rightButtonsPanel.Dock = DockStyle.Bottom;
        _rightButtonsPanel.Location = new Point(0, 307);
        _rightButtonsPanel.Name = "_rightButtonsPanel";
        _rightButtonsPanel.Padding = new Padding(0, 4, 0, 0);
        _rightButtonsPanel.Size = new Size(434, 62);
        _rightButtonsPanel.TabIndex = 1;
        // 
        // _rightGroup
        // 
        _rightGroup.Controls.Add(_rightPanel);
        _rightGroup.Dock = DockStyle.Fill;
        _rightGroup.Location = new Point(0, 0);
        _rightGroup.Name = "_rightGroup";
        _rightGroup.Padding = new Padding(8);
        _rightGroup.Size = new Size(450, 402);
        _rightGroup.TabIndex = 0;
        _rightGroup.TabStop = false;
        _rightGroup.Text = "Thiết bị (bấm chọn một dòng \"Đã kết nối\" để cài lên đó)";
        // 
        // _splitContainer
        // 
        _splitContainer.Dock = DockStyle.Fill;
        _splitContainer.Location = new Point(0, 0);
        _splitContainer.Name = "_splitContainer";
        // 
        // _splitContainer.Panel1
        // 
        _splitContainer.Panel1.Controls.Add(_leftGroup);
        _splitContainer.Panel1MinSize = 260;
        // 
        // _splitContainer.Panel2
        // 
        _splitContainer.Panel2.Controls.Add(_rightGroup);
        _splitContainer.Panel2MinSize = 260;
        _splitContainer.Size = new Size(820, 402);
        _splitContainer.SplitterDistance = 366;
        _splitContainer.TabIndex = 0;
        // 
        // _bottomPanel
        // 
        _bottomPanel.Controls.Add(_installButton);
        _bottomPanel.Dock = DockStyle.Bottom;
        _bottomPanel.Location = new Point(0, 402);
        _bottomPanel.Name = "_bottomPanel";
        _bottomPanel.Padding = new Padding(8, 6, 8, 6);
        _bottomPanel.Size = new Size(820, 46);
        _bottomPanel.TabIndex = 1;
        //
        // dataGridViewTextBoxColumn1
        //
        dataGridViewTextBoxColumn1.HeaderText = "Thiết bị";
        dataGridViewTextBoxColumn1.Name = "Name";
        dataGridViewTextBoxColumn1.ReadOnly = true;
        //
        // dataGridViewTextBoxColumn2
        //
        dataGridViewTextBoxColumn2.HeaderText = "Serial / Địa chỉ";
        dataGridViewTextBoxColumn2.Name = "Serial";
        dataGridViewTextBoxColumn2.ReadOnly = true;
        //
        // dataGridViewTextBoxColumn3
        //
        dataGridViewTextBoxColumn3.HeaderText = "Trạng thái";
        dataGridViewTextBoxColumn3.Name = "Status";
        dataGridViewTextBoxColumn3.ReadOnly = true;
        //
        // dataGridViewButtonColumn1
        //
        // Name must stay "Action" (not the field's own identifier) -- MainForm.cs's ActionColumnIndex /
        // row.Cells[...] lookups key off this column Name string, not the generated field name.
        dataGridViewButtonColumn1.Name = "Action";
        dataGridViewButtonColumn1.HeaderText = "";
        dataGridViewButtonColumn1.UseColumnTextForButtonValue = false;
        dataGridViewButtonColumn1.FillWeight = 60F;
        dataGridViewButtonColumn1.ReadOnly = true;
        // 
        // MainForm
        // 
        ClientSize = new Size(820, 480);
        Controls.Add(_splitContainer);
        Controls.Add(_bottomPanel);
        Controls.Add(_status);
        Controls.Add(_progress);
        Font = new Font("Segoe UI", 9.5F);
        MinimumSize = new Size(640, 360);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "APK/AAB Installer";
        Load += MainForm_Load;
        ((System.ComponentModel.ISupportInitialize)_deviceGrid).EndInit();
        _leftPanel.ResumeLayout(false);
        _leftButtonsPanel.ResumeLayout(false);
        _leftGroup.ResumeLayout(false);
        _rightPanel.ResumeLayout(false);
        _rightButtonsPanel.ResumeLayout(false);
        _rightGroup.ResumeLayout(false);
        _splitContainer.Panel1.ResumeLayout(false);
        _splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_splitContainer).EndInit();
        _splitContainer.ResumeLayout(false);
        _bottomPanel.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.ListBox _fileList;
    private System.Windows.Forms.DataGridView _deviceGrid;
    private System.Windows.Forms.Button _addButton;
    private System.Windows.Forms.Button _removeButton;
    private System.Windows.Forms.Button _refreshButton;
    private System.Windows.Forms.Button _pairWifiButton;
    private System.Windows.Forms.Button _scanWifiButton;
    private System.Windows.Forms.Button _connectIpButton;
    private System.Windows.Forms.Button _installButton;
    private System.Windows.Forms.ProgressBar _progress;
    private System.Windows.Forms.Label _status;
    private System.Windows.Forms.Timer _devicePoll;
    private System.Windows.Forms.Panel _leftPanel;
    private System.Windows.Forms.FlowLayoutPanel _leftButtonsPanel;
    private System.Windows.Forms.GroupBox _leftGroup;
    private System.Windows.Forms.Panel _rightPanel;
    private System.Windows.Forms.FlowLayoutPanel _rightButtonsPanel;
    private System.Windows.Forms.GroupBox _rightGroup;
    private System.Windows.Forms.SplitContainer _splitContainer;
    private System.Windows.Forms.Panel _bottomPanel;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
    private DataGridViewButtonColumn dataGridViewButtonColumn1;
}
