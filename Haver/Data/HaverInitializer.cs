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
                //Refresh the database as per the parameter options
             
                #region Prepare the Database
                try
                {
                    //Note: .CanConnect() will return false if the database is not there!
                    if (DeleteDatabase || !context.Database.CanConnect())
                    {
                        context.Database.EnsureDeleted(); //Delete the existing version 
                        if (UseMigrations)
                        {
                            context.Database.Migrate(); //Create the Database and apply all migrations
                        }
                        else
                        {
                            context.Database.EnsureCreated(); //Create and update the database as per the Model
                        }
                        //Now create any additional database objects such as Triggers or Views
                        //--------------------------------------------------------------------
                        //Create the Triggers
                        string sqlCmd = @"
                            CREATE TRIGGER SetSalesOrderTimestampOnUpdate
                            AFTER UPDATE ON SalesOrders
                            BEGIN
                                UPDATE SalesOrders
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END;
                        ";
                        context.Database.ExecuteSqlRaw(sqlCmd);

                        sqlCmd = @"
                            CREATE TRIGGER SetSalesOrderTimestampOnInsert
                            AFTER INSERT ON SalesOrders
                            BEGIN
                                UPDATE SalesOrders
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END
                        ";
                        context.Database.ExecuteSqlRaw(sqlCmd);

                        sqlCmd = @"
                            CREATE TRIGGER SetMachineTimestampOnUpdate
                            AFTER UPDATE ON Machines
                            BEGIN
                                UPDATE Machines
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END;
                        ";
                        context.Database.ExecuteSqlRaw(sqlCmd);

                        sqlCmd = @"
                            CREATE TRIGGER SetMachineTimestampOnInsert
                            AFTER INSERT ON Machines
                            BEGIN
                                UPDATE Machines
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END
                        ";
                        context.Database.ExecuteSqlRaw(sqlCmd);


                    }
                    else //The database is already created
                    {
                        if (UseMigrations)
                        {
                            context.Database.Migrate(); //Apply all migrations
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
                            new Customer { ID = 5, Phone = "6479981123", CompanyName = "Granite Works" },
                             new Customer { ID = 6, Phone = "9086766565", CompanyName = "FMI" },
                              new Customer { ID = 7, Phone = "5764768797", CompanyName = "Rio Tinto Sorel" },
                               new Customer { ID = 8, Phone = "8768909876", CompanyName = "Coast Aggregates" },
                                new Customer { ID = 9, Phone = "5489097654", CompanyName = "Intradco" },
                                 new Customer { ID = 10, Phone = "2897650987", CompanyName = "United Taconite" }
                        );
                        context.SaveChanges();
                    }

                    // Look for any Employees.  Seed ones to match the seeded Identity accounts.
                    if (!context.Employees.Any())
                    {
                        context.Employees.AddRange(
                         new Employee
                         {
                             FirstName = "Gregory",
                             LastName = "House",
                             Email = "admin@haverniagara.com"
                         },
                         new Employee
                         {
                             FirstName = "Fred",
                             LastName = "Flintstone",
                             Email = "sales@haverniagara.com"
                         },
                          new Employee
                          {
                              FirstName = "Kelly",
                              LastName = "Hunt",
                              Email = "pment@haverniagara.com"
                          },
                          new Employee
                          {
                              FirstName = "Klay",
                              LastName = "Log",
                              Email = "production@haverniagara.com"
                          },
                          new Employee
                          {
                              FirstName = "Gody",
                              LastName = "Lakes",
                              Email = "pic@haverniagara.com"
                          },
                            new Employee
                            {
                                FirstName = "Siu",
                                LastName = "Chris",
                                Email = "engineering@haverniagara.com"
                            }
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
                            new Vendor { ID = 5, Name = "MEGA METALS", Phone = "6478885544", Email = "orders@megametals.com" },
                            new Vendor { ID = 6, Name = "Rosta Inc", Phone = "2837462374", Email = "orders@rosta.com" },
                            new Vendor { ID = 7, Name = "Niagara Rubber Supply", Phone = "2342321213", Email = "orders@nrs.com" },
                            new Vendor { ID = 8, Name = "Right Machine", Phone = "2341212131", Email = "orders@rm.com" },
                            new Vendor { ID = 9, Name = "Martin Sprocket", Phone = "1241213131", Email = "orders@ms.com" },
                            new Vendor { ID = 10, Name = "Precision Metalworks", Phone = "1214131311", Email = "orders@pm.com" }
                        );
                        context.SaveChanges();
                    }
                    // 5. Seed Engineers
                    if (!context.Engineers.Any())
                    {
                        context.Engineers.AddRange(
                            new Engineer { ID = 1, FirstName = "Ethan", LastName = "Jones",EngineerInitials="EJ",Email="ej@outlook.com" },
                            new Engineer { ID = 2, FirstName = "Chloe", LastName = "Homp", EngineerInitials = "CH", Email = "ch@outlook.com" },
                            new Engineer { ID = 3, FirstName = "Cady", LastName = "Tank", EngineerInitials = "CT", Email = "ct@outlook.com" },
                            new Engineer { ID = 4, FirstName = "Lucy", LastName = "Hills", EngineerInitials = "LI", Email = "li@outlook.com" },
                             new Engineer { ID = 5, FirstName = "Joe", LastName = "Roe", EngineerInitials = "JR", Email = "jr@outlook.com" },
                              new Engineer { ID = 6, FirstName = "Rose", LastName = "Kelly", EngineerInitials = "RK", Email = "rk@outlook.com" },
                               new Engineer { ID = 7, FirstName = "Maddy", LastName = "Camp", EngineerInitials = "MC", Email = "mc@outlook.com" },
                                new Engineer { ID = 8, FirstName = "Roy", LastName = "Jon", EngineerInitials = "RJ", Email = "rj@outlook.com" },
                                 new Engineer { ID = 9, FirstName = "Klay", LastName = "Lot", EngineerInitials = "KL", Email = "kl@outlook.com" },
                                  new Engineer { ID = 10, FirstName = "Eddie", LastName = "Hearn", EngineerInitials = "EH", Email = "eH@outlook.com" },
                                   new Engineer { ID = 11, FirstName = "Loe", LastName = "Hoy", EngineerInitials = "LH", Email = "lh@outlook.com" }
                        );
                        context.SaveChanges();
                    }



					// 3. Seed Sales Orders (Each customer gets 2)
					if (!context.SalesOrders.Any())
					{
						var today = DateTime.Today;
						var salesOrders = new List<SalesOrder>();

						var companies = new[]
                        {"FMI", "Rio Tinto Sorel", "Intradco", "United Taconite", "Direct Reduction Iron",
                    "Kumtor", "Owens Corning", "Coloured Aggregates", "Coast Aggregates", "Granite Works"};

						int idCounter = 1;
						int orderNumberBase = 10430000;

						foreach (var company in companies)
						{
							for (int j = 0; j < 3; j++) // 3 records per company
							{
								var fluctuation = (j % 3) * 2; // 0, 2, 4 pattern
								var soDate = today.AddDays(fluctuation);
								var appDwgExp = soDate.AddDays(2);
								var engPExp = appDwgExp.AddDays(17);

								salesOrders.Add(new SalesOrder
								{
									ID = idCounter++,
									OrderNumber = (orderNumberBase + idCounter).ToString(),
									SoDate = soDate,
									CompanyName = company,
									AppDwgExp = appDwgExp,
									EngPExp = engPExp
								});
							}
						}

						context.SalesOrders.AddRange(salesOrders);
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
