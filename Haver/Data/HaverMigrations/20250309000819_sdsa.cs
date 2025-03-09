using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace haver.Data.HaverMigrations
{
    /// <inheritdoc />
    public partial class sdsa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AirSeal",
                table: "SalesOrders",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Base",
                table: "SalesOrders",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CoatingLining",
                table: "SalesOrders",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Disassembly",
                table: "SalesOrders",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Media",
                table: "SalesOrders",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SparePMedia",
                table: "SalesOrders",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SpareParts",
                table: "SalesOrders",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AirSeal",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "Base",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "CoatingLining",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "Disassembly",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "Media",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "SparePMedia",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "SpareParts",
                table: "SalesOrders");
        }
    }
}
