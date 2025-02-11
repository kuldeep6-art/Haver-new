using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace haver.Data.HaverMigrations
{
    /// <inheritdoc />
    public partial class newu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "SalesOrders");

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "SalesOrders",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "SalesOrders");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "SalesOrders",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
