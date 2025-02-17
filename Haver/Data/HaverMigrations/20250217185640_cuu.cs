using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace haver.Data.HaverMigrations
{
    /// <inheritdoc />
    public partial class cuu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Machines_SalesOrders_SalesOrderID",
                table: "Machines");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_SalesOrders_SalesOrderID",
                table: "Machines",
                column: "SalesOrderID",
                principalTable: "SalesOrders",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Machines_SalesOrders_SalesOrderID",
                table: "Machines");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_SalesOrders_SalesOrderID",
                table: "Machines",
                column: "SalesOrderID",
                principalTable: "SalesOrders",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
