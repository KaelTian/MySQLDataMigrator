namespace DataMigrator.Service.Models
{
    /// <summary>
    /// 迁移配置
    /// </summary>
    public class MigrationConfig
    {
        /// <summary>
        /// 源数据库连接字符串
        /// </summary>
        public string? SourceConnectionString { get; set; }

        /// <summary>
        /// 备份数据库连接字符串
        /// </summary>
        public string? BackupConnectionString { get; set; }

        /// <summary>
        /// 需要迁移的表名列表
        /// </summary>
        public List<string> TableNames { get; set; } = new List<string>();

        /// <summary>
        /// 时间字段名（每张表用于排序的时间列，通常是创建时间）
        /// </summary>
        public string TimeColumnName { get; set; } = "CreateTime";

        /// <summary>
        /// 保留最近N天的数据在源库
        /// </summary>
        public int KeepRecentDays { get; set; } = 30; // 默认保留最近30天

        /// <summary>
        /// 批量处理大小
        /// </summary>
        public int BatchSize { get; set; } = 1000;

        /// <summary>
        /// 是否在迁移后删除源库数据
        /// </summary>
        public bool DeleteAfterMigration { get; set; } = true;

        /// <summary>
        /// 迁移任务执行超时时间(秒)
        /// </summary>
        public int TimeoutSeconds { get; set; } = 3600;
    }
}
