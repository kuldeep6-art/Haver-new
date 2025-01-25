using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace haver.Data.HaverMigrations
{
    /// <inheritdoc />
    public partial class Audit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Vendors",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Vendors",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Vendors",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "Vendors",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SalesOrders",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "SalesOrders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "SalesOrders",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "SalesOrders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "PackageReleases",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "PackageReleases",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "PackageReleases",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "PackageReleases",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Notes",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Notes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Notes",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "Notes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MachineSchedules",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "MachineSchedules",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "MachineSchedules",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "MachineSchedules",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MachineScheduleEngineers",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "MachineScheduleEngineers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "MachineScheduleEngineers",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "MachineScheduleEngineers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Machines",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Machines",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Machines",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "Machines",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Engineers",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Engineers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Engineers",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "Engineers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Customers",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Customers",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "Customers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PackageReleases");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "PackageReleases");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PackageReleases");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "PackageReleases");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MachineSchedules");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "MachineSchedules");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "MachineSchedules");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "MachineSchedules");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MachineScheduleEngineers");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "MachineScheduleEngineers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "MachineScheduleEngineers");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "MachineScheduleEngineers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Engineers");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Engineers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Engineers");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "Engineers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "Customers");
        }
    }
}
