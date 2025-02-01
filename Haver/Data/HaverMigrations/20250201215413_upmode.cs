using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace haver.Data.HaverMigrations
{
    /// <inheritdoc />
    public partial class upmode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PackageReleases_SalesOrderID",
                table: "PackageReleases");

            migrationBuilder.CreateIndex(
                name: "IX_PackageReleases_SalesOrderID",
                table: "PackageReleases",
                column: "SalesOrderID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PackageReleases_SalesOrderID",
                table: "PackageReleases");

            migrationBuilder.CreateIndex(
                name: "IX_PackageReleases_SalesOrderID",
                table: "PackageReleases",
                column: "SalesOrderID");
        }
    }
}
