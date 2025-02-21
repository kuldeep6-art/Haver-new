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
                            new Customer { ID = 1,Phone = "4395509876", CompanyName = "Owens Corning" },
                            new Customer { ID = 2,Phone = "4375509876", CompanyName = "Coloured Aggregates" },
                            new Customer { ID = 3, Phone = "4162235566", CompanyName = "Bright Mining" },
                            new Customer { ID = 4,  Phone = "9058873321", CompanyName = "Rock Solid Ltd" },
                            new Customer { ID = 5, Phone = "6479981123", CompanyName = "Granite Works" }
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
                            new Vendor { ID = 5, Name = "MEGA METALS", Phone = "6478885544", Email = "orders@megametals.com" }
                        );
                        context.SaveChanges();
                    }

                    // 3. Seed Sales Orders (Each customer gets 2)
                    if (!context.SalesOrders.Any())
                    {
                        context.SalesOrders.AddRange(
                            new SalesOrder { ID = 1, OrderNumber = "SO-1001", SoDate = DateTime.Parse("2024-06-01") },
                            new SalesOrder { ID = 2, OrderNumber = "SO-1002", SoDate = DateTime.Parse("2024-06-10") },
                            new SalesOrder { ID = 3, OrderNumber = "SO-1003", SoDate = DateTime.Parse("2024-07-01") },
                            new SalesOrder { ID = 4, OrderNumber = "SO-1004", SoDate = DateTime.Parse("2024-07-15") },
                            new SalesOrder { ID = 5, OrderNumber = "SO-1005", SoDate = DateTime.Parse("2024-08-01") },
                            new SalesOrder { ID = 6, OrderNumber = "SO-1006", SoDate = DateTime.Parse("2024-08-10") }
                        );
                        context.SaveChanges();
                    }

                    // 4. Seed Machines (Linked to Sales Orders)
                    if (!context.Machines.Any())
                    {
                        context.Machines.AddRange(
                            //new Machine { ID = 1,  ProductionOrderNumber = "3938472", SerialNumber = "HDCM-12345", SalesOrderID = 1 },
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
