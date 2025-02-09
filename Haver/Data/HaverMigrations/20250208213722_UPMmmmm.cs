using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace haver.Data.HaverMigrations
{
    /// <inheritdoc />
    public partial class UPMmmmm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Machines_MachineType_MachineTypeID",
                table: "Machines");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_MachineType_MachineTypeID",
                table: "Machines",
                column: "MachineTypeID",
                principalTable: "MachineType",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Machines_MachineType_MachineTypeID",
                table: "Machines");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_MachineType_MachineTypeID",
                table: "Machines",
                column: "MachineTypeID",
                principalTable: "MachineType",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
