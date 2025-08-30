using DataMigrator.Service.Models;
using DataMigrator.Service.Services;
using Serilog;

namespace DataMigrator.App
{
    public partial class FrmSimple : Form
    {
        // 迁移器实例（按需初始化）
        private MySqlMigrator? _migrator;
        public FrmSimple()
        {
            InitializeComponent();
            // 初始化Serilog（程序启动时执行）
            InitSerilog();
            // 初始化迁移器（替换为你的实际配置）
            InitMigrator();
        }

        /// <summary>
        /// 2. 初始化迁移器 + 订阅所有事件
        /// </summary>
        private void InitMigrator()
        {
            // 1. 构建迁移配置（替换为你的实际数据库连接信息）
            var migrationConfig = new MigrationConfig
            {
                SourceConnectionString = "server=192.168.0.189;user id=root;password=root;database=005_mes;charset=utf8;sslMode=None;pooling=true;minpoolsize=1;maxpoolsize=1024;ConnectionLifetime=30;DefaultCommandTimeout=600;",
                BackupConnectionString = "server=192.168.0.122;user id=root;password=root;database=005_mes_backup;charset=utf8;sslMode=None;pooling=true;minpoolsize=1;maxpoolsize=1024;ConnectionLifetime=30;DefaultCommandTimeout=600;",
                TableNames = new List<string>
                  {
                      //"jj_alarm_records",
                      //"jj_plc_alarm",
                      //"xyh_alarm_records"
                      "xyh_plc_loading_and_unloading"
                  },
                KeepRecentDays = 20, // 保留最近30天数据，更早的数据迁移
                BatchSize = 1000, // 每批迁移1000条
                DeleteAfterMigration = true // 迁移后删除源库数据
            };

            // 2. 创建迁移器并订阅事件
            _migrator = new MySqlMigrator(migrationConfig);
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

        /// <summary>
        /// 3. 迁移按钮点击事件（触发迁移）
        /// </summary>
        private async void btnStartMigration_Click(object sender, EventArgs e)
        {
            if (_migrator == null)
            {
                Log.Warning("迁移器未初始化");
                return;
            }

            // 禁用按钮防止重复点击
            btnStartMigration.Enabled = false;
            Log.Information("============== 开始数据库迁移 ==============");

            try
            {
                // 异步执行迁移（避免阻塞UI）
                await _migrator.ExecuteMigrationAsync(CancellationToken.None);
            }
            finally
            {
                // 恢复按钮
                btnStartMigration.Enabled = true;
            }
        }
    }
}
