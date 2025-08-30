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
            // ����Ĭ��ֵ
            SetDefaultValues();
        }

        /// <summary>
        /// ���º��InitMigrator��������UI�ؼ���ȡ����
        /// </summary>
        private void InitMigrator()
        {
            // ��UI�ؼ���ȡ����ֵ
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
                // ����Ѿ�����Ǩ��������ȡ�����ľɵ��¼�
                _migrator.MigrationStarted -= OnMigrationStarted;
                _migrator.TableMigrationStarted -= OnTableMigrationStarted;
                _migrator.BatchMigrationProgress -= OnBatchMigrationProgress;
                _migrator.TableMigrationCompleted -= OnTableMigrationCompleted;
                _migrator.MigrationCompleted -= OnMigrationCompleted;
                _migrator.MigrationFailed -= OnMigrationFailed;
                _migrator = null;
            }

            // 2. ����Ǩ�����������¼�
            _migrator = new MySqlMigrator(migrationConfig);
            // ... �����¼������ֲ��䣩
            _migrator.MigrationStarted += OnMigrationStarted;
            _migrator.TableMigrationStarted += OnTableMigrationStarted;
            _migrator.BatchMigrationProgress += OnBatchMigrationProgress;
            _migrator.TableMigrationCompleted += OnTableMigrationCompleted;
            _migrator.MigrationCompleted += OnMigrationCompleted;
            _migrator.MigrationFailed += OnMigrationFailed;
        }
        /// <summary>
        /// 1. ��ʼ��Serilog��������ı��� + �����ļ�
        /// </summary>
        private void InitSerilog()
        {
            // ����Serilog��
            // - ������ļ��������ڹ���������30�죩
            // - �����WinForm�ı��򣨽���ʾInfo�����ϼ���
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose() // ȫ����ͼ���
                .WriteTo.File(
                    path: $@"Logs\Migration_{DateTime.Now:yyyyMMdd}.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .WriteTo.Sink(new RichTextBoxSink(txtLog)) // �Զ���Sink��������ı���
                .CreateLogger();
        }
        /// <summary>
        /// ���������ַ���
        /// </summary>
        private string BuildConnectionString(string server, string database, string userId, string password)
        {
            return $"server={server};user id={userId};password={password};database={database};charset=utf8;sslMode=None;pooling=true;minpoolsize=1;maxpoolsize=1024;ConnectionLifetime=30;DefaultCommandTimeout=600;AllowLoadLocalInfile=true";
        }
        /// <summary>
        /// ��ȡѡ�еı�
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
        // 4. �¼��ص�����������UI + ��¼��־��
        // ========================================
        /// <summary>
        /// ����Ǩ�ƿ�ʼ
        /// </summary>
        private void OnMigrationStarted(object? sender, EventArgs e)
        {
            Log.Information("����Ǩ���������������{TableCount}��", _migrator?._config.TableNames.Count ?? 0);
        }
        /// <summary>
        /// ������Ǩ�ƿ�ʼ
        /// </summary>
        private void OnTableMigrationStarted(object? sender, string tableName)
        {
            Log.Information("�� ��ʼǨ�Ʊ���{TableName}��", tableName);
        }
        /// <summary>
        /// ����Ǩ�ƽ���
        /// </summary>
        private void OnBatchMigrationProgress(object? sender, (string TableName, int CurrentBatch, int TotalBatches, long MigratedSoFar, long TotalToMigrate) progress)
        {
            var (tableName, currentBatch, totalBatches, migratedSoFar, totalToMigrate) = progress;
            var progressPercent = (double)migratedSoFar / totalToMigrate * 100; // ���Ȱٷֱ�
            Log.Information("�� ��{TableName}�����ȣ���{CurrentBatch}/{TotalBatches}������Ǩ��{MigratedSoFar}/{TotalToMigrate}����{ProgressPercent:F1}%��",
                tableName, currentBatch, totalBatches, migratedSoFar, totalToMigrate, progressPercent);
        }
        /// <summary>
        /// ������Ǩ�����
        /// </summary>
        private void OnTableMigrationCompleted(object? sender, TableMigrationResult result)
        {
            if (result.Success)
            {
                Log.Information("�� ��{TableName}��Ǩ����ɣ���ʱ{Elapsed:F2}�룬Ǩ��{MigratedCount}����ɾ��{DeletedCount}��",
                    result.TableName, result.ElapsedSeconds, result.MigratedCount, result.DeletedCount);
            }
            else
            {
                Log.Warning("�� ��{TableName}��Ǩ��ʧ�ܣ�{Message}", result.TableName, result.Message);
            }
        }
        /// <summary>
        /// ����Ǩ�����
        /// </summary>
        private void OnMigrationCompleted(object? sender, List<TableMigrationResult> results)
        {
            var successCount = results.FindAll(r => r.Success).Count;
            var totalCount = results.Count;
            Log.Information("============== ����Ǩ�ƽ��� ==============");
            Log.Information("Ǩ��ͳ�ƣ���{TotalCount}�����ɹ�{SuccessCount}����ʧ��{FailCount}��",
                totalCount, successCount, totalCount - successCount);
        }
        /// <summary>
        /// Ǩ���쳣
        /// </summary>
        private void OnMigrationFailed(object? sender, (Exception Ex, string? TableName) error)
        {
            var (ex, tableName) = error;
            if (tableName != null)
            {
                Log.Error(ex, "��{TableName}��Ǩ�Ʒ����쳣��{Message}", tableName, ex.Message);
            }
            else
            {
                Log.Error(ex, "ȫ��Ǩ�Ʒ����쳣��{Message}", ex.Message);
            }
        }
        /// <summary>
        /// 5. ����ر�ʱ�ͷ�Serilog
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log.CloseAndFlush(); // ȷ����־д�����
        }
        #endregion

        private async void BtnTestSource_Click(object sender, EventArgs e)
        {
            await TestConnectionAsync(BuildConnectionString(
                  txtSourceServer.Text,
                  txtSourceDatabase.Text,
                  txtSourceUserId.Text,
                  txtSourcePassword.Text), "Դ���ݿ�");
        }

        private async void BtnTestBackup_Click(object sender, EventArgs e)
        {
            await TestConnectionAsync(BuildConnectionString(
                  txtBackupServer.Text,
                  txtBackupDatabase.Text,
                  txtBackupUserId.Text,
                  txtBackupPassword.Text), "�������ݿ�");
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
        /// �������ݿ�����
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
                        MessageBox.Show($"{connectionName}���ӳɹ�!", "���Ӳ���", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{connectionName}����ʧ��: {ex.Message}", "���Ӳ���", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                  -- �ų� MySQL ��ʱ�ڲ�����������ż���ܼ�����
                  AND TABLE_NAME NOT LIKE '#sql%' 
                ORDER BY TABLE_NAME;";

                using var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@db", db);

                var list = new List<string>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    list.Add(reader.GetString(0));

                // ��ѡ���ų����������ֶ�������Ҳ����ϵͳ�⡱�Ŀ�ܱ�
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
                MessageBox.Show($"��ȡ���б�ʧ��: {ex.Message}", "����",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// �������õ��ļ�
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
                    MessageBox.Show("���ñ���ɹ�!", "��������", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"��������ʧ��: {ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// ���ļ���������
        /// </summary>
        private void LoadConfiguration()
        {
            try
            {
                if (!File.Exists("migration_config.json"))
                {
                    MessageBox.Show("�����ļ�������!", "��������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var json = File.ReadAllText("migration_config.json");
                var config = JsonSerializer.Deserialize<dynamic>(json);

                // ����UI�ؼ���������Ҫ����ʵ��JSON�ṹ���е�����
                // ʾ��: txtSourceServer.Text = config.SourceServer;

                MessageBox.Show("���ü��سɹ�!", "��������", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"��������ʧ��: {ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// ����Ĭ��ֵ
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
            // ���֮ǰ�Ĵ�����ʾ
            ClearAllErrorHighlights();

            // ִ�кϷ���У��
            var validationResult = ValidateInput();
            if (!validationResult.IsValid)
            {
                MessageBox.Show(validationResult.ErrorMessage, "������֤ʧ��",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ���ð�ť��ֹ�ظ����
            btnStartMigration.Enabled = false;
            btnCancelMigration.Enabled = true;
            btnStartMigration.Text = "Ǩ����...";
            // �����־�ı���
            txtLog.Text = string.Empty;

            Log.Information("============== ��ʼ���ݿ�Ǩ�� ==============");

            try
            {
                // ��ʼ��Ǩ��������UI��ȡ�������ã�
                InitMigrator();

                if (_migrator == null)
                {
                    Log.Error("Ǩ������ʼ��ʧ��");
                    return;
                }
                _cts = new CancellationTokenSource();
                // �첽ִ��Ǩ��
                await _migrator.ExecuteMigrationAsync(_cts.Token);

                Log.Information("============== Ǩ����� ==============");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ǩ�ƹ����з����쳣");
                MessageBox.Show($"Ǩ��ʧ��: {ex.Message}", "����",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UpdateUIControl(btnStartMigration, () =>
                {
                    btnStartMigration.Enabled = true;
                    btnCancelMigration.Enabled = false;
                    btnStartMigration.Text = "��ʼǨ��";
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
                Log.Warning("Ǩ��ȡ�������ѷ��ͣ����ڴ�����...");
            }
        }

        /// <summary>
        /// ��UI�߳��ϸ��¿ؼ�
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
        /// ��֤����ĺϷ���
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidateInput()
        {

            // 1. ������ݿ�������Ϣ
            if (string.IsNullOrWhiteSpace(txtSourceServer.Text))
            {
                HighlightErrorControl(txtSourceServer, true);
                return (false, "Դ��������ַ����Ϊ��");
            }

            if (string.IsNullOrWhiteSpace(txtSourceDatabase.Text))
            {
                HighlightErrorControl(txtSourceDatabase, true);
                return (false, "Դ���ݿ�������Ϊ��");
            }

            if (string.IsNullOrWhiteSpace(txtSourceUserId.Text))
            {
                HighlightErrorControl(txtSourceUserId, true);
                return (false, "Դ���ݿ��û�������Ϊ��");
            }

            if (string.IsNullOrWhiteSpace(txtSourcePassword.Text))
            {
                HighlightErrorControl(txtSourcePassword, true);
                return (false, "Դ���ݿ����벻��Ϊ��");
            }

            if (string.IsNullOrWhiteSpace(txtBackupServer.Text))
            {
                HighlightErrorControl(txtBackupServer, true);
                return (false, "���ݷ�������ַ����Ϊ��");
            }

            if (string.IsNullOrWhiteSpace(txtBackupDatabase.Text))
            {
                HighlightErrorControl(txtBackupDatabase, true);
                return (false, "�������ݿ�������Ϊ��");
            }

            if (string.IsNullOrWhiteSpace(txtBackupUserId.Text))
            {
                HighlightErrorControl(txtBackupUserId, true);
                return (false, "�������ݿ��û�������Ϊ��");
            }

            if (string.IsNullOrWhiteSpace(txtBackupPassword.Text))
            {
                HighlightErrorControl(txtBackupPassword, true);
                return (false, "�������ݿ����벻��Ϊ��");
            }

            // 2. ����������ַ��ʽ
            if (!IsValidServerAddress(txtSourceServer.Text))
            {
                HighlightErrorControl(txtSourceServer, true);
                return (false, "Դ��������ַ��ʽ����ȷ");
            }

            if (!IsValidServerAddress(txtBackupServer.Text))
            {
                HighlightErrorControl(txtBackupServer, true);
                return (false, "���ݷ�������ַ��ʽ����ȷ");
            }

            // 3. ����Ƿ�ѡ���˱�
            if (clbTables.CheckedItems.Count == 0)
            {
                // ����CheckedListBox�����Ը��������ؼ�������ʾ��ʾ
                clbTables.BackColor = Color.LightPink;
                return (false, "������ѡ��һ��ҪǨ�Ƶ����ݱ�");
            }

            // 4. ���߼�����
            if (nudKeepDays.Value <= 0)
            {
                HighlightErrorControl(nudKeepDays, true);
                return (false, "���������������0");
            }

            if (nudBatchSize.Value <= 0)
            {
                HighlightErrorControl(nudBatchSize, true);
                return (false, "���δ�С�������0");
            }

            // 5. ���Դ��ͱ��ݿ��Ƿ���ͬ
            if (txtSourceServer.Text == txtBackupServer.Text &&
                txtSourceDatabase.Text == txtBackupDatabase.Text)
            {
                var result = MessageBox.Show("Դ���ݿ�ͱ������ݿ���ͬ������ܵ������ݶ�ʧ���Ƿ������",
                    "����", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                {
                    return (false, "������ȡ��");
                }
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// ������ʾ����Ŀؼ�
        /// </summary>
        private void HighlightErrorControl(Control control, bool hasError)
        {
            if (hasError)
            {
                control.BackColor = Color.LightPink;
                // ����ĳЩ�ؼ�����������������Ӿ���ʾ
                if (control is TextBox textBox)
                {
                    textBox.SelectAll(); // ѡ�������ı������޸�
                }
                control.Focus(); // �������Ƶ�����ؼ�
            }
            else
            {
                // �ָ�Ĭ�ϱ���ɫ
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
        /// ������д������
        /// </summary>
        private void ClearAllErrorHighlights()
        {
            // ��������ı���ĸ���
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

                // �ݹ���������ڵĿؼ�
                if (control.HasChildren)
                {
                    ClearChildControls(control);
                }
            }
        }

        /// <summary>
        /// �ݹ�����ӿؼ��ĸ���
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
        /// ��֤��������ַ��ʽ
        /// </summary>
        private bool IsValidServerAddress(string server)
        {
            if (string.IsNullOrWhiteSpace(server))
                return false;

            // ֧��IP��ַ��������localhost
            if (server == "localhost" || server == "127.0.0.1")
                return true;

            // ���IP��ַ��ʽ
            if (System.Net.IPAddress.TryParse(server, out _))
                return true;

            // ���������ʽ������֤��
            if (server.Contains(".") && server.Length > 3)
                return true;

            return false;
        }
    }
}
