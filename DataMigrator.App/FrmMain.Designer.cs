namespace DataMigrator.App
{
    partial class FrmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            splitContainer1 = new SplitContainer();
            tabControl = new TabControl();
            tabPageDatabase = new TabPage();
            tableLayoutPanel1 = new TableLayoutPanel();
            grpBackup = new GroupBox();
            btnTestBackup = new Button();
            txtBackupPassword = new TextBox();
            lblBackupPassword = new Label();
            txtBackupUserId = new TextBox();
            lblBackupUserId = new Label();
            txtBackupDatabase = new TextBox();
            lblBackupDatabase = new Label();
            txtBackupServer = new TextBox();
            lblBackupServer = new Label();
            grpSource = new GroupBox();
            btnTestSource = new Button();
            txtSourcePassword = new TextBox();
            lblSourcePassword = new Label();
            txtSourceUserId = new TextBox();
            lblSourceUserId = new Label();
            txtSourceDatabase = new TextBox();
            lblSourceDatabase = new Label();
            txtSourceServer = new TextBox();
            lblSourceServer = new Label();
            tabPageTables = new TabPage();
            btnRefreshTables = new Button();
            btnDeselectAll = new Button();
            btnSelectAll = new Button();
            clbTables = new CheckedListBox();
            tabPageAdvanced = new TabPage();
            label1 = new Label();
            btnLoadConfig = new Button();
            btnSaveConfig = new Button();
            chkDeleteAfterMigration = new CheckBox();
            nudBatchSize = new NumericUpDown();
            lblBatchSize = new Label();
            nudKeepDays = new NumericUpDown();
            lblKeepDays = new Label();
            splitContainer2 = new SplitContainer();
            txtLog = new RichTextBox();
            btnCancelMigration = new Button();
            btnStartMigration = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tabControl.SuspendLayout();
            tabPageDatabase.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            grpBackup.SuspendLayout();
            grpSource.SuspendLayout();
            tabPageTables.SuspendLayout();
            tabPageAdvanced.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudBatchSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudKeepDays).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(4);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tabControl);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(897, 881);
            splitContainer1.SplitterDistance = 467;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPageDatabase);
            tabControl.Controls.Add(tabPageTables);
            tabControl.Controls.Add(tabPageAdvanced);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Margin = new Padding(4);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(897, 467);
            tabControl.TabIndex = 0;
            // 
            // tabPageDatabase
            // 
            tabPageDatabase.Controls.Add(tableLayoutPanel1);
            tabPageDatabase.Location = new Point(4, 33);
            tabPageDatabase.Margin = new Padding(4);
            tabPageDatabase.Name = "tabPageDatabase";
            tabPageDatabase.Padding = new Padding(4);
            tabPageDatabase.Size = new Size(889, 430);
            tabPageDatabase.TabIndex = 0;
            tabPageDatabase.Text = "数据库配置";
            tabPageDatabase.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(grpBackup, 1, 0);
            tableLayoutPanel1.Controls.Add(grpSource, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(4, 4);
            tableLayoutPanel1.Margin = new Padding(4);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 433F));
            tableLayoutPanel1.Size = new Size(881, 422);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // grpBackup
            // 
            grpBackup.Controls.Add(btnTestBackup);
            grpBackup.Controls.Add(txtBackupPassword);
            grpBackup.Controls.Add(lblBackupPassword);
            grpBackup.Controls.Add(txtBackupUserId);
            grpBackup.Controls.Add(lblBackupUserId);
            grpBackup.Controls.Add(txtBackupDatabase);
            grpBackup.Controls.Add(lblBackupDatabase);
            grpBackup.Controls.Add(txtBackupServer);
            grpBackup.Controls.Add(lblBackupServer);
            grpBackup.Dock = DockStyle.Fill;
            grpBackup.Location = new Point(444, 4);
            grpBackup.Margin = new Padding(4);
            grpBackup.Name = "grpBackup";
            grpBackup.Padding = new Padding(4);
            grpBackup.Size = new Size(433, 414);
            grpBackup.TabIndex = 1;
            grpBackup.TabStop = false;
            grpBackup.Text = "备份数据库";
            // 
            // btnTestBackup
            // 
            btnTestBackup.Location = new Point(292, 285);
            btnTestBackup.Name = "btnTestBackup";
            btnTestBackup.Size = new Size(115, 42);
            btnTestBackup.TabIndex = 8;
            btnTestBackup.Text = "测试连接";
            btnTestBackup.UseVisualStyleBackColor = true;
            btnTestBackup.Click += BtnTestBackup_Click;
            // 
            // txtBackupPassword
            // 
            txtBackupPassword.Location = new Point(124, 233);
            txtBackupPassword.Name = "txtBackupPassword";
            txtBackupPassword.PasswordChar = '*';
            txtBackupPassword.Size = new Size(283, 30);
            txtBackupPassword.TabIndex = 7;
            // 
            // lblBackupPassword
            // 
            lblBackupPassword.AutoSize = true;
            lblBackupPassword.Location = new Point(8, 237);
            lblBackupPassword.Name = "lblBackupPassword";
            lblBackupPassword.Size = new Size(50, 24);
            lblBackupPassword.TabIndex = 6;
            lblBackupPassword.Text = "密码:";
            // 
            // txtBackupUserId
            // 
            txtBackupUserId.Location = new Point(124, 170);
            txtBackupUserId.Name = "txtBackupUserId";
            txtBackupUserId.Size = new Size(283, 30);
            txtBackupUserId.TabIndex = 5;
            // 
            // lblBackupUserId
            // 
            lblBackupUserId.AutoSize = true;
            lblBackupUserId.Location = new Point(8, 174);
            lblBackupUserId.Name = "lblBackupUserId";
            lblBackupUserId.Size = new Size(68, 24);
            lblBackupUserId.TabIndex = 4;
            lblBackupUserId.Text = "用户名:";
            // 
            // txtBackupDatabase
            // 
            txtBackupDatabase.Location = new Point(124, 111);
            txtBackupDatabase.Name = "txtBackupDatabase";
            txtBackupDatabase.Size = new Size(283, 30);
            txtBackupDatabase.TabIndex = 3;
            // 
            // lblBackupDatabase
            // 
            lblBackupDatabase.AutoSize = true;
            lblBackupDatabase.Location = new Point(8, 115);
            lblBackupDatabase.Name = "lblBackupDatabase";
            lblBackupDatabase.Size = new Size(68, 24);
            lblBackupDatabase.TabIndex = 2;
            lblBackupDatabase.Text = "数据库:";
            // 
            // txtBackupServer
            // 
            txtBackupServer.Location = new Point(124, 51);
            txtBackupServer.Name = "txtBackupServer";
            txtBackupServer.Size = new Size(283, 30);
            txtBackupServer.TabIndex = 1;
            // 
            // lblBackupServer
            // 
            lblBackupServer.AutoSize = true;
            lblBackupServer.Location = new Point(8, 55);
            lblBackupServer.Name = "lblBackupServer";
            lblBackupServer.Size = new Size(68, 24);
            lblBackupServer.TabIndex = 0;
            lblBackupServer.Text = "服务器:";
            // 
            // grpSource
            // 
            grpSource.Controls.Add(btnTestSource);
            grpSource.Controls.Add(txtSourcePassword);
            grpSource.Controls.Add(lblSourcePassword);
            grpSource.Controls.Add(txtSourceUserId);
            grpSource.Controls.Add(lblSourceUserId);
            grpSource.Controls.Add(txtSourceDatabase);
            grpSource.Controls.Add(lblSourceDatabase);
            grpSource.Controls.Add(txtSourceServer);
            grpSource.Controls.Add(lblSourceServer);
            grpSource.Dock = DockStyle.Fill;
            grpSource.Location = new Point(4, 4);
            grpSource.Margin = new Padding(4);
            grpSource.Name = "grpSource";
            grpSource.Padding = new Padding(4);
            grpSource.Size = new Size(432, 414);
            grpSource.TabIndex = 0;
            grpSource.TabStop = false;
            grpSource.Text = "源数据库";
            // 
            // btnTestSource
            // 
            btnTestSource.Location = new Point(292, 285);
            btnTestSource.Name = "btnTestSource";
            btnTestSource.Size = new Size(115, 42);
            btnTestSource.TabIndex = 8;
            btnTestSource.Text = "测试连接";
            btnTestSource.UseVisualStyleBackColor = true;
            btnTestSource.Click += BtnTestSource_Click;
            // 
            // txtSourcePassword
            // 
            txtSourcePassword.Location = new Point(124, 233);
            txtSourcePassword.Name = "txtSourcePassword";
            txtSourcePassword.PasswordChar = '*';
            txtSourcePassword.Size = new Size(283, 30);
            txtSourcePassword.TabIndex = 7;
            // 
            // lblSourcePassword
            // 
            lblSourcePassword.AutoSize = true;
            lblSourcePassword.Location = new Point(7, 236);
            lblSourcePassword.Name = "lblSourcePassword";
            lblSourcePassword.Size = new Size(50, 24);
            lblSourcePassword.TabIndex = 6;
            lblSourcePassword.Text = "密码:";
            // 
            // txtSourceUserId
            // 
            txtSourceUserId.Location = new Point(124, 170);
            txtSourceUserId.Name = "txtSourceUserId";
            txtSourceUserId.Size = new Size(283, 30);
            txtSourceUserId.TabIndex = 5;
            // 
            // lblSourceUserId
            // 
            lblSourceUserId.AutoSize = true;
            lblSourceUserId.Location = new Point(7, 173);
            lblSourceUserId.Name = "lblSourceUserId";
            lblSourceUserId.Size = new Size(68, 24);
            lblSourceUserId.TabIndex = 4;
            lblSourceUserId.Text = "用户名:";
            // 
            // txtSourceDatabase
            // 
            txtSourceDatabase.Location = new Point(124, 111);
            txtSourceDatabase.Name = "txtSourceDatabase";
            txtSourceDatabase.Size = new Size(283, 30);
            txtSourceDatabase.TabIndex = 3;
            // 
            // lblSourceDatabase
            // 
            lblSourceDatabase.AutoSize = true;
            lblSourceDatabase.Location = new Point(7, 114);
            lblSourceDatabase.Name = "lblSourceDatabase";
            lblSourceDatabase.Size = new Size(68, 24);
            lblSourceDatabase.TabIndex = 2;
            lblSourceDatabase.Text = "数据库:";
            // 
            // txtSourceServer
            // 
            txtSourceServer.Location = new Point(124, 51);
            txtSourceServer.Name = "txtSourceServer";
            txtSourceServer.Size = new Size(283, 30);
            txtSourceServer.TabIndex = 1;
            // 
            // lblSourceServer
            // 
            lblSourceServer.AutoSize = true;
            lblSourceServer.Location = new Point(7, 54);
            lblSourceServer.Name = "lblSourceServer";
            lblSourceServer.Size = new Size(68, 24);
            lblSourceServer.TabIndex = 0;
            lblSourceServer.Text = "服务器:";
            // 
            // tabPageTables
            // 
            tabPageTables.Controls.Add(btnRefreshTables);
            tabPageTables.Controls.Add(btnDeselectAll);
            tabPageTables.Controls.Add(btnSelectAll);
            tabPageTables.Controls.Add(clbTables);
            tabPageTables.Location = new Point(4, 29);
            tabPageTables.Margin = new Padding(4);
            tabPageTables.Name = "tabPageTables";
            tabPageTables.Padding = new Padding(4);
            tabPageTables.Size = new Size(889, 434);
            tabPageTables.TabIndex = 1;
            tabPageTables.Text = "表选择";
            tabPageTables.UseVisualStyleBackColor = true;
            // 
            // btnRefreshTables
            // 
            btnRefreshTables.Location = new Point(740, 228);
            btnRefreshTables.Name = "btnRefreshTables";
            btnRefreshTables.Size = new Size(115, 42);
            btnRefreshTables.TabIndex = 11;
            btnRefreshTables.Text = "刷新表列表";
            btnRefreshTables.UseVisualStyleBackColor = true;
            btnRefreshTables.Click += btnRefreshTables_Click;
            // 
            // btnDeselectAll
            // 
            btnDeselectAll.Location = new Point(740, 136);
            btnDeselectAll.Name = "btnDeselectAll";
            btnDeselectAll.Size = new Size(115, 42);
            btnDeselectAll.TabIndex = 10;
            btnDeselectAll.Text = "全不选";
            btnDeselectAll.UseVisualStyleBackColor = true;
            btnDeselectAll.Click += btnDeselectAll_Click;
            // 
            // btnSelectAll
            // 
            btnSelectAll.Location = new Point(740, 44);
            btnSelectAll.Name = "btnSelectAll";
            btnSelectAll.Size = new Size(115, 42);
            btnSelectAll.TabIndex = 9;
            btnSelectAll.Text = "全选";
            btnSelectAll.UseVisualStyleBackColor = true;
            btnSelectAll.Click += btnSelectAll_Click;
            // 
            // clbTables
            // 
            clbTables.FormattingEnabled = true;
            clbTables.Location = new Point(3, 7);
            clbTables.Name = "clbTables";
            clbTables.Size = new Size(440, 404);
            clbTables.TabIndex = 0;
            // 
            // tabPageAdvanced
            // 
            tabPageAdvanced.Controls.Add(label1);
            tabPageAdvanced.Controls.Add(btnLoadConfig);
            tabPageAdvanced.Controls.Add(btnSaveConfig);
            tabPageAdvanced.Controls.Add(chkDeleteAfterMigration);
            tabPageAdvanced.Controls.Add(nudBatchSize);
            tabPageAdvanced.Controls.Add(lblBatchSize);
            tabPageAdvanced.Controls.Add(nudKeepDays);
            tabPageAdvanced.Controls.Add(lblKeepDays);
            tabPageAdvanced.Location = new Point(4, 33);
            tabPageAdvanced.Margin = new Padding(4);
            tabPageAdvanced.Name = "tabPageAdvanced";
            tabPageAdvanced.Padding = new Padding(4);
            tabPageAdvanced.Size = new Size(889, 430);
            tabPageAdvanced.TabIndex = 2;
            tabPageAdvanced.Text = "高级设置";
            tabPageAdvanced.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(260, 317);
            label1.Name = "label1";
            label1.Size = new Size(451, 24);
            label1.TabIndex = 14;
            label1.Text = "路径为Tool运行目录下的 'migration_config.json' 文件";
            label1.Click += label1_Click;
            // 
            // btnLoadConfig
            // 
            btnLoadConfig.Location = new Point(260, 364);
            btnLoadConfig.Name = "btnLoadConfig";
            btnLoadConfig.Size = new Size(115, 42);
            btnLoadConfig.TabIndex = 13;
            btnLoadConfig.Text = "加载配置";
            btnLoadConfig.UseVisualStyleBackColor = true;
            btnLoadConfig.Click += BtnLoadConfig_Click;
            // 
            // btnSaveConfig
            // 
            btnSaveConfig.Location = new Point(260, 254);
            btnSaveConfig.Name = "btnSaveConfig";
            btnSaveConfig.Size = new Size(115, 42);
            btnSaveConfig.TabIndex = 12;
            btnSaveConfig.Text = "保存配置";
            btnSaveConfig.UseVisualStyleBackColor = true;
            btnSaveConfig.Click += BtnSaveConfig_Click;
            // 
            // chkDeleteAfterMigration
            // 
            chkDeleteAfterMigration.AutoSize = true;
            chkDeleteAfterMigration.Location = new Point(260, 192);
            chkDeleteAfterMigration.Name = "chkDeleteAfterMigration";
            chkDeleteAfterMigration.Size = new Size(176, 28);
            chkDeleteAfterMigration.TabIndex = 5;
            chkDeleteAfterMigration.Text = "迁移后删除源数据";
            chkDeleteAfterMigration.UseVisualStyleBackColor = true;
            // 
            // nudBatchSize
            // 
            nudBatchSize.Location = new Point(260, 119);
            nudBatchSize.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudBatchSize.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            nudBatchSize.Name = "nudBatchSize";
            nudBatchSize.Size = new Size(176, 30);
            nudBatchSize.TabIndex = 4;
            nudBatchSize.Value = new decimal(new int[] { 40000, 0, 0, 0 });
            // 
            // lblBatchSize
            // 
            lblBatchSize.AutoSize = true;
            lblBatchSize.Location = new Point(103, 121);
            lblBatchSize.Name = "lblBatchSize";
            lblBatchSize.Size = new Size(86, 24);
            lblBatchSize.TabIndex = 3;
            lblBatchSize.Text = "批次大小:";
            // 
            // nudKeepDays
            // 
            nudKeepDays.Location = new Point(260, 33);
            nudKeepDays.Maximum = new decimal(new int[] { 365, 0, 0, 0 });
            nudKeepDays.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudKeepDays.Name = "nudKeepDays";
            nudKeepDays.Size = new Size(176, 30);
            nudKeepDays.TabIndex = 2;
            nudKeepDays.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // lblKeepDays
            // 
            lblKeepDays.AutoSize = true;
            lblKeepDays.Location = new Point(103, 39);
            lblKeepDays.Name = "lblKeepDays";
            lblKeepDays.Size = new Size(86, 24);
            lblKeepDays.TabIndex = 1;
            lblKeepDays.Text = "保留天数:";
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Margin = new Padding(4);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(txtLog);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(btnCancelMigration);
            splitContainer2.Panel2.Controls.Add(btnStartMigration);
            splitContainer2.Size = new Size(897, 409);
            splitContainer2.SplitterDistance = 700;
            splitContainer2.SplitterWidth = 5;
            splitContainer2.TabIndex = 0;
            // 
            // txtLog
            // 
            txtLog.Dock = DockStyle.Fill;
            txtLog.Location = new Point(0, 0);
            txtLog.Margin = new Padding(4);
            txtLog.Name = "txtLog";
            txtLog.Size = new Size(700, 409);
            txtLog.TabIndex = 0;
            txtLog.Text = "";
            // 
            // btnCancelMigration
            // 
            btnCancelMigration.Location = new Point(39, 127);
            btnCancelMigration.Margin = new Padding(4, 4, 4, 24);
            btnCancelMigration.Name = "btnCancelMigration";
            btnCancelMigration.Size = new Size(115, 42);
            btnCancelMigration.TabIndex = 1;
            btnCancelMigration.Text = "取消迁移";
            btnCancelMigration.UseVisualStyleBackColor = true;
            btnCancelMigration.Click += btnCancelMigration_Click;
            // 
            // btnStartMigration
            // 
            btnStartMigration.Location = new Point(39, 34);
            btnStartMigration.Margin = new Padding(4, 4, 4, 24);
            btnStartMigration.Name = "btnStartMigration";
            btnStartMigration.Size = new Size(115, 42);
            btnStartMigration.TabIndex = 0;
            btnStartMigration.Text = "开始迁移";
            btnStartMigration.UseVisualStyleBackColor = true;
            btnStartMigration.Click += btnStartMigration_Click;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(897, 881);
            Controls.Add(splitContainer1);
            Font = new Font("Microsoft YaHei UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 134);
            Margin = new Padding(4);
            Name = "FrmMain";
            Text = "MySql数据库迁移工具";
            FormClosing += MainForm_FormClosing;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            tabPageDatabase.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            grpBackup.ResumeLayout(false);
            grpBackup.PerformLayout();
            grpSource.ResumeLayout(false);
            grpSource.PerformLayout();
            tabPageTables.ResumeLayout(false);
            tabPageAdvanced.ResumeLayout(false);
            tabPageAdvanced.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudBatchSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudKeepDays).EndInit();
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private TabControl tabControl;
        private TabPage tabPageDatabase;
        private TabPage tabPageTables;
        private SplitContainer splitContainer2;
        private RichTextBox txtLog;
        private Button btnStartMigration;
        private TabPage tabPageAdvanced;
        private TableLayoutPanel tableLayoutPanel1;
        private GroupBox grpSource;
        private TextBox txtSourceServer;
        private Label lblSourceServer;
        private Label lblSourceDatabase;
        private TextBox txtSourceDatabase;
        private Label lblSourceUserId;
        private TextBox txtSourceUserId;
        private Label lblSourcePassword;
        private TextBox txtSourcePassword;
        private Button btnTestSource;
        private GroupBox grpBackup;
        private Button btnTestBackup;
        private TextBox txtBackupPassword;
        private Label lblBackupPassword;
        private TextBox txtBackupUserId;
        private Label lblBackupUserId;
        private TextBox txtBackupDatabase;
        private Label lblBackupDatabase;
        private TextBox txtBackupServer;
        private Label lblBackupServer;
        private CheckedListBox clbTables;
        private Button btnSelectAll;
        private Button btnRefreshTables;
        private Button btnDeselectAll;
        private NumericUpDown nudKeepDays;
        private Label lblKeepDays;
        private Label lblBatchSize;
        private NumericUpDown nudBatchSize;
        private CheckBox chkDeleteAfterMigration;
        private Button btnLoadConfig;
        private Button btnSaveConfig;
        private Button btnCancelMigration;
        private Label label1;
    }
}
