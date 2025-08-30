namespace DataMigrator.Service.Models
{
    /// <summary>
    /// 表迁移结果
    /// </summary>
    public class TableMigrationResult
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string? TableName { get; set; }
        /// <summary>
        /// 迁移的记录数
        /// </summary>
        public long MigratedCount { get; set; }
        /// <summary>
        /// 删除的记录数
        /// </summary>
        public long DeletedCount { get; set; }
        /// <summary>
        /// 迁移是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 错误或状态信息
        /// </summary>
        public string? Message { get; set; }
        /// <summary>
        /// 耗时(秒)
        /// </summary>
        public double ElapsedSeconds { get; set; }
    }
}
