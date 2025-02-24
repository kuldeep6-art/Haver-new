using System.Diagnostics;
using haver.Models;
using Microsoft.EntityFrameworkCore;

namespace haver.Data
{
    public static class HaverInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider, bool DeleteDatabase = false, bool UseMigrations = true)
        {
            using (var context = new HaverContext(
                serviceProvider.GetRequiredService<DbContextOptions<HaverContext>>()))
            {
				#region Prepare the Database
				try
				{
					if (UseMigrations)
					{
						if (DeleteDatabase)
						{
							context.Database.EnsureDeleted(); //Delete the existing version 
						}
						context.Database.Migrate(); //Apply all migrations
					}
					else
					{
						if (DeleteDatabase)
						{
							context.Database.EnsureDeleted(); //Delete the existing version 
							context.Database.EnsureCreated(); //Create and update the database as per the Model
						}
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.GetBaseException().Message);
				}
				#endregion
				#region seed data
				try
				{
                    // 1. Seed Customers
                    if (!context.Customers.Any())
                    {
                        context.Customers.AddRange(
                            new Customer { ID = 1, Phone = "4395509876", CompanyName = "Owens Corning" },
                            new Customer { ID = 2, Phone = "4375509876", CompanyName = "Coloured Aggregates" },
                            new Customer { ID = 3, Phone = "4162235566", CompanyName = "Bright Mining" },
                            new Customer { ID = 4, Phone = "9058873321", CompanyName = "Rock Solid Ltd" },
                            new Customer { ID = 5, Phone = "6479981123", CompanyName = "Granite Works" },
                            new Customer { ID = 6, Phone = "6479901234", CompanyName = "FMI" },
                            new Customer { ID = 7, Phone = "6479901235", CompanyName = "Teck Coal (EVR)" },
                            new Customer { ID = 8, Phone = "6479901236", CompanyName = "Nutrien (PCS Lanigan)" },
                            new Customer { ID = 9, Phone = "6479901237", CompanyName = "Commonwealth Equipment" },
                            new Customer { ID = 10, Phone = "6479901238", CompanyName = "Rio Tinto Sorel" },
                            new Customer { ID = 11, Phone = "6479901239", CompanyName = "Calidra La Laja (HBM)" },
                            new Customer { ID = 12, Phone = "6479901240", CompanyName = "Coast Aggregates" },
                            new Customer { ID = 13, Phone = "6479901241", CompanyName = "Direct Reduction Iron (HNG)" },
                            new Customer { ID = 14, Phone = "6479901242", CompanyName = "Mosaic 4C (Motion)" },
                            new Customer { ID = 15, Phone = "6479901243", CompanyName = "Lhoist NA Clifton, TX" },
                            new Customer { ID = 16, Phone = "6479901244", CompanyName = "Kumtor Gold (HNG)" },
                            new Customer { ID = 17, Phone = "6479901245", CompanyName = "Tehachapi Cement" }
                        );
                        context.SaveChanges();
                    }

                    // 2. Seed Vendors
                    if (!context.Vendors.Any())
                    {
                        context.Vendors.AddRange(
                            new Vendor { ID = 1, Name = "HINGSTON METAL", Phone = "9056583456", Email = "purchaseorders@hingstonmetal.com" },
                            new Vendor { ID = 2, Name = "PROCESS GROUP", Phone = "2876581056", Email = "orders@processgroup.com" },
                            new Vendor { ID = 3, Name = "STEELWORKS INC.", Phone = "4167789988", Email = "sales@steelworks.com" },
                            new Vendor { ID = 4, Name = "MINING SUPPLY CO.", Phone = "9056623344", Email = "info@miningsupply.com" },
                            new Vendor { ID = 5, Name = "Rosta", Phone = "6478885540", Email = "orders@rosta.com" },
                            new Vendor { ID = 6, Name = "VJ Pamensky", Phone = "6478885541", Email = "orders@vj-pamensky.com" },
                            new Vendor { ID = 7, Name = "Precision MetalWorks", Phone = "6478885542", Email = "orders@precisionmw.com" },
                            new Vendor { ID = 8, Name = "Teco-Westinghouse", Phone = "6478885543", Email = "orders@teco-westinghouse.com" },
                            new Vendor { ID = 9, Name = "SKF Canada", Phone = "6478885544", Email = "orders@skfcanada.com" },
                            new Vendor { ID = 10, Name = "METALZR PROFILES", Phone = "6478885545", Email = "orders@metalzrprofiles.com" },
                            new Vendor { ID = 11, Name = "Right Machine", Phone = "6478885546", Email = "orders@rightmachine.com" },
                            new Vendor { ID = 12, Name = "MEGA METALS", Phone = "6478885547", Email = "orders@megametals.com" },
                            new Vendor { ID = 13, Name = "Tandem", Phone = "6478885548", Email = "orders@tandem.com" },
                            new Vendor { ID = 14, Name = "Niagara Rubber Supply", Phone = "6478885549", Email = "orders@niagararubber.com" },
                            new Vendor { ID = 15, Name = "Martin Sprocket", Phone = "6478885550", Email = "orders@martinsprocket.com" },
                            new Vendor { ID = 16, Name = "HMFT Inc", Phone = "6478885551", Email = "orders@hmftinc.com" },
                            new Vendor { ID = 17, Name = "HNG", Phone = "6478885552", Email = "orders@hng.com" },
                            new Vendor { ID = 18, Name = "HBL", Phone = "6478885553", Email = "orders@hbl.com" },
                            new Vendor { ID = 19, Name = "MAJOR WIRE", Phone = "6478885554", Email = "orders@majorwire.com" }
                        );
                        context.SaveChanges();
                    }

                    // 3. Seed Sales Orders (Each customer gets 2)
                    if (!context.SalesOrders.Any())
                    {
                        context.SalesOrders.AddRange(
                            new SalesOrder { ID = 1, OrderNumber = "10430736", SoDate = DateTime.Parse("2025-02-21") },
                            new SalesOrder { ID = 2, OrderNumber = "10430754", SoDate = DateTime.Parse("2025-02-17") },
                            new SalesOrder { ID = 3, OrderNumber = "10430709", SoDate = DateTime.Parse("2025-02-18") },
                            new SalesOrder { ID = 4, OrderNumber = "10430798", SoDate = DateTime.Parse("2025-02-23") },
                            new SalesOrder { ID = 5, OrderNumber = "10430765", SoDate = DateTime.Parse("2025-02-22") },
                            new SalesOrder { ID = 6, OrderNumber = "10430792", SoDate = DateTime.Parse("2025-02-20") }
                        );
                        context.SaveChanges();
                    }

                    // 4. Seed Machines (Linked to Sales Orders)
                    if (!context.Machines.Any())
                    {
                        context.Machines.AddRange(
                            new Machine { ID = 1,  ProductionOrderNumber = "3938472", SerialNumber = "HDCM-12345", SalesOrderID = 1 },
                            new Machine { ID = 2,  ProductionOrderNumber = "3933442", SerialNumber = "PM-54321",  SalesOrderID = 2 },
                            new Machine { ID = 3,  ProductionOrderNumber = "3987472", SerialNumber = "MX-98765",  SalesOrderID = 3 },
                            new Machine { ID = 4,  ProductionOrderNumber = "3930972", SerialNumber = "GTX-56789", SalesOrderID = 4 },
                            new Machine { ID = 5,  ProductionOrderNumber = "3975472", SerialNumber = "PR-11223",  SalesOrderID = 5 },
                            new Machine { ID = 6, ProductionOrderNumber = "3933472", SerialNumber = "BLK-33445", SalesOrderID = 6 }
                        );
                        context.SaveChanges();
                    }

                    // 5. Seed Engineers
                    if (!context.Engineers.Any())
                    {
                        context.Engineers.AddRange(
                            new Engineer { ID = 1, FirstName = "Ethan", LastName = "Jones" },
                            new Engineer { ID = 2, FirstName = "Sophia", LastName = "Smith" },
                            new Engineer { ID = 3, FirstName = "Liam", LastName = "Brown" },
                            new Engineer { ID = 4, FirstName = "Olivia", LastName = "Taylor" }
                        );
                        context.SaveChanges();
                    }

                    // 6. Seed SalesOrderEngineers (Multiple engineers per Sales Order)
                    if (!context.SalesOrderEngineers.Any())
                    {
                        context.SalesOrderEngineers.AddRange(
                            new SalesOrderEngineer { SalesOrderID = 1, EngineerID = 1 },
                            new SalesOrderEngineer { SalesOrderID = 1, EngineerID = 2 },
                            new SalesOrderEngineer { SalesOrderID = 2, EngineerID = 3 },
                            new SalesOrderEngineer { SalesOrderID = 3, EngineerID = 1 },
                            new SalesOrderEngineer { SalesOrderID = 4, EngineerID = 2 },
                            new SalesOrderEngineer { SalesOrderID = 5, EngineerID = 4 },
                            new SalesOrderEngineer { SalesOrderID = 6, EngineerID = 3 }
                        );
                        context.SaveChanges();
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error seeding database: {ex.GetBaseException().Message}");
                }
                #endregion
            }
        }
    }
}
