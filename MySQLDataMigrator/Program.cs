using DataMigrator.Service.Models;
using DataMigrator.Service.Services;
using MySQLDataMigrator.Models;
using MySQLDataMigrator.Services;

//MySqlMigrationConfig migrationConfig = new MySqlMigrationConfig
//{
//    SourceConnectionString = "server=192.168.0.189;user id=root;password=root;database=005_mes;charset=utf8;sslMode=None;pooling=true;minpoolsize=1;maxpoolsize=1024;ConnectionLifetime=30;DefaultCommandTimeout=600;",
//    BackupConnectionString = "server=192.168.0.122;user id=root;password=root;database=005_mes_backup;charset=utf8;sslMode=None;pooling=true;minpoolsize=1;maxpoolsize=1024;ConnectionLifetime=30;DefaultCommandTimeout=600;",
//    TableNames = new List<string>
//      {
//          //"jj_alarm_records",
//          //"jj_plc_alarm",
//          //"xyh_alarm_records"
//          "xyh_plc_loading_and_unloading"
//      },
//    Threshold = 50000,
//    MigrationRatio = 0.15
//};


//MySqlMigratorConsole migrator = new MySqlMigratorConsole(migrationConfig);
//await migrator.ExecuteMigrationAsync();

MigrationConfig migrationConfig = new MigrationConfig
{
    SourceConnectionString = "server=192.168.0.189;user id=root;password=root;database=005_mes;charset=utf8;sslMode=None;pooling=true;minpoolsize=1;maxpoolsize=1024;ConnectionLifetime=30;DefaultCommandTimeout=600;AllowLoadLocalInfile=true",
    BackupConnectionString = "server=192.168.0.122;user id=root;password=root;database=005_mes_backup;charset=utf8;sslMode=None;pooling=true;minpoolsize=1;maxpoolsize=1024;ConnectionLifetime=30;DefaultCommandTimeout=600;AllowLoadLocalInfile=true",
    TableNames = new List<string>
      {
          //"jj_alarm_records",
          //"jj_plc_alarm",
          //"xyh_alarm_records"
          "xyh_plc_loading_and_unloading"
      },
    KeepRecentDays = 20
};


MySqlMigrator migrator = new MySqlMigrator(migrationConfig);
await migrator.ExecuteMigrationAsync(CancellationToken.None);

Console.Read();