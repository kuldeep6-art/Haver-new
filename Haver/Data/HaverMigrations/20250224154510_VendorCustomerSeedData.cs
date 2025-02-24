using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace haver.Data.HaverMigrations
{
    /// <inheritdoc />
    public partial class VendorCustomerSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "ID", "CompanyName", "CreatedBy", "CreatedOn", "Phone", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { 1, "Owens Corning", null, null, "4395509876", null, null },
                    { 2, "Coloured Aggregates", null, null, "4375509876", null, null },
                    { 3, "Bright Mining", null, null, "4162235566", null, null },
                    { 4, "Rock Solid Ltd", null, null, "9058873321", null, null },
                    { 5, "Granite Works", null, null, "6479981123", null, null },
                    { 6, "FMI", null, null, "6479901234", null, null },
                    { 7, "Teck Coal (EVR)", null, null, "6479901235", null, null },
                    { 8, "Nutrien (PCS Lanigan)", null, null, "6479901236", null, null },
                    { 9, "Commonwealth Equipment", null, null, "6479901237", null, null },
                    { 10, "Rio Tinto Sorel", null, null, "6479901238", null, null },
                    { 11, "Calidra La Laja (HBM)", null, null, "6479901239", null, null },
                    { 12, "Coast Aggregates", null, null, "6479901240", null, null },
                    { 13, "Direct Reduction Iron (HNG)", null, null, "6479901241", null, null },
                    { 14, "Mosaic 4C (Motion)", null, null, "6479901242", null, null },
                    { 15, "Lhoist NA Clifton, TX", null, null, "6479901243", null, null },
                    { 16, "Kumtor Gold (HNG)", null, null, "6479901244", null, null },
                    { 17, "Tehachapi Cement", null, null, "6479901245", null, null }
                });

            migrationBuilder.InsertData(
                table: "Vendors",
                columns: new[] { "ID", "CreatedBy", "CreatedOn", "Email", "IsActive", "Name", "Phone", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { 1, null, null, "purchaseorders@hingstonmetal.com", true, "HINGSTON METAL", "9056583456", null, null },
                    { 2, null, null, "orders@processgroup.com", true, "PROCESS GROUP", "2876581056", null, null },
                    { 3, null, null, "sales@steelworks.com", true, "STEELWORKS INC.", "4167789988", null, null },
                    { 4, null, null, "info@miningsupply.com", true, "MINING SUPPLY CO.", "9056623344", null, null },
                    { 5, null, null, "orders@rosta.com", true, "Rosta", "6478885540", null, null },
                    { 6, null, null, "orders@vj-pamensky.com", true, "VJ Pamensky", "6478885541", null, null },
                    { 7, null, null, "orders@precisionmw.com", true, "Precision MetalWorks", "6478885542", null, null },
                    { 8, null, null, "orders@teco-westinghouse.com", true, "Teco-Westinghouse", "6478885543", null, null },
                    { 9, null, null, "orders@skfcanada.com", true, "SKF Canada", "6478885544", null, null },
                    { 10, null, null, "orders@metalzrprofiles.com", true, "METALZR PROFILES", "6478885545", null, null },
                    { 11, null, null, "orders@rightmachine.com", true, "Right Machine", "6478885546", null, null },
                    { 12, null, null, "orders@megametals.com", true, "MEGA METALS", "6478885547", null, null },
                    { 13, null, null, "orders@tandem.com", true, "Tandem", "6478885548", null, null },
                    { 14, null, null, "orders@niagararubber.com", true, "Niagara Rubber Supply", "6478885549", null, null },
                    { 15, null, null, "orders@martinsprocket.com", true, "Martin Sprocket", "6478885550", null, null },
                    { 16, null, null, "orders@hmftinc.com", true, "HMFT Inc", "6478885551", null, null },
                    { 17, null, null, "orders@hng.com", true, "HNG", "6478885552", null, null },
                    { 18, null, null, "orders@hbl.com", true, "HBL", "6478885553", null, null },
                    { 19, null, null, "orders@majorwire.com", true, "MAJOR WIRE", "6478885554", null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "ID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "ID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "ID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "ID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "ID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "ID",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "ID",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "ID",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "ID",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "ID",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "ID",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "ID",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "ID",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "ID",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "ID",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "ID",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "ID",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "ID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "ID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "ID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "ID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "ID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "ID",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "ID",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "ID",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "ID",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "ID",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "ID",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "ID",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "ID",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "ID",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "ID",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "ID",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "ID",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "ID",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "ID",
                keyValue: 19);
        }
    }
}
