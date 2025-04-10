using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace haver.Data.HaverMigrations
{
    /// <inheritdoc />
    public partial class sodeldate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DelDt",
                table: "SalesOrders",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DelDt",
                table: "SalesOrders");
        }
    }
}
