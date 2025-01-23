using System.Diagnostics;
using haver.Models;
using Microsoft.EntityFrameworkCore;

namespace haver.Data
{
    public static class HaverInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new HaverContext(
                serviceProvider.GetRequiredService<DbContextOptions<HaverContext>>()))
            {
                try
                {
                    // 1. Seed Customers
                    if (!context.Customers.Any())
                    {
                        context.Customers.AddRange(
                            new Customer
                            {
                                FirstName = "Greg",
                                LastName = "Owenson",
                                Date = DateTime.Parse("2024-06-03"),
                                Phone = "4395509876",
                                CompanyName = "Owens Corning"
                            },
                            new Customer
                            {
                                FirstName = "Phill",
                                LastName = "Morgan",
                                Date = DateTime.Parse("2024-06-30"),
                                Phone = "4375509876",
                                CompanyName = "Coloured Aggregates"
                            });
                        context.SaveChanges();
                    }

                    // 2. Seed Vendors
                    if (!context.Vendors.Any())
                    {
                        context.Vendors.AddRange(
                            new Vendor
                            {
                                Name = Name.HINGSTONMETAL,
                                Phone = "9056583456",
                                Email = "purchaseorders@precisionmetals.com"
                            },
                            new Vendor
                            {
                                Name = Name.PROCESSGROUP,
                                Phone = "2876581056",
                                Email = "purchaseorders@hingstonmetal.com"
                            });
                        context.SaveChanges();
                    }

                    // 3. Seed Machines
                    if (!context.Machines.Any())
                    {
                        context.Machines.AddRange(
                            new Machine
                            {
                                Description = "T-330 4'x10' - 1D",
                                SerialNumber = "HDCM-12345",
                                Quantity = 3,
                                Size = "Large",
                                Class = "Industrial",
                                SizeDeck = "1500x2000"
                            },
                            new Machine
                            {
                                Description = "F-600 5'x10' - 1D",
                                SerialNumber = "PM-54321",
                                Quantity = 5,
                                Size = "Medium",
                                Class = "Fabrication",
                                SizeDeck = "1000x1500"
                            });
                        context.SaveChanges();
                    }

                    // 4. Seed Engineers
                    if (!context.Engineers.Any())
                    {
                        context.Engineers.AddRange(
                            new Engineer { FirstName = "John", LastName = "Doe", Phone = "1234567890", Email = "johndoe@example.com" },
                            new Engineer { FirstName = "Jane", LastName = "Smith", Phone = "2345678901", Email = "janesmith@example.com" }
                        );
                        context.SaveChanges();
                    }

                    // 5. Seed Notes
                    if (!context.Notes.Any())
                    {
                        context.Notes.AddRange(
                            new Note
                            {
                                PreOrder = "Pre-order for machine assembly.",
                                Scope = "Assembly of base components.",
                                AssemblyHours = 15.5m,
                                ReworkHours = 2.0m,
                                BudgetHours = 20.0m,
                                NamePlate = NamePlate.Required
                            },
                            new Note
                            {
                                PreOrder = "Pre-order for advanced assembly.",
                                Scope = "Assembly of additional components.",
                                AssemblyHours = 18.0m,
                                ReworkHours = 3.5m,
                                BudgetHours = 22.0m,
                                NamePlate = NamePlate.Received
                            });
                        context.SaveChanges();
                    }

                    // 6. Seed Machine Schedules
                    if (!context.MachineSchedules.Any())
                    {
                        var machineSchedule = new MachineSchedule
                        {
                            StartDate = DateTime.Now,
                            DueDate = DateTime.Parse("2024-07-15"),
                            EndDate = DateTime.Parse("2024-07-20"),
                            PackageRDate = DateTime.Parse("2024-07-16"),
                            PODueDate = DateTime.Parse("2024-07-17"),
                            DeliveryDate = DateTime.Parse("2024-07-21"),
                            Media = true,
                            SpareParts = false,
                            NoteID = context.Notes.FirstOrDefault().ID, // Make sure Notes exists
                            MachineID = context.Machines.FirstOrDefault().ID // Make sure Machines exist
                        };
                        context.MachineSchedules.Add(machineSchedule);
                        context.SaveChanges();
                    }

                    // 7. Seed Package Releases
                    if (!context.PackageReleases.Any())
                    {
                        var packageRelease = new PackageRelease
                        {
                            MachineScheduleID = context.MachineSchedules.FirstOrDefault().ID, // Link to MachineSchedule
                            Name = "Package 101 Release",
                            PReleaseDateP = DateTime.Parse("2024-07-01"),
                            PReleaseDateA = DateTime.Parse("2024-07-05"),
                            Notes = "Initial package release."
                        };
                        context.PackageReleases.Add(packageRelease);
                        context.SaveChanges();
                    }

                    // 8. Seed Sales Orders (with valid MachineScheduleID and VendorID)
                    if (!context.SalesOrders.Any())
                    {
                        var salesOrder = new SalesOrder
                        {
                            OrderNumber = "10439607",
                            SoDate = DateTime.Parse("2024-07-23"),
                            ShippingTerms = "Delivered to the customer.",
                            AppDwgRcd = DateTime.Parse("2024-07-17"),
                            DwgIsDt = DateTime.Parse("2024-07-12"),
                            CustomerID = context.Customers.FirstOrDefault(c => c.CompanyName == "Owens Corning").ID,
                            MachineScheduleID = context.MachineSchedules.FirstOrDefault().ID, // Make sure MachineScheduleID exists
                            VendorID = context.Vendors.FirstOrDefault().ID // Make sure VendorID exists
                        };
                        context.SalesOrders.Add(salesOrder);
                        context.SaveChanges();
                    }

                    // 9. Seed Machine Schedule Engineers (with valid MachineScheduleID and EngineerID)
                    if (!context.MachineScheduleEngineers.Any())
                    {
                        var machineScheduleEngineer = new MachineScheduleEngineer
                        {
                            MachineScheduleID = context.MachineSchedules.FirstOrDefault().ID, // Make sure MachineScheduleID exists
                            EngineerID = context.Engineers.FirstOrDefault().ID // Make sure EngineerID exists
                        };
                        context.MachineScheduleEngineers.Add(machineScheduleEngineer);
                        context.SaveChanges();
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.GetBaseException().Message);
                }
            }
        }
    }
}
