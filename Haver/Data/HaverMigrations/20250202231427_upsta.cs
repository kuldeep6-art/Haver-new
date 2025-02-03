using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace haver.Data.HaverMigrations
{
    /// <inheritdoc />
    public partial class upsta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "SalesOrders",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "SalesOrders");
        }
    }
}
