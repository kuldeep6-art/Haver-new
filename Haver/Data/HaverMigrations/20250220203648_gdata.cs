using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace haver.Data.HaverMigrations
{
    /// <inheritdoc />
    public partial class gdata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GanttDatas",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineID = table.Column<int>(type: "INTEGER", nullable: false),
                    EngExpected = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EngReleased = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CustomerApproval = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PackageReleased = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PurchaseOrdersIssued = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PurchaseOrdersCompleted = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SupplierPODue = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AssemblyStart = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AssemblyComplete = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ShipExpected = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ShipActual = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeliveryExpected = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeliveryActual = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GanttDatas", x => x.ID);
                    table.ForeignKey(
                        name: "FK_GanttDatas_Machines_MachineID",
                        column: x => x.MachineID,
                        principalTable: "Machines",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GanttDatas_MachineID",
                table: "GanttDatas",
                column: "MachineID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GanttDatas");
        }
    }
}
