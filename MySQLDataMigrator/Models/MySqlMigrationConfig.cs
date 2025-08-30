namespace MySQLDataMigrator.Models
{
    /// <summary>
    /// 数据库迁移配置
    /// </summary>
    public class MySqlMigrationConfig
    {
        /// <summary>
        /// 主数据库连接字符串
        /// </summary>
        public string? SourceConnectionString { get; set; }
        /// <summary>
        /// 备份数据库连接字符串
        /// </summary>
        public string? BackupConnectionString { get; set; }
        /// <summary>
        /// 迁移的表名列表
        /// </summary>
        public List<string> TableNames { get; set; } = new List<string>();
        /// <summary>
        /// 触发迁移的阈值(记录数)
        /// </summary>
        public long Threshold { get; set; } = 5000000; // 默认500万
        /// <summary>
        /// 每次迁移的比例
        /// </summary>
        public double MigrationRatio { get; set; } = 0.5; // 默认50%
        /// <summary>
        /// 批量迁移的记录数
        /// </summary>
        public int BatchSize { get; set; } = 2000; // 每次迁移的批次大小，默认2000
        /// <summary>
        /// 排序字段(通常是时间字段)
        /// </summary>
        public string OrderByField { get; set; } = "CreateTime";
    }
}
