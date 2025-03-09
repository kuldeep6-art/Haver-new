using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace haver.Data.HaverMigrations
{
    /// <inheritdoc />
    public partial class mtypeup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MachineType_Description",
                table: "MachineType");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "MachineType");

            migrationBuilder.DropColumn(
                name: "AirSeal",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "Base",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "CoatingLining",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "Disassembly",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "Media",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "SparePMedia",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "SpareParts",
                table: "Machines");

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

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "MachineType",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Class",
                table: "MachineType");

            migrationBuilder.DropColumn(
                name: "Deck",
                table: "MachineType");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "MachineType");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "MachineType",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "AirSeal",
                table: "Machines",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Base",
                table: "Machines",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CoatingLining",
                table: "Machines",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Disassembly",
                table: "Machines",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Media",
                table: "Machines",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SparePMedia",
                table: "Machines",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SpareParts",
                table: "Machines",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_MachineType_Description",
                table: "MachineType",
                column: "Description",
                unique: true);
        }
    }
}
