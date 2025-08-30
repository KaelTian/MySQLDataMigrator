using DataMigrator.Service.Models;
using DataMigrator.Service.Services;
using MySqlConnector;
using Serilog;
using System.Data;
using System.Text.Json;

namespace DataMigrator.App
{
    public partial class FrmMain : Form
    {
        private MySqlMigrator? _migrator;
        private CancellationTokenSource? _cts;
        public FrmMain()
        {
            InitializeComponent();
            InitSerilog();
            // 设置默认值
            SetDefaultValues();
        }

        /// <summary>
        /// 更新后的InitMigrator方法，从UI控件获取配置
        /// </summary>
        private void InitMigrator()
        {
            // 从UI控件获取配置值
            var migrationConfig = new MigrationConfig
            {
                SourceConnectionString = BuildConnectionString(
                    txtSourceServer.Text,
                    txtSourceDatabase.Text,
                    txtSourceUserId.Text,
                    txtSourcePassword.Text),

                BackupConnectionString = BuildConnectionString(
                    txtBackupServer.Text,
                    txtBackupDatabase.Text,
                    txtBackupUserId.Text,
                    txtBackupPassword.Text),

                TableNames = GetSelectedTables(),
                KeepRecentDays = (int)nudKeepDays.Value,
                BatchSize = (int)nudBatchSize.Value,
                DeleteAfterMigration = chkDeleteAfterMigration.Checked
            };

            if (_migrator != null)
            {
                // 如果已经存在迁移器，先取消订阅旧的事件
                _migrator.MigrationStarted -= OnMigrationStarted;
                _migrator.TableMigrationStarted -= OnTableMigrationStarted;
                _migrator.BatchMigrationProgress -= OnBatchMigrationProgress;
                _migrator.TableMigrationCompleted -= OnTableMigrationCompleted;
                _migrator.MigrationCompleted -= OnMigrationCompleted;
                _migrator.MigrationFailed -= OnMigrationFailed;
                _migrator = null;
            }

            // 2. 创建迁移器并订阅事件
            _migrator = new MySqlMigrator(migrationConfig);
            // ... 订阅事件（保持不变）
            _migrator.MigrationStarted += OnMigrationStarted;
            _migrator.TableMigrationStarted += OnTableMigrationStarted;
            _migrator.BatchMigrationProgress += OnBatchMigrationProgress;
            _migrator.TableMigrationCompleted += OnTableMigrationCompleted;
            _migrator.MigrationCompleted += OnMigrationCompleted;
            _migrator.MigrationFailed += OnMigrationFailed;
        }
        /// <summary>
        /// 1. 初始化Serilog：输出到文本框 + 本地文件
        /// </summary>
        private void InitSerilog()
        {
            // 配置Serilog：
            // - 输出到文件（按日期滚动，保留30天）
            // - 输出到WinForm文本框（仅显示Info及以上级别）
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose() // 全局最低级别
                .WriteTo.File(
                    path: $@"Logs\Migration_{DateTime.Now:yyyyMMdd}.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .WriteTo.Sink(new RichTextBoxSink(txtLog)) // 自定义Sink：输出到文本框
                .CreateLogger();
        }
        /// <summary>
        /// 构建连接字符串
        /// </summary>
        private string BuildConnectionString(string server, string database, string userId, string password)
        {
            return $"server={server};user id={userId};password={password};database={database};charset=utf8;sslMode=None;pooling=true;minpoolsize=1;maxpoolsize=1024;ConnectionLifetime=30;DefaultCommandTimeout=600;AllowLoadLocalInfile=true";
        }
        /// <summary>
        /// 获取选中的表
        /// </summary>
        private List<string> GetSelectedTables()
        {
            var selectedTables = new List<string>();
            foreach (var item in clbTables.CheckedItems)
            {
                if (item != null)
                    selectedTables.Add(item.ToString()!);
            }
            return selectedTables;
        }
        #region Update message from migrator.
        // ========================================
        // 4. 事件回调方法（更新UI + 记录日志）
        // ========================================
        /// <summary>
        /// 整体迁移开始
        /// </summary>
        private void OnMigrationStarted(object? sender, EventArgs e)
        {
            Log.Information("整体迁移启动，待处理表：{TableCount}个", _migrator?._config.TableNames.Count ?? 0);
        }
        /// <summary>
        /// 单个表迁移开始
        /// </summary>
        private void OnTableMigrationStarted(object? sender, string tableName)
        {
            Log.Information("→ 开始迁移表：【{TableName}】", tableName);
        }
        /// <summary>
        /// 批量迁移进度
        /// </summary>
        private void OnBatchMigrationProgress(object? sender, (string TableName, int CurrentBatch, int TotalBatches, long MigratedSoFar, long TotalToMigrate) progress)
        {
            var (tableName, currentBatch, totalBatches, migratedSoFar, totalToMigrate) = progress;
            var progressPercent = (double)migratedSoFar / totalToMigrate * 100; // 进度百分比
            Log.Information("→ 表【{TableName}】进度：第{CurrentBatch}/{TotalBatches}批，已迁移{MigratedSoFar}/{TotalToMigrate}条（{ProgressPercent:F1}%）",
                tableName, currentBatch, totalBatches, migratedSoFar, totalToMigrate, progressPercent);
        }
        /// <summary>
        /// 单个表迁移完成
        /// </summary>
        private void OnTableMigrationCompleted(object? sender, TableMigrationResult result)
        {
            if (result.Success)
            {
                Log.Information("→ 表【{TableName}】迁移完成：耗时{Elapsed:F2}秒，迁移{MigratedCount}条，删除{DeletedCount}条",
                    result.TableName, result.ElapsedSeconds, result.MigratedCount, result.DeletedCount);
            }
            else
            {
                Log.Warning("→ 表【{TableName}】迁移失败：{Message}", result.TableName, result.Message);
            }
        }
        /// <summary>
        /// 整体迁移完成
        /// </summary>
        private void OnMigrationCompleted(object? sender, List<TableMigrationResult> results)
        {
            var successCount = results.FindAll(r => r.Success).Count;
            var totalCount = results.Count;
            Log.Information("============== 整体迁移结束 ==============");
            Log.Information("迁移统计：共{TotalCount}个表，成功{SuccessCount}个，失败{FailCount}个",
                totalCount, successCount, totalCount - successCount);
        }
        /// <summary>
        /// 迁移异常
        /// </summary>
        private void OnMigrationFailed(object? sender, (Exception Ex, string? TableName) error)
        {
            var (ex, tableName) = error;
            if (tableName != null)
            {
                Log.Error(ex, "表【{TableName}】迁移发生异常：{Message}", tableName, ex.Message);
            }
            else
            {
                Log.Error(ex, "全局迁移发生异常：{Message}", ex.Message);
            }
        }
        /// <summary>
        /// 5. 窗体关闭时释放Serilog
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log.CloseAndFlush(); // 确保日志写入完成
        }
        #endregion

        private async void BtnTestSource_Click(object sender, EventArgs e)
        {
            await TestConnectionAsync(BuildConnectionString(
                  txtSourceServer.Text,
                  txtSourceDatabase.Text,
                  txtSourceUserId.Text,
                  txtSourcePassword.Text), "源数据库");
        }

        private async void BtnTestBackup_Click(object sender, EventArgs e)
        {
            await TestConnectionAsync(BuildConnectionString(
                  txtBackupServer.Text,
                  txtBackupDatabase.Text,
                  txtBackupUserId.Text,
                  txtBackupPassword.Text), "备份数据库");
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbTables.Items.Count; i++)
                clbTables.SetItemChecked(i, true);
        }

        private void btnDeselectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbTables.Items.Count; i++)
                clbTables.SetItemChecked(i, false);
        }

        private void btnRefreshTables_Click(object sender, EventArgs e)
        {
            BtnRefreshTables_Click(sender, e);
        }

        private async void BtnRefreshTables_Click(object sender, EventArgs e)
        {
            await RefreshTableListAsync();
        }

        private async void BtnSaveConfig_Click(object sender, EventArgs e)
        {
            await SaveConfigurationAsync();
        }

        private void BtnLoadConfig_Click(object sender, EventArgs e)
        {
            LoadConfiguration();
        }

        /// <summary>
        /// 测试数据库连接
        /// </summary>
        private async Task TestConnectionAsync(string connectionString, string connectionName)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    UpdateUIControl(this, () =>
                    {
                        MessageBox.Show($"{connectionName}连接成功!", "连接测试", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{connectionName}连接失败: {ex.Message}", "连接测试", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async Task RefreshTableListAsync()
        {
            try
            {
                clbTables.Items.Clear();

                var db = txtSourceDatabase.Text?.Trim();
                using var connection = new MySqlConnection(BuildConnectionString(
                    txtSourceServer.Text,
                    db!,
                    txtSourceUserId.Text,
                    txtSourcePassword.Text));

                await connection.OpenAsync();

                const string sql = @"
                SELECT TABLE_NAME
                FROM information_schema.tables
                WHERE TABLE_SCHEMA = @db
                  AND TABLE_TYPE = 'BASE TABLE'
                  -- 排除 MySQL 临时内部残留表名（偶尔能见到）
                  AND TABLE_NAME NOT LIKE '#sql%' 
                ORDER BY TABLE_NAME;";

                using var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@db", db);

                var list = new List<string>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    list.Add(reader.GetString(0));

                // 可选：排除“不是你手动建，但也不是系统库”的框架表
                var blacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "__EFMigrationsHistory", "flyway_schema_history",
                    "databasechangelog", "databasechangeloglock"
                };
                list = list.Where(n => !blacklist.Contains(n)).ToList();

                UpdateUIControl(clbTables, () => clbTables.Items.AddRange(list.ToArray()));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取表列表失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        private async Task SaveConfigurationAsync()
        {
            try
            {
                var config = new
                {
                    SourceServer = txtSourceServer.Text,
                    SourceDatabase = txtSourceDatabase.Text,
                    SourceUserId = txtSourceUserId.Text,
                    SourcePassword = txtSourcePassword.Text,
                    BackupServer = txtBackupServer.Text,
                    BackupDatabase = txtBackupDatabase.Text,
                    BackupUserId = txtBackupUserId.Text,
                    BackupPassword = txtBackupPassword.Text,
                    KeepDays = nudKeepDays.Value,
                    BatchSize = nudBatchSize.Value,
                    DeleteAfterMigration = chkDeleteAfterMigration.Checked,
                    SelectedTables = GetSelectedTables()
                };

                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync("migration_config.json", json);
                UpdateUIControl(this, () =>
                {
                    MessageBox.Show("配置保存成功!", "保存配置", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 从文件加载配置
        /// </summary>
        private void LoadConfiguration()
        {
            try
            {
                if (!File.Exists("migration_config.json"))
                {
                    MessageBox.Show("配置文件不存在!", "加载配置", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var json = File.ReadAllText("migration_config.json");
                var config = JsonSerializer.Deserialize<dynamic>(json);

                // 更新UI控件（这里需要根据实际JSON结构进行调整）
                // 示例: txtSourceServer.Text = config.SourceServer;

                MessageBox.Show("配置加载成功!", "加载配置", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载配置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 设置默认值
        /// </summary>
        private void SetDefaultValues()
        {
            txtSourceServer.Text = "192.168.0.189";
            txtSourceDatabase.Text = "005_mes";
            txtSourceUserId.Text = "root";
            txtSourcePassword.Text = "root";

            txtBackupServer.Text = "192.168.0.122";
            txtBackupDatabase.Text = "005_mes_backup";
            txtBackupUserId.Text = "root";
            txtBackupPassword.Text = "root";

            btnCancelMigration.Enabled = false;
        }

        private async void btnStartMigration_Click(object sender, EventArgs e)
        {
            // 清除之前的错误提示
            ClearAllErrorHighlights();

            // 执行合法性校验
            var validationResult = ValidateInput();
            if (!validationResult.IsValid)
            {
                MessageBox.Show(validationResult.ErrorMessage, "输入验证失败",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 禁用按钮防止重复点击
            btnStartMigration.Enabled = false;
            btnCancelMigration.Enabled = true;
            btnStartMigration.Text = "迁移中...";
            // 清空日志文本框
            txtLog.Text = string.Empty;

            Log.Information("============== 开始数据库迁移 ==============");

            try
            {
                // 初始化迁移器（从UI获取最新配置）
                InitMigrator();

                if (_migrator == null)
                {
                    Log.Error("迁移器初始化失败");
                    return;
                }
                _cts = new CancellationTokenSource();
                // 异步执行迁移
                await _migrator.ExecuteMigrationAsync(_cts.Token);

                Log.Information("============== 迁移完成 ==============");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "迁移过程中发生异常");
                MessageBox.Show($"迁移失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UpdateUIControl(btnStartMigration, () =>
                {
                    btnStartMigration.Enabled = true;
                    btnCancelMigration.Enabled = false;
                    btnStartMigration.Text = "开始迁移";
                });
                if (_cts != null)
                {
                    _cts.Dispose();
                    _cts = null;
                }

            }
        }

        private void btnCancelMigration_Click(object sender, EventArgs e)
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                Log.Warning("迁移取消请求已发送，正在处理中...");
            }
        }

        /// <summary>
        /// 在UI线程上更新控件
        /// </summary>
        /// <param name="control"></param>
        /// <param name="action"></param>
        private void UpdateUIControl(Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }

        /// <summary>
        /// 验证输入的合法性
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidateInput()
        {

            // 1. 检查数据库连接信息
            if (string.IsNullOrWhiteSpace(txtSourceServer.Text))
            {
                HighlightErrorControl(txtSourceServer, true);
                return (false, "源服务器地址不能为空");
            }

            if (string.IsNullOrWhiteSpace(txtSourceDatabase.Text))
            {
                HighlightErrorControl(txtSourceDatabase, true);
                return (false, "源数据库名不能为空");
            }

            if (string.IsNullOrWhiteSpace(txtSourceUserId.Text))
            {
                HighlightErrorControl(txtSourceUserId, true);
                return (false, "源数据库用户名不能为空");
            }

            if (string.IsNullOrWhiteSpace(txtSourcePassword.Text))
            {
                HighlightErrorControl(txtSourcePassword, true);
                return (false, "源数据库密码不能为空");
            }

            if (string.IsNullOrWhiteSpace(txtBackupServer.Text))
            {
                HighlightErrorControl(txtBackupServer, true);
                return (false, "备份服务器地址不能为空");
            }

            if (string.IsNullOrWhiteSpace(txtBackupDatabase.Text))
            {
                HighlightErrorControl(txtBackupDatabase, true);
                return (false, "备份数据库名不能为空");
            }

            if (string.IsNullOrWhiteSpace(txtBackupUserId.Text))
            {
                HighlightErrorControl(txtBackupUserId, true);
                return (false, "备份数据库用户名不能为空");
            }

            if (string.IsNullOrWhiteSpace(txtBackupPassword.Text))
            {
                HighlightErrorControl(txtBackupPassword, true);
                return (false, "备份数据库密码不能为空");
            }

            // 2. 检查服务器地址格式
            if (!IsValidServerAddress(txtSourceServer.Text))
            {
                HighlightErrorControl(txtSourceServer, true);
                return (false, "源服务器地址格式不正确");
            }

            if (!IsValidServerAddress(txtBackupServer.Text))
            {
                HighlightErrorControl(txtBackupServer, true);
                return (false, "备份服务器地址格式不正确");
            }

            // 3. 检查是否选择了表
            if (clbTables.CheckedItems.Count == 0)
            {
                // 对于CheckedListBox，可以高亮整个控件或者显示提示
                clbTables.BackColor = Color.LightPink;
                return (false, "请至少选择一个要迁移的数据表");
            }

            // 4. 检查高级设置
            if (nudKeepDays.Value <= 0)
            {
                HighlightErrorControl(nudKeepDays, true);
                return (false, "保留天数必须大于0");
            }

            if (nudBatchSize.Value <= 0)
            {
                HighlightErrorControl(nudBatchSize, true);
                return (false, "批次大小必须大于0");
            }

            // 5. 检查源库和备份库是否相同
            if (txtSourceServer.Text == txtBackupServer.Text &&
                txtSourceDatabase.Text == txtBackupDatabase.Text)
            {
                var result = MessageBox.Show("源数据库和备份数据库相同，这可能导致数据丢失！是否继续？",
                    "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                {
                    return (false, "操作已取消");
                }
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// 高亮显示错误的控件
        /// </summary>
        private void HighlightErrorControl(Control control, bool hasError)
        {
            if (hasError)
            {
                control.BackColor = Color.LightPink;
                // 对于某些控件，还可以添加其他视觉提示
                if (control is TextBox textBox)
                {
                    textBox.SelectAll(); // 选中所有文本方便修改
                }
                control.Focus(); // 将焦点移到错误控件
            }
            else
            {
                // 恢复默认背景色
                if (control is TextBox || control is NumericUpDown)
                {
                    control.BackColor = SystemColors.Window;
                }
                else if (control is CheckedListBox)
                {
                    control.BackColor = SystemColors.Window;
                }
            }
        }

        /// <summary>
        /// 清除所有错误高亮
        /// </summary>
        private void ClearAllErrorHighlights()
        {
            // 清除所有文本框的高亮
            foreach (Control control in this.Controls)
            {
                if (control is TextBox textBox)
                {
                    textBox.BackColor = SystemColors.Window;
                }
                else if (control is NumericUpDown numericUpDown)
                {
                    numericUpDown.BackColor = SystemColors.Window;
                }
                else if (control is CheckedListBox checkedListBox)
                {
                    checkedListBox.BackColor = SystemColors.Window;
                }

                // 递归清除容器内的控件
                if (control.HasChildren)
                {
                    ClearChildControls(control);
                }
            }
        }

        /// <summary>
        /// 递归清除子控件的高亮
        /// </summary>
        private void ClearChildControls(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is TextBox textBox)
                {
                    textBox.BackColor = SystemColors.Window;
                }
                else if (control is NumericUpDown numericUpDown)
                {
                    numericUpDown.BackColor = SystemColors.Window;
                }
                else if (control is CheckedListBox checkedListBox)
                {
                    checkedListBox.BackColor = SystemColors.Window;
                }

                if (control.HasChildren)
                {
                    ClearChildControls(control);
                }
            }
        }
        /// <summary>
        /// 验证服务器地址格式
        /// </summary>
        private bool IsValidServerAddress(string server)
        {
            if (string.IsNullOrWhiteSpace(server))
                return false;

            // 支持IP地址、域名、localhost
            if (server == "localhost" || server == "127.0.0.1")
                return true;

            // 检查IP地址格式
            if (System.Net.IPAddress.TryParse(server, out _))
                return true;

            // 检查域名格式（简单验证）
            if (server.Contains(".") && server.Length > 3)
                return true;

            return false;
        }
    }
}
