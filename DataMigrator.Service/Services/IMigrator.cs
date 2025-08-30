using DataMigrator.Service.Models;

namespace DataMigrator.Service.Services
{
    /// <summary>
    /// 数据迁移服务接口
    /// </summary>
    public interface IMigrator
    {
        /// <summary>
        /// 执行迁移任务
        /// </summary>
        /// <returns></returns>
        Task<List<TableMigrationResult>> ExecuteMigrationAsync(CancellationToken cancellationToken);
        /// <summary>
        /// 检查备份数据库表结构是否与源数据库一致
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        Task<bool> CheckTableStructureAsync(string tableName);
        /// <summary>
        /// 在备份数据库中创建表
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        Task<bool> CreateTableInBackupAsync(string tableName);
    }
}
