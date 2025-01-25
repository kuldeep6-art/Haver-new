using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace haver.Data.HaverMigrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    CompanyName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Engineers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Engineers", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SerialNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Size = table.Column<string>(type: "TEXT", nullable: false),
                    Class = table.Column<string>(type: "TEXT", nullable: false),
                    SizeDeck = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MachineSchedules",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PackageRDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PODueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    Media = table.Column<bool>(type: "INTEGER", nullable: false),
                    SpareParts = table.Column<bool>(type: "INTEGER", nullable: false),
                    SparePMedia = table.Column<bool>(type: "INTEGER", nullable: false),
                    Base = table.Column<bool>(type: "INTEGER", nullable: false),
                    AirSeal = table.Column<bool>(type: "INTEGER", nullable: false),
                    CoatingLining = table.Column<bool>(type: "INTEGER", nullable: false),
                    Dissembly = table.Column<bool>(type: "INTEGER", nullable: false),
                    MachineID = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineSchedules", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MachineSchedules_Machines_MachineID",
                        column: x => x.MachineID,
                        principalTable: "Machines",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MachineScheduleEngineers",
                columns: table => new
                {
                    MachineScheduleID = table.Column<int>(type: "INTEGER", nullable: false),
                    EngineerID = table.Column<int>(type: "INTEGER", nullable: false),
                    ID = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineScheduleEngineers", x => new { x.MachineScheduleID, x.EngineerID });
                    table.ForeignKey(
                        name: "FK_MachineScheduleEngineers_Engineers_EngineerID",
                        column: x => x.EngineerID,
                        principalTable: "Engineers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MachineScheduleEngineers_MachineSchedules_MachineScheduleID",
                        column: x => x.MachineScheduleID,
                        principalTable: "MachineSchedules",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PreOrder = table.Column<string>(type: "TEXT", nullable: true),
                    Scope = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    AssemblyHours = table.Column<decimal>(type: "TEXT", nullable: false),
                    ReworkHours = table.Column<decimal>(type: "TEXT", nullable: false),
                    BudgetHours = table.Column<decimal>(type: "TEXT", nullable: false),
                    NamePlate = table.Column<int>(type: "INTEGER", nullable: true),
                    MachineScheduleID = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Notes_MachineSchedules_MachineScheduleID",
                        column: x => x.MachineScheduleID,
                        principalTable: "MachineSchedules",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageReleases",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PReleaseDateP = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PReleaseDateA = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    MachineScheduleID = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageReleases", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PackageReleases_MachineSchedules_MachineScheduleID",
                        column: x => x.MachineScheduleID,
                        principalTable: "MachineSchedules",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrders",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderNumber = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    SoDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    ShippingTerms = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AppDwgRcd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DwgIsDt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PoNumber = table.Column<string>(type: "TEXT", nullable: true),
                    CustomerID = table.Column<int>(type: "INTEGER", nullable: false),
                    VendorID = table.Column<int>(type: "INTEGER", nullable: false),
                    MachineScheduleID = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrders", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SalesOrders_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrders_MachineSchedules_MachineScheduleID",
                        column: x => x.MachineScheduleID,
                        principalTable: "MachineSchedules",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesOrders_Vendors_VendorID",
                        column: x => x.VendorID,
                        principalTable: "Vendors",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Phone",
                table: "Customers",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_SerialNumber",
                table: "Machines",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MachineScheduleEngineers_EngineerID",
                table: "MachineScheduleEngineers",
                column: "EngineerID");

            migrationBuilder.CreateIndex(
                name: "IX_MachineScheduleEngineers_MachineScheduleID_EngineerID",
                table: "MachineScheduleEngineers",
                columns: new[] { "MachineScheduleID", "EngineerID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MachineSchedules_MachineID",
                table: "MachineSchedules",
                column: "MachineID");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_MachineScheduleID",
                table: "Notes",
                column: "MachineScheduleID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageReleases_MachineScheduleID",
                table: "PackageReleases",
                column: "MachineScheduleID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageReleases_Name_MachineScheduleID",
                table: "PackageReleases",
                columns: new[] { "Name", "MachineScheduleID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_CustomerID",
                table: "SalesOrders",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_MachineScheduleID",
                table: "SalesOrders",
                column: "MachineScheduleID");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_OrderNumber",
                table: "SalesOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_VendorID",
                table: "SalesOrders",
                column: "VendorID");

            ExtraMigration.Steps(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MachineScheduleEngineers");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "PackageReleases");

            migrationBuilder.DropTable(
                name: "SalesOrders");

            migrationBuilder.DropTable(
                name: "Engineers");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "MachineSchedules");

            migrationBuilder.DropTable(
                name: "Vendors");

            migrationBuilder.DropTable(
                name: "Machines");
        }
    }
}
