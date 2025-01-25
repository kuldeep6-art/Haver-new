using Microsoft.EntityFrameworkCore.Migrations;

namespace haver.Data
{
    public class ExtraMigration
    {
        public static void Steps(MigrationBuilder migrationBuilder)
        {
            // Drop triggers if they already exist
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS SetMachineScheduleTimestampOnUpdate;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS SetMachineScheduleTimestampOnInsert;");

            // Create triggers for MachineSchedule table
            migrationBuilder.Sql(
                @"
                CREATE TRIGGER SetMachineScheduleTimestampOnUpdate
                AFTER UPDATE ON MachineSchedules
                BEGIN
                    UPDATE  MachineSchedules
                    SET RowVersion = randomblob(8)
                    WHERE rowid = NEW.rowid;
                END
            ");

            migrationBuilder.Sql(
                @"
                CREATE TRIGGER SetMachineScheduleTimestampOnInsert
                AFTER INSERT ON MachineSchedules
                BEGIN
                    UPDATE MachineSchedules
                    SET RowVersion = randomblob(8)
                    WHERE rowid = NEW.rowid;
                END
            ");

            // Create triggers for Tables table
            //migrationBuilder.Sql(
              
            //");
        }
    }
}
