using MySqlConnector;
using MySQLDataMigrator.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MySQLDataMigrator.Services
{
    /// <summary>
    /// MySQL数据库迁移服务
    /// </summary>
    public class MySqlMigratorConsole
    {
        /// <summary>
        /// 数据库迁移服务配置
        /// </summary>
        private readonly MySqlMigrationConfig _config;

        public MySqlMigratorConsole(MySqlMigrationConfig config)
        {
            _config = config;
        }
        /// <summary>
        /// 执行数据迁移
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteMigrationAsync()
        {
            foreach (var tableName in _config.TableNames)
            {
                try
                {
                    Console.WriteLine($"开始迁移表: {tableName}......");

                    // 1.检查是否需要迁移
                    var recordCount = await GetTableRecordCountAsync(tableName);
                    Console.WriteLine($"表 {tableName} 当前记录数: {recordCount}");

                    if (recordCount < _config.Threshold)
                    {
                        Console.WriteLine($"表 {tableName} 记录数未超过阈值 {_config.Threshold}，跳过迁移。");
                        continue;
                    }

                    // 2.计算迁移数量
                    var recordsToMigrate = (long)(recordCount * _config.MigrationRatio);
                    Console.WriteLine($"表 {tableName} 计划迁移记录数: {recordsToMigrate}");

                    // 3.迁移数据
                    var migratedCount = await MigrateRecordsAsync(tableName, recordsToMigrate);
                    Console.WriteLine($"表 {tableName} 迁移完成,成功迁移 {migratedCount} 条记录");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"表 {tableName} 迁移失败: {ex.Message}");
                    // 记录详细错误日志
                }
            }
        }
        /// <summary>
        /// 获取表的记录数
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private async Task<long> GetTableRecordCountAsync(string tableName)
        {
            using (var connection = new MySqlConnection(_config.SourceConnectionString))
            {
                await connection.OpenAsync();
                var command = new MySqlCommand($"SELECT COUNT(*) FROM {tableName}", connection);
                var result = await command.ExecuteScalarAsync();
                return result != null ? (long)result : 0;
            }
        }
        /// <summary>
        /// 迁移指定数量的记录
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="recordsToMigrate"></param>
        /// <returns></returns>
        private async Task<long> MigrateRecordsAsync(string tableName, long recordsToMigrate)
        {
            // 1. 确保备份表存在
            await EnsureBackupTableExistsAsync(tableName);

            // 记录迁移总数
            long totalMigrated = 0;
            // 计算批次数
            int batches = (int)Math.Ceiling((double)recordsToMigrate / _config.BatchSize);

            for (int i = 0; i < batches; i++)
            {
                // 每批处理需要开启事务，确保数据一致性
                using (var sourceConnection = new MySqlConnection(_config.SourceConnectionString))
                {
                    await sourceConnection.OpenAsync();
                    using (var transaction = await sourceConnection.BeginTransactionAsync())
                    {
                        try
                        {
                            // 2. 查询需要迁移的记录
                            var records = await GetRecordsToMigrateAsync(
                                sourceConnection, transaction, tableName,
                                i * _config.BatchSize, _config.BatchSize);

                            if (records.Count == 0)
                            {
                                break; // 没有更多记录可迁移
                            }

                            // 3. 插入到备份表
                            await InsertToBackupTableAsync(tableName, records);

                            // 4. 删除源表中的记录
                            await DeleteMigratedRecordsAsync(sourceConnection, transaction, tableName, records);

                            // 5. 提交事务
                            await transaction.CommitAsync();

                            totalMigrated += records.Count;
                            Console.WriteLine($"{DateTime.Now} -- 已迁移 {totalMigrated}/{recordsToMigrate} 条记录");
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            Console.WriteLine($"批次 {i + 1} 迁移失败: {ex.Message}");
                            throw;
                        }
                    }
                }
            }

            return totalMigrated;
        }
        /// <summary>
        /// 获取需要迁移的记录
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="tableName"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        private async Task<List<Dictionary<string, object>>> GetRecordsToMigrateAsync(
            MySqlConnection connection, MySqlTransaction transaction,
            string tableName, int offset, int limit)
        {
            var records = new List<Dictionary<string, object>>();

            var command = new MySqlCommand(
                $"SELECT * FROM {tableName} ORDER BY {_config.OrderByField} LIMIT @offset, @limit",
                connection, transaction);

            command.Parameters.AddWithValue("@offset", offset);
            command.Parameters.AddWithValue("@limit", limit);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var record = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        record[reader.GetName(i)] = reader.GetValue(i);
                    }
                    records.Add(record);
                }
            }

            return records;
        }
        /// <summary>
        /// 插入记录到备份表
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="records"></param>
        /// <returns></returns>
        private async Task InsertToBackupTableAsync(string tableName, List<Dictionary<string, object>> records)
        {
            if (records == null || records.Count == 0)
            {
                return;
            }
            using (var connection = new MySqlConnection(_config.BackupConnectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (var record in records)
                        {
                            var columns = string.Join(", ", record.Keys);
                            var parameters = string.Join(", ", record.Keys.Select(k => "@" + k));
                            var command = new MySqlCommand(
                                $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})",
                                connection, transaction);
                            foreach (var kvp in record)
                            {
                                command.Parameters.AddWithValue("@" + kvp.Key, kvp.Value);
                            }
                            await command.ExecuteNonQueryAsync();
                        }
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine($"插入备份表 {tableName} 失败: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        private async Task DeleteMigratedRecordsAsync(
            MySqlConnection connection, MySqlTransaction transaction,
            string tableName, List<Dictionary<string, object>> records)
        {
            if (records == null || records.Count == 0)
            {
                return;
            }

            try
            {
                const int batchSize = 500;
                var allIds = records
                    .Select(r => r["ID"]?.ToString())
                    .Where(id => !string.IsNullOrEmpty(id))
                    .ToList();

                if (allIds.Count != records.Count)
                {
                    throw new Exception("部分记录中缺少主键字段 ID，无法删除。");
                }
                // 分批删除
                for (int i = 0; i < allIds.Count; i += batchSize)
                {
                    var batchIds = allIds.Skip(i).Take(batchSize).ToList();

                    // 使用JSON_TABLE方式处理大量参数（MySQL 8.0+）
                    var command = new MySqlCommand(
                        $"DELETE FROM {tableName} WHERE ID IN (SELECT value FROM JSON_TABLE(@ids, '$[*]' COLUMNS(value VARCHAR(50) PATH '$')) AS j)",
                        connection, transaction);

                    command.Parameters.AddWithValue("@ids", JsonSerializer.Serialize(batchIds));

                    await command.ExecuteNonQueryAsync();
                }
                #region 另一种 分批删除
                //var idLists = records
                //    .Where(r => r.ContainsKey("ID"))
                //    .Select(r => r["ID"])
                //    .ToList();

                //// 如果没有有效的ID，抛出异常
                //if (idLists.Count != records.Count)
                //{
                //    throw new Exception("部分记录中缺少主键字段 ID，无法删除。");
                //}

                //// 分批处理
                //for (int i = 0; i < idLists.Count; i += batchSize)
                //{
                //    var batchIds = idLists.Skip(i).Take(batchSize).ToList();

                //    // 构建参数化的IN查询
                //    var parameters = new List<string>();
                //    var command = new MySqlCommand("", connection, transaction);

                //    for (int j = 0; j < batchIds.Count; j++)
                //    {
                //        var paramName = $"@id{j}";
                //        parameters.Add(paramName);
                //        command.Parameters.AddWithValue(paramName, batchIds[j]);
                //    }

                //    var inClause = string.Join(",", parameters);
                //    command.CommandText = $"DELETE FROM {tableName} WHERE ID IN ({inClause})";

                //    await command.ExecuteNonQueryAsync();
                //}
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除源表 {tableName} 记录失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 确保备份表存在(结构与源表一致)
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task EnsureBackupTableExistsAsync(string tableName)
        {
            // 1. 获取源表结构
            string createTableSql;
            using (var sourceConnection = new MySqlConnection(_config.SourceConnectionString))
            {
                await sourceConnection.OpenAsync();
                var getTableCmd = new MySqlCommand($"SHOW CREATE TABLE {tableName}", sourceConnection);
                using (var reader = await getTableCmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        createTableSql = reader.GetString(1);

                        // 移除 AUTO_INCREMENT 值，确保新表从 1 开始
                        createTableSql = RemoveAutoIncrementValue(createTableSql);
                    }
                    else
                    {
                        throw new Exception($"无法获取表 {tableName} 的创建语句。");
                    }
                }
            }

            // 2. 在备份数据库中创建表
            using (var backupConnection = new MySqlConnection(_config.BackupConnectionString))
            {
                await backupConnection.OpenAsync();
                var checkTableCmd = new MySqlCommand($"SHOW TABLES LIKE '{tableName}'", backupConnection);
                var exists = await checkTableCmd.ExecuteScalarAsync();
                if (exists == null)
                {
                    var createTableCmd = new MySqlCommand(createTableSql, backupConnection);
                    await createTableCmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"备份数据库中创建表 {tableName} 成功。");
                }
                else
                {
                    Console.WriteLine($"备份数据库中表 {tableName} 已存在，跳过创建。");
                }
            }
        }

        /// <summary>
        /// 从 CREATE TABLE 语句中移除 AUTO_INCREMENT 值
        /// </summary>
        /// <param name="createTableSql">原始 CREATE TABLE 语句</param>
        /// <returns>移除 AUTO_INCREMENT 值后的 CREATE TABLE 语句</returns>
        private string RemoveAutoIncrementValue(string createTableSql)
        {
            // 使用正则表达式匹配并移除 AUTO_INCREMENT 值
            var regex = new Regex(@"AUTO_INCREMENT=\d+\s*");
            return regex.Replace(createTableSql, string.Empty);
        }
    }
}
