using DataMigrator.Service.Models;
using MySqlConnector;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace DataMigrator.Service.Services
{
    /// <summary>
    /// MySQL数据库迁移实现
    /// </summary>
    public class MySqlMigrator : IMigrator
    {
        // ========================================
        // 1. 声明回调事件（供WinForm订阅）
        // ========================================
        /// <summary>
        /// 整体迁移开始事件
        /// </summary>
        public event EventHandler? MigrationStarted;

        /// <summary>
        /// 单个表迁移开始事件（参数：表名）
        /// </summary>
        public event EventHandler<string>? TableMigrationStarted;

        /// <summary>
        /// 批量迁移进度事件（参数：表名、当前批次、总批次、已迁移条数、总待迁移条数）
        /// </summary>
        public event EventHandler<(string TableName, int CurrentBatch, int TotalBatches, long MigratedSoFar, long TotalToMigrate)>? BatchMigrationProgress;

        /// <summary>
        /// 单个表迁移完成事件（参数：表迁移结果）
        /// </summary>
        public event EventHandler<TableMigrationResult>? TableMigrationCompleted;

        /// <summary>
        /// 整体迁移完成事件（参数：所有表迁移结果列表）
        /// </summary>
        public event EventHandler<List<TableMigrationResult>>? MigrationCompleted;

        /// <summary>
        /// 迁移异常事件（参数：异常信息、关联表名（可为null））
        /// </summary>
        public event EventHandler<(Exception Ex, string? TableName)>? MigrationFailed;

        public readonly MigrationConfig _config;
        /// <summary>
        /// 累计已迁移总数(存储到备份库的数据量)
        /// </summary>
        private long currentMigratedCount = 0;

        public MySqlMigrator(MigrationConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }
        /// <summary>
        /// 执行数据迁移
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<List<TableMigrationResult>> ExecuteMigrationAsync(CancellationToken cancellationToken)
        {
            var results = new List<TableMigrationResult>();
            try
            {
                // 触发【整体迁移开始】事件
                MigrationStarted?.Invoke(this, EventArgs.Empty);
                foreach (var tableName in _config.TableNames)
                {
                    var result = new TableMigrationResult { TableName = tableName };
                    var stopwatch = new System.Diagnostics.Stopwatch();
                    try
                    {
                        // 触发【单个表迁移开始】事件
                        TableMigrationStarted?.Invoke(this, tableName);
                        stopwatch.Start();
                        // --------------------------
                        // 原有迁移逻辑（不变）
                        // --------------------------
                        // 1. 检查备份库是否存在该表，不存在则创建
                        if (!await CheckTableStructureAsync(tableName))
                        {
                            if (!await CreateTableInBackupAsync(tableName))
                            {
                                result.Success = false;
                                result.Message = "创建备份表结构失败";
                                results.Add(result);
                                continue;
                            }
                        }

                        // 2. 计算迁移时间点 (保留最近N天的数据)
                        var cutoffTime = DateTime.Now.AddDays(-_config.KeepRecentDays);

                        // 3. 获取需要迁移的数据量
                        var countToMigrate = await GetRecordCountToMigrateAsync(tableName, cutoffTime);
                        if (countToMigrate <= 0)
                        {
                            result.Success = true;
                            result.Message = "没有需要迁移的数据";
                            results.Add(result);
                            continue;
                        }
                        //// 4. 批量迁移数据
                        //var migratedCount = await MigrateRecordsAsync(tableName, cutoffTime, countToMigrate, cancellationToken);

                        //// 5. 迁移成功后删除源库数据
                        //long deletedCount = 0;
                        //if (_config.DeleteAfterMigration && migratedCount > 0)
                        //{
                        //    deletedCount = await DeleteMigratedRecordsAsync(tableName, cutoffTime, migratedCount, cancellationToken);
                        //}

                        (var migratedCount, var deletedCount) = await MigrateTableWithReaderAsync(tableName, cutoffTime, countToMigrate, cancellationToken);

                        result.Success = true;
                        result.MigratedCount = migratedCount;
                        result.DeletedCount = deletedCount;
                        result.Message = $"迁移成功: 共迁移 {migratedCount} 条，删除 {deletedCount} 条";
                    }
                    catch (OperationCanceledException)
                    {
                        // ✅ 用户主动取消
                        throw new OperationCanceledException($"任务被取消,当前表: {tableName} 迁移数量: {currentMigratedCount}");
                    }
                    catch (Exception ex)
                    {
                        result.Success = false;
                        result.Message = $"迁移失败: {ex.Message}";
                        // 触发【迁移异常】事件（关联当前表名）
                        MigrationFailed?.Invoke(this, (Ex: ex, TableName: tableName));
                    }
                    finally
                    {
                        stopwatch.Stop();
                        result.ElapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                        // 触发【单个表迁移完成】事件
                        TableMigrationCompleted?.Invoke(this, result);
                        // 重置累计计数，准备下一张表迁移
                        currentMigratedCount = 0;
                    }
                    results.Add(result);
                }
                // 触发【整体迁移完成】事件
                MigrationCompleted?.Invoke(this, results);
            }
            catch (OperationCanceledException ex)
            {
                // ✅ 用户主动取消
                MigrationFailed?.Invoke(this, (Ex: ex, TableName: null));
            }
            catch (Exception ex)
            {
                // 触发全局异常事件（无关联表名）
                MigrationFailed?.Invoke(this, (Ex: ex, TableName: null));
                //throw; // 若需上层捕获，可保留；若仅靠事件处理，可移除
            }
            return results;
        }

        #region Migrate table with reader.
        /// <summary>
        /// 使用 MySqlDataReader 和 MySqlBulkCopy 批量迁移数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="cutoffTime"></param>
        /// <param name="totalCount"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<(long totalMigrated, long totalDeleted)> MigrateTableWithReaderAsync(
            string tableName,
            DateTime cutoffTime,
            long totalCount, CancellationToken cancellationToken)
        {
            long totalMigrated = 0;
            long totalDeleted = 0;

            if (totalCount <= 0) return (totalMigrated, totalDeleted);
            var batchCount = (int)Math.Ceiling((double)totalCount / _config.BatchSize);

            for (int batchIndex = 0; batchIndex < batchCount; batchIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 1. 打开源库连接
                using var sourceConn = new MySqlConnection(_config.SourceConnectionString);
                await sourceConn.OpenAsync(cancellationToken);

                var sql = $@"SELECT * FROM `{tableName}` 
                         WHERE `{_config.TimeColumnName}` < @CutoffTime 
                         ORDER BY `{_config.TimeColumnName}` ASC 
                         LIMIT @Offset, @BatchSize";

                using var cmd = new MySqlCommand(sql, sourceConn);
                cmd.Parameters.AddWithValue("@CutoffTime", cutoffTime);
                cmd.Parameters.AddWithValue("@Offset", batchIndex * _config.BatchSize);
                cmd.Parameters.AddWithValue("@BatchSize", _config.BatchSize);

                using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

                if (!reader.HasRows)
                {
                    break;
                }

                // 2. 打开目标库连接
                using var destConn = new MySqlConnection(_config.BackupConnectionString);
                await destConn.OpenAsync(cancellationToken);

                var bulkCopy = new MySqlBulkCopy(destConn)
                {
                    DestinationTableName = tableName,
                    BulkCopyTimeout = 0,
                    NotifyAfter = 10000
                };
                bulkCopy.MySqlRowsCopied += (sender, e) =>
                {
                    // 可选 to do ：触发进度事件
                    //BulkCopyProgress?.Invoke(this, (TableName: tableName, RowsCopied: e.RowsCopied));
                };
                // 3. 列映射一次构建即可
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping
                    {
                        SourceOrdinal = i,
                        DestinationColumn = reader.GetName(i)
                    });
                }

                // 4. 执行批量写入
                var result = await bulkCopy.WriteToServerAsync(reader, cancellationToken);
                int rowsCopied = result?.RowsInserted ?? 0;

                // 5. 删除源数据（按 batch）
                if (_config.DeleteAfterMigration && rowsCopied > 0)
                {
                    long deleted = await DeleteMigratedRecordsAsync(tableName, cutoffTime, rowsCopied, cancellationToken);
                    totalDeleted += deleted;
                }

                totalMigrated += rowsCopied;
                currentMigratedCount += rowsCopied;
                // 触发【批量迁移进度】事件（告知当前批次进度）
                BatchMigrationProgress?.Invoke(this, (
                    TableName: tableName,
                    CurrentBatch: batchIndex + 1, // 批次从1开始显示
                    TotalBatches: batchCount,
                    MigratedSoFar: totalMigrated,
                    TotalToMigrate: totalCount
                ));
            }

            return (totalMigrated, totalDeleted);
        }
        #endregion
        /// <summary>
        /// 批量迁移记录
        /// </summary>
        private async Task<long> MigrateRecordsAsync(string tableName, DateTime cutoffTime, long totalCount, CancellationToken cancellationToken)
        {
            if (totalCount <= 0) return 0;

            long migrated = 0;
            var batchCount = (int)Math.Ceiling((double)totalCount / _config.BatchSize);

            for (int batch = 0; batch < batchCount; batch++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 1. 读取源库数据
                var dataTable = await GetBatchDataAsync(tableName, cutoffTime, batch);
                if (dataTable.Rows.Count == 0)
                    break;

                // 2. 插入到备份库
                var inserted = await UnifiedInsertToBackupAsync(tableName, dataTable, cancellationToken);
                //if (inserted != dataTable.Rows.Count)
                //{
                //    throw new Exception($"第 {batch + 1} 批数据迁移不完整，预期 {dataTable.Rows.Count} 条，实际插入 {inserted} 条");
                //}

                migrated += inserted;
                currentMigratedCount += inserted;
                // 触发【批量迁移进度】事件（告知当前批次进度）
                BatchMigrationProgress?.Invoke(this, (
                    TableName: tableName,
                    CurrentBatch: batch + 1, // 批次从1开始显示
                    TotalBatches: batchCount,
                    MigratedSoFar: migrated,
                    TotalToMigrate: totalCount
                ));

                // 释放DataTable内存
                dataTable.Clear();
                dataTable.Dispose();
            }

            return migrated;
        }
        /// <summary>
        /// 从源库获取一批待迁移数据
        /// </summary>
        private async Task<DataTable> GetBatchDataAsync(string tableName, DateTime cutoffTime, int batchIndex)
        {
            var sql = $@"SELECT * FROM `{tableName}` 
                         WHERE `{_config.TimeColumnName}` < @CutoffTime 
                         ORDER BY `{_config.TimeColumnName}` ASC 
                         LIMIT @Offset, @BatchSize";

            using var connection = new MySqlConnection(_config.SourceConnectionString);
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@CutoffTime", cutoffTime);
            command.Parameters.AddWithValue("@Offset", batchIndex * _config.BatchSize);
            command.Parameters.AddWithValue("@BatchSize", _config.BatchSize);

            using var adapter = new MySqlDataAdapter(command);
            var dataTable = new DataTable();

            await connection.OpenAsync();
            await Task.Run(() => adapter.Fill(dataTable)); // MySqlDataAdapter.Fill没有异步方法

            return dataTable;
        }
        /// <summary>
        /// 检查备份库表结构是否与源库一致
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> CheckTableStructureAsync(string tableName)
        {
            // 获取源库表结构哈希
            var sourceHash = await GetTableStructureHashAsync(tableName, _config.SourceConnectionString!);
            // 获取备份库表结构哈希
            var backupHash = await GetTableStructureHashAsync(tableName, _config.BackupConnectionString!);

            return string.Equals(sourceHash, backupHash, StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// 在备份库创建与源库相同的表结构
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> CreateTableInBackupAsync(string tableName)
        {
            try
            {
                // get CREATE statement from source
                var createSql = await GetTableCreateStatementAsync(tableName);
                if (string.IsNullOrWhiteSpace(createSql))
                    return false;

                // normalize simple name (strip schema if provided)
                var tableOnly = GetSimpleTableName(tableName);

                using var conn = new MySqlConnection(_config.BackupConnectionString);
                await conn.OpenAsync();

                // check if table exists in backup DB
                var exists = await TableExistsInDatabaseAsync(conn, tableOnly);
                if (exists)
                {
                    // drop safely; disable FK checks to avoid FK constraint blocking drops
                    using (var cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", conn))
                        await cmd.ExecuteNonQueryAsync();

                    using (var cmd = new MySqlCommand($"DROP TABLE {QuoteIdentifier(tableOnly)};", conn))
                        await cmd.ExecuteNonQueryAsync();

                    using (var cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 1;", conn))
                        await cmd.ExecuteNonQueryAsync();
                }

                // sanitize and adapt create statement:
                //  - remove DEFINER (can cause permission errors)
                //  - remove AUTO_INCREMENT value
                //  - ensure CREATE TABLE uses the target table name (no schema prefix)
                var sanitized = RemoveDefiner(createSql);
                sanitized = RemoveAutoIncrementValue(sanitized);
                sanitized = NormalizeCreateTableHeaderToTarget(sanitized, tableOnly);

                // finally create in backup
                using var createCmd = new MySqlCommand(sanitized, conn);
                await createCmd.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception ex)
            {
                MigrationFailed?.Invoke(this, (Ex: ex, TableName: tableName));
                throw; // 若需上层捕获，可保留；若仅靠事件处理，可移除
            }
        }

        private async Task<string?> GetTableCreateStatementAsync(string tableName)
        {
            // SHOW CREATE TABLE does not accept parameters for identifiers,
            // so we must safely quote the identifier.
            var quoted = QuoteFullIdentifier(tableName);

            var sql = $"SHOW CREATE TABLE {quoted};";

            using var conn = new MySqlConnection(_config.SourceConnectionString);
            using var cmd = new MySqlCommand(sql, conn);
            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                // second column (index 1) is the CREATE statement
                var createStmt = reader.GetString(1);
                return createStmt;
            }

            return null;
        }

        private static async Task<bool> TableExistsInDatabaseAsync(MySqlConnection conn, string tableOnly)
        {
            // conn.Database is the current DB name from connection string
            var sql = @"SELECT COUNT(*) FROM information_schema.tables 
                    WHERE table_schema = @db AND table_name = @table";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@db", conn.Database);
            cmd.Parameters.AddWithValue("@table", tableOnly);
            var cnt = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return cnt > 0;
        }

        // --- helpers ---

        private static string GetSimpleTableName(string fullName)
        {
            // accept "schema.table" or "table"; returns last segment
            if (string.IsNullOrEmpty(fullName)) return fullName ?? "";
            var parts = fullName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            return parts[^1];
        }

        private static string QuoteIdentifier(string identifier)
        {
            if (identifier is null) throw new ArgumentNullException(nameof(identifier));
            // double any backticks inside name and wrap with backticks
            return $"`{identifier.Replace("`", "``")}`";
        }

        private static string QuoteFullIdentifier(string full)
        {
            if (string.IsNullOrEmpty(full)) throw new ArgumentNullException(nameof(full));
            var parts = full.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
                parts[i] = QuoteIdentifier(parts[i]);
            return string.Join(".", parts);
        }

        private static string RemoveAutoIncrementValue(string sql)
        {
            if (string.IsNullOrEmpty(sql)) return sql;
            // remove AUTO_INCREMENT=<digits> with optional surrounding spaces
            sql = Regex.Replace(sql, @"AUTO_INCREMENT\s*=\s*\d+\s*", "", RegexOptions.IgnoreCase);
            return sql;
        }

        private static string RemoveDefiner(string sql)
        {
            if (string.IsNullOrEmpty(sql)) return sql;
            // remove DEFINER=`user`@`host` (if present)
            sql = Regex.Replace(sql, @"DEFINER\s*=\s*`[^`]+`@`[^`]+`", "", RegexOptions.IgnoreCase);
            // remove leftover double spaces
            sql = Regex.Replace(sql, @"\s{2,}", " ");
            return sql;
        }

        private static string NormalizeCreateTableHeaderToTarget(string createSql, string targetTableOnly)
        {
            if (string.IsNullOrEmpty(createSql)) return createSql;
            // Replace "CREATE TABLE [`db`.]`oldname`" with "CREATE TABLE `targetTable`"
            string pattern = @"^(\s*CREATE\s+TABLE\s+)(?:`[^`]+`\.)?`([^`]+)`";
            var result = Regex.Replace(createSql, pattern,
                m => m.Groups[1].Value + QuoteIdentifier(targetTableOnly),
                RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return result;
        }
        /// <summary>
        /// 获取表结构的哈希值
        /// 为了比较表结构是否一致，我们通过获取表的列信息并生成一个哈希值来进行比较
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private async Task<string?> GetTableStructureHashAsync(string tableName, string connectionString)
        {
            var sql = $@"SELECT CONCAT(COALESCE(COLUMN_NAME, ''),
                               COALESCE(DATA_TYPE, ''),
                               COALESCE(IS_NULLABLE, ''),
                               COALESCE(COLUMN_DEFAULT, '')) 
                 FROM INFORMATION_SCHEMA.COLUMNS 
                 WHERE TABLE_NAME = @TableName
                 AND TABLE_SCHEMA = DATABASE() -- 只查询当前数据库
                 ORDER BY ORDINAL_POSITION";

            using var connection = new MySqlConnection(connectionString);
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@TableName", tableName);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            var structureBuilder = new StringBuilder();
            while (await reader.ReadAsync())
            {
                // 双重检查：先检查是否为NULL，再获取值
                if (!reader.IsDBNull(0))
                {
                    var tempValue = reader.GetString(0);
                    structureBuilder.Append(tempValue);
                }
            }

            return structureBuilder.Length > 0
                ? structureBuilder.ToString().GetHashCode().ToString()
                : null;
        }
        ///// <summary>
        ///// 获取表创建语句
        ///// </summary>
        //private async Task<string?> GetTableCreateStatementAsync(string tableName)
        //{
        //    var sql = "SHOW CREATE TABLE @TableName";

        //    using var connection = new MySqlConnection(_config.SourceConnectionString);
        //    using var command = new MySqlCommand(sql, connection);
        //    command.Parameters.AddWithValue("@TableName", tableName);

        //    await connection.OpenAsync();
        //    using var reader = await command.ExecuteReaderAsync();

        //    if (await reader.ReadAsync())
        //    {
        //        return RemoveAutoIncrementValue(reader.GetString(1)); // 第二列是创建语句
        //    }

        //    return null;
        //}
        ///// <summary>
        ///// 从 CREATE TABLE 语句中移除 AUTO_INCREMENT 值
        ///// </summary>
        ///// <param name="createTableSql">原始 CREATE TABLE 语句</param>
        ///// <returns>移除 AUTO_INCREMENT 值后的 CREATE TABLE 语句</returns>
        //private string RemoveAutoIncrementValue(string createTableSql)
        //{
        //    // 使用正则表达式匹配并移除 AUTO_INCREMENT 值
        //    var regex = new Regex(@"AUTO_INCREMENT=\d+\s*");
        //    return regex.Replace(createTableSql, string.Empty);
        //}
        /// <summary>
        /// 获取需要迁移的记录数量
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="cutoffTime"></param>
        /// <returns></returns>
        private async Task<long> GetRecordCountToMigrateAsync(string tableName, DateTime cutoffTime)
        {
            var sql = $@"SELECT COUNT(*) FROM {tableName} 
                         WHERE `{_config.TimeColumnName}` < @CutoffTime"; // 假设有 CreatedAt 字段
            using var connection = new MySqlConnection(_config.SourceConnectionString);
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@CutoffTime", cutoffTime);
            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt64(result) : 0;
        }

        /// <summary>
        /// 统一插入入口：小数据量用原有参数化插入，大数据量用CSV BulkLoad
        /// </summary>
        private async Task<int> UnifiedInsertToBackupAsync(
            string tableName,
            DataTable dataTable,
            CancellationToken cancellationToken)
        {
            // 关键：设置阈值（建议先测试，推荐1万~5万条，根据字段多少调整）
            const int BULK_LOAD_THRESHOLD = 10000;

            if (dataTable.Rows.Count < BULK_LOAD_THRESHOLD)
            {
                // 小数据量：走原有参数化批量插入（无需CSV overhead）
                return await InsertToBackupAsync(tableName, dataTable, cancellationToken);
            }
            else
            {
                // 大数据量：MySqlBulkCopy（解决性能瓶颈）
                return await BulkCopyToBackupAsync(tableName, dataTable, cancellationToken);
            }
        }
        /// <summary>
        /// 批量插入到备份库（一次性插入多行，去掉 IGNORE）
        /// </summary>
        private async Task<int> InsertToBackupAsync(
            string tableName,
            DataTable dataTable,
            CancellationToken cancellationToken)
        {
            if (dataTable.Rows.Count == 0)
                return 0;

            cancellationToken.ThrowIfCancellationRequested();

            var columns = string.Join(", ", dataTable.Columns.Cast<DataColumn>().Select(c => $"`{c.ColumnName}`"));

            using var connection = new MySqlConnection(_config.BackupConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            using var command = connection.CreateCommand();
            command.Transaction = transaction;

            int inserted = 0;

            try
            {
                // 每次批量处理 1000 行（可根据 max_allowed_packet 调整）
                const int batchSize = 1000;
                for (int batchStart = 0; batchStart < dataTable.Rows.Count; batchStart += batchSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var batchRows = dataTable.AsEnumerable()
                                             .Skip(batchStart)
                                             .Take(batchSize)
                                             .ToList();

                    // 构造 VALUES (@p0_0, @p1_0 ...), (@p0_1, @p1_1 ...)
                    var values = new List<string>();
                    command.Parameters.Clear();

                    for (int rowIndex = 0; rowIndex < batchRows.Count; rowIndex++)
                    {
                        var paramNames = new List<string>();
                        for (int colIndex = 0; colIndex < dataTable.Columns.Count; colIndex++)
                        {
                            string paramName = $"@p{colIndex}_{rowIndex}";
                            paramNames.Add(paramName);
                            command.Parameters.Add(new MySqlParameter(paramName, GetMySqlDbType(dataTable.Columns[colIndex].DataType))
                            {
                                Value = batchRows[rowIndex][colIndex] ?? DBNull.Value
                            });
                        }
                        values.Add("(" + string.Join(", ", paramNames) + ")");
                    }

                    command.CommandText = $"INSERT INTO `{tableName}` ({columns}) VALUES {string.Join(", ", values)}";

                    inserted += await command.ExecuteNonQueryAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

            return inserted;
        }
        /// <summary>
        /// 大数据量专用：CSV + MySqlBulkLoader 插入（修正 NULL 处理、BOM、换行）
        /// </summary>
        private async Task<int> CsvBulkLoadToBackupAsync(
            string tableName,
            DataTable dataTable,
            CancellationToken cancellationToken)
        {
            if (dataTable.Rows.Count == 0)
                return 0;

            cancellationToken.ThrowIfCancellationRequested();

            // 使用无 BOM 的 UTF8（避免首行被污染），并强制 NewLine 与 LineTerminator 一致
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream, new UTF8Encoding(false), leaveOpen: true)
            {
                NewLine = "\n"
            };

            foreach (DataRow row in dataTable.Rows)
            {
                var parts = new string[row.ItemArray.Length];
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    var field = row.ItemArray[i];
                    if (field == null || field == DBNull.Value)
                    {
                        // 不带引号的 \N 会被 MySQL LOAD DATA 识别为 NULL
                        parts[i] = "\\N";
                    }
                    else
                    {
                        var s = field.ToString() ?? "";
                        // 标准 CSV 双引号转义
                        s = s.Replace("\"", "\"\"");
                        parts[i] = $"\"{s}\"";
                    }
                }

                streamWriter.WriteLine(string.Join(",", parts));
            }

            await streamWriter.FlushAsync();
            memoryStream.Position = 0;

            using var connection = new MySqlConnection(_config.BackupConnectionString);
            await connection.OpenAsync(cancellationToken);

            var bulkLoader = new MySqlBulkLoader(connection)
            {
                TableName = tableName,
                SourceStream = memoryStream,
                FieldTerminator = ",",
                LineTerminator = "\n",          // 与 streamWriter.NewLine 保持一致
                FieldQuotationCharacter = '"',
                EscapeCharacter = '\\',         // 更标准的转义字符
                NumberOfLinesToSkip = 0,
                Local = true,
                ConflictOption = MySqlBulkLoaderConflictOption.None
            };

            // 如需显式指定列顺序（推荐当 DataTable 列顺序与目标表不一致时）
            // foreach (DataColumn col in dataTable.Columns) bulkLoader.Columns.Add(col.ColumnName);

            return await bulkLoader.LoadAsync(cancellationToken);
        }
        private async Task<int> BulkCopyToBackupAsync(
            string tableName,
            DataTable dataTable,
            CancellationToken cancellationToken)
        {
            if (dataTable.Rows.Count == 0)
                return 0;

            cancellationToken.ThrowIfCancellationRequested();

            using var connection = new MySqlConnection(_config.BackupConnectionString);
            await connection.OpenAsync(cancellationToken);

            var bulkCopy = new MySqlBulkCopy(connection)
            {
                DestinationTableName = tableName,
                BulkCopyTimeout = 0,     // 不限时（按需改）
                NotifyAfter = 10000      // 每插入 1w 行触发事件，可做进度条
            };
            bulkCopy.MySqlRowsCopied += (sender, e) =>
            {
                // 可选 to do ：触发进度事件
                //BulkCopyProgress?.Invoke(this, (TableName: tableName, RowsCopied: e.RowsCopied));
            };

            // 映射 DataTable 列到目标表列
            foreach (DataColumn col in dataTable.Columns)
            {
                bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping
                {
                    SourceOrdinal = col.Ordinal,
                    DestinationColumn = col.ColumnName
                });
            }

            // 执行批量写入
            var result = await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
            return result?.RowsInserted ?? 0;
        }

        /// <summary>
        /// 转换数据类型
        /// </summary>
        private MySqlDbType GetMySqlDbType(Type type)
        {
            if (type == typeof(int)) return MySqlDbType.Int32;
            if (type == typeof(long)) return MySqlDbType.Int64;
            if (type == typeof(decimal)) return MySqlDbType.Decimal;
            if (type == typeof(DateTime)) return MySqlDbType.DateTime;
            if (type == typeof(bool)) return MySqlDbType.Bit;
            if (type == typeof(byte[])) return MySqlDbType.Blob;

            return MySqlDbType.VarChar;
        }
        /// <summary>
        /// 删除已迁移的记录
        /// </summary>
        private async Task<long> DeleteMigratedRecordsAsync(string tableName, DateTime cutoffTime, long limit, CancellationToken cancellationToken)
        {

            long totalDeleted = 0;
            // 分批次删除，防止锁表，一次最多10000条
            int batchSize = 10000;

            using var connection = new MySqlConnection(_config.SourceConnectionString);
            await connection.OpenAsync();

            while (totalDeleted < limit)
            {
                cancellationToken.ThrowIfCancellationRequested();

                long remaining = limit - totalDeleted;
                int currentBatch = (int)Math.Min(batchSize, remaining);
                var sql = $@"DELETE FROM `{tableName}` 
                                     WHERE `{_config.TimeColumnName}` < @CutoffTime 
                                     LIMIT @Limit"; // 使用LIMIT防止一次删除过多数据
                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@CutoffTime", cutoffTime);
                command.Parameters.AddWithValue("@Limit", currentBatch); // 最多删除当前批次应该删除的数量

                int deleted = await command.ExecuteNonQueryAsync(cancellationToken);
                if (deleted == 0)
                    break; // 没有更多数据可删，提前退出

                totalDeleted += deleted;
            }
            return totalDeleted;
        }
    }
}
