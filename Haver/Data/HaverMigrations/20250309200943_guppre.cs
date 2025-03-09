using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace haver.Data.HaverMigrations
{
    /// <inheritdoc />
    public partial class guppre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreOExp",
                table: "GanttDatas");

            migrationBuilder.DropColumn(
                name: "PreORel",
                table: "GanttDatas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PreOExp",
                table: "GanttDatas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreORel",
                table: "GanttDatas",
                type: "TEXT",
                nullable: true);
        }
    }
}
