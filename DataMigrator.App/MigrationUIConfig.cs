namespace DataMigrator.App
{
    /// <summary>
    /// 迁移UI配置
    /// </summary>
    public class MigrationUIConfig
    {
        /// <summary>
        /// 源数据库连接信息
        /// </summary>
        public string? SourceServer { get; set; }
        /// <summary>
        /// 源数据库名称
        /// </summary>
        public string? SourceDatabase { get; set; }
        /// <summary>
        /// 源数据库用户名
        /// </summary>
        public string? SourceUserId { get; set; }
        /// <summary>
        /// 源数据库密码
        /// </summary>
        public string? SourcePassword { get; set; }
        /// <summary>
        /// 备份数据库连接信息
        /// </summary>
        public string? BackupServer { get; set; }
        /// <summary>
        /// 备份数据库名称
        /// </summary>
        public string? BackupDatabase { get; set; }
        /// <summary>
        /// 备份数据库用户名
        /// </summary>
        public string? BackupUserId { get; set; }
        /// <summary>
        /// 备份数据库密码
        /// </summary>
        public string? BackupPassword { get; set; }
        /// <summary>
        /// 保存时间
        /// </summary>
        public int KeepDays { get; set; }
        /// <summary>
        /// 批处理大小
        /// </summary>
        public int BatchSize { get; set; }
        /// <summary>
        /// 迁移后删除源数据
        /// </summary>
        public bool DeleteAfterMigration { get; set; }
        /// <summary>
        /// 选择的表名列表
        /// </summary>
        public List<string>? SelectedTables { get; set; }
    }
}
