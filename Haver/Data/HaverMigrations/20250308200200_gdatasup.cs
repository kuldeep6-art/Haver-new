using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace haver.Data.HaverMigrations
{
    /// <inheritdoc />
    public partial class gdatasup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalesOrderID",
                table: "GanttDatas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GanttDatas_SalesOrderID",
                table: "GanttDatas",
                column: "SalesOrderID");

            migrationBuilder.AddForeignKey(
                name: "FK_GanttDatas_SalesOrders_SalesOrderID",
                table: "GanttDatas",
                column: "SalesOrderID",
                principalTable: "SalesOrders",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GanttDatas_SalesOrders_SalesOrderID",
                table: "GanttDatas");

            migrationBuilder.DropIndex(
                name: "IX_GanttDatas_SalesOrderID",
                table: "GanttDatas");

            migrationBuilder.DropColumn(
                name: "SalesOrderID",
                table: "GanttDatas");
        }
    }
}
