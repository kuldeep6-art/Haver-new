using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace haver.Data.HaverMigrations
{
    /// <inheritdoc />
    public partial class gdatasupmm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GanttDatas_Machines_MachineID",
                table: "GanttDatas");

            migrationBuilder.AlterColumn<int>(
                name: "MachineID",
                table: "GanttDatas",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_GanttDatas_Machines_MachineID",
                table: "GanttDatas",
                column: "MachineID",
                principalTable: "Machines",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GanttDatas_Machines_MachineID",
                table: "GanttDatas");

            migrationBuilder.AlterColumn<int>(
                name: "MachineID",
                table: "GanttDatas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GanttDatas_Machines_MachineID",
                table: "GanttDatas",
                column: "MachineID",
                principalTable: "Machines",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
