using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace haver.Data.HaverMigrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompanyName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
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
                name: "Employees",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Engineers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EngineerInitials = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
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
                name: "MachineType",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineType", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UserSelections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SelectionJson = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSelections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
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
                name: "SalesOrders",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderNumber = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    CompanyName = table.Column<string>(type: "TEXT", nullable: false),
                    SoDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Price = table.Column<decimal>(type: "TEXT", nullable: true),
                    Currency = table.Column<string>(type: "TEXT", nullable: true),
                    ShippingTerms = table.Column<string>(type: "TEXT", maxLength: 800, nullable: true),
                    AppDwgExp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AppDwgRel = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AppDwgRet = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PreOExp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PreORel = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EngPExp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EngPRel = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    Comments = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDraft = table.Column<bool>(type: "INTEGER", nullable: false),
                    CustomerID = table.Column<int>(type: "INTEGER", nullable: true),
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
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "GanttTasks",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SalesOrderID = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GanttTasks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_GanttTasks_SalesOrders_SalesOrderID",
                        column: x => x.SalesOrderID,
                        principalTable: "SalesOrders",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineModel = table.Column<string>(type: "TEXT", nullable: false),
                    SerialNumber = table.Column<string>(type: "TEXT", nullable: false),
                    ProductionOrderNumber = table.Column<string>(type: "TEXT", maxLength: 7, nullable: true),
                    AssemblyExp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AssemblyStart = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AssemblyComplete = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RToShipExp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RToShipA = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Media = table.Column<bool>(type: "INTEGER", nullable: false),
                    SpareParts = table.Column<bool>(type: "INTEGER", nullable: false),
                    SparePMedia = table.Column<bool>(type: "INTEGER", nullable: false),
                    Base = table.Column<bool>(type: "INTEGER", nullable: false),
                    AirSeal = table.Column<bool>(type: "INTEGER", nullable: false),
                    CoatingLining = table.Column<bool>(type: "INTEGER", nullable: false),
                    Disassembly = table.Column<bool>(type: "INTEGER", nullable: false),
                    BudgetedHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    ActualAssemblyHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    ReworkHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    Nameplate = table.Column<int>(type: "INTEGER", nullable: true),
                    PreOrder = table.Column<string>(type: "TEXT", nullable: true),
                    Scope = table.Column<string>(type: "TEXT", nullable: true),
                    SalesOrderID = table.Column<int>(type: "INTEGER", nullable: false),
                    MachineTypeID = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Machines_MachineType_MachineTypeID",
                        column: x => x.MachineTypeID,
                        principalTable: "MachineType",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Machines_SalesOrders_SalesOrderID",
                        column: x => x.SalesOrderID,
                        principalTable: "SalesOrders",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderEngineers",
                columns: table => new
                {
                    SalesOrderID = table.Column<int>(type: "INTEGER", nullable: false),
                    EngineerID = table.Column<int>(type: "INTEGER", nullable: false),
                    ID = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderEngineers", x => new { x.SalesOrderID, x.EngineerID });
                    table.ForeignKey(
                        name: "FK_SalesOrderEngineers_Engineers_EngineerID",
                        column: x => x.EngineerID,
                        principalTable: "Engineers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrderEngineers_SalesOrders_SalesOrderID",
                        column: x => x.SalesOrderID,
                        principalTable: "SalesOrders",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GanttMilestones",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GanttTaskID = table.Column<int>(type: "INTEGER", nullable: false),
                    MilestoneName = table.Column<int>(type: "INTEGER", nullable: false),
                    Progress = table.Column<int>(type: "INTEGER", nullable: false),
                    DateCompleted = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GanttMilestones", x => x.ID);
                    table.ForeignKey(
                        name: "FK_GanttMilestones_GanttTasks_GanttTaskID",
                        column: x => x.GanttTaskID,
                        principalTable: "GanttTasks",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GanttDatas",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SalesOrderID = table.Column<int>(type: "INTEGER", nullable: false),
                    MachineID = table.Column<int>(type: "INTEGER", nullable: true),
                    AppDExp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AppDRcd = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StartOfWeek = table.Column<int>(type: "INTEGER", nullable: false),
                    EngExpected = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EngReleased = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PackageReleased = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PurchaseOrdersIssued = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PurchaseOrdersCompleted = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PurchaseOrdersReceived = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SupplierPODue = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AssemblyStart = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AssemblyComplete = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ShipExpected = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ShipActual = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeliveryExpected = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeliveryActual = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    IsFinalized = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GanttDatas", x => x.ID);
                    table.ForeignKey(
                        name: "FK_GanttDatas_Machines_MachineID",
                        column: x => x.MachineID,
                        principalTable: "Machines",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_GanttDatas_SalesOrders_SalesOrderID",
                        column: x => x.SalesOrderID,
                        principalTable: "SalesOrders",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Procurements",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VendorID = table.Column<int>(type: "INTEGER", nullable: false),
                    MachineID = table.Column<int>(type: "INTEGER", nullable: true),
                    PONumber = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    PODueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PORcd = table.Column<DateTime>(type: "TEXT", nullable: true),
                    QualityICom = table.Column<bool>(type: "INTEGER", nullable: false),
                    NcrRaised = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Procurements", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Procurements_Machines_MachineID",
                        column: x => x.MachineID,
                        principalTable: "Machines",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Procurements_Vendors_VendorID",
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
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Engineers_Email",
                table: "Engineers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Engineers_EngineerInitials",
                table: "Engineers",
                column: "EngineerInitials",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Engineers_FirstName_LastName",
                table: "Engineers",
                columns: new[] { "FirstName", "LastName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GanttDatas_MachineID",
                table: "GanttDatas",
                column: "MachineID");

            migrationBuilder.CreateIndex(
                name: "IX_GanttDatas_SalesOrderID",
                table: "GanttDatas",
                column: "SalesOrderID");

            migrationBuilder.CreateIndex(
                name: "IX_GanttMilestones_GanttTaskID",
                table: "GanttMilestones",
                column: "GanttTaskID");

            migrationBuilder.CreateIndex(
                name: "IX_GanttTasks_SalesOrderID",
                table: "GanttTasks",
                column: "SalesOrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_MachineTypeID",
                table: "Machines",
                column: "MachineTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_ProductionOrderNumber",
                table: "Machines",
                column: "ProductionOrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_SalesOrderID",
                table: "Machines",
                column: "SalesOrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_SerialNumber",
                table: "Machines",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MachineType_Description",
                table: "MachineType",
                column: "Description",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Procurements_MachineID",
                table: "Procurements",
                column: "MachineID");

            migrationBuilder.CreateIndex(
                name: "IX_Procurements_PONumber",
                table: "Procurements",
                column: "PONumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Procurements_VendorID",
                table: "Procurements",
                column: "VendorID");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderEngineers_EngineerID",
                table: "SalesOrderEngineers",
                column: "EngineerID");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_CustomerID",
                table: "SalesOrders",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_OrderNumber",
                table: "SalesOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_Name",
                table: "Vendors",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "GanttDatas");

            migrationBuilder.DropTable(
                name: "GanttMilestones");

            migrationBuilder.DropTable(
                name: "Procurements");

            migrationBuilder.DropTable(
                name: "SalesOrderEngineers");

            migrationBuilder.DropTable(
                name: "UserSelections");

            migrationBuilder.DropTable(
                name: "GanttTasks");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "Vendors");

            migrationBuilder.DropTable(
                name: "Engineers");

            migrationBuilder.DropTable(
                name: "MachineType");

            migrationBuilder.DropTable(
                name: "SalesOrders");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
