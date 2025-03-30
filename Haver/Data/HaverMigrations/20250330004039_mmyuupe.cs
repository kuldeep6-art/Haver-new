using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace haver.Data.HaverMigrations
{
    /// <inheritdoc />
    public partial class mmyuupe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MachineType_Class_Size_Deck",
                table: "MachineType");

            migrationBuilder.DropColumn(
                name: "Class",
                table: "MachineType");

            migrationBuilder.DropColumn(
                name: "Deck",
                table: "MachineType");

            migrationBuilder.RenameColumn(
                name: "Size",
                table: "MachineType",
                newName: "Description");

            migrationBuilder.CreateIndex(
                name: "IX_MachineType_Description",
                table: "MachineType",
                column: "Description",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MachineType_Description",
                table: "MachineType");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "MachineType",
                newName: "Size");

            migrationBuilder.AddColumn<string>(
                name: "Class",
                table: "MachineType",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Deck",
                table: "MachineType",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MachineType_Class_Size_Deck",
                table: "MachineType",
                columns: new[] { "Class", "Size", "Deck" },
                unique: true);
        }
    }
}
