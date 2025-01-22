using System.Diagnostics;
using haver.Models;
using Microsoft.EntityFrameworkCore;

namespace haver.Data
{
    public static class HaverInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using(var context =new HaverContext(
                serviceProvider.GetRequiredService<DbContextOptions<HaverContext>>()))
            {
                try
                {
                    //Seed data for customers, since we cant have Sales orders without Customers
                    if (!context.Customers.Any())
                    {
                        context.Customers.AddRange(
                            new Customer
                            {
                                FirstName = "Greg",
                                LastName = "Owenson",
                                Date = DateTime.Parse("2024-06-3"),
                                Phone = "4375509876",
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


                    //Seed data for sales orders
                    if (!context.SalesOrders.Any())
                    {
                        context.SalesOrders.AddRange(
                        new SalesOrder
                        {
                            OrderNumber = "10439607",
                            SoDate = DateTime.Parse("2024-07-23"),
                            ShippingTerms = "The seller is responsible for all risks and costs involed in delivering this order to the customer.",
                            AppDwgRcd = DateTime.Parse("2024-07-17"),
                            DwgIsDt = DateTime.Parse("2024-07-12"),
                            CustomerID = context.Customers.FirstOrDefault(static d => d.CompanyName == "Owens Corning").ID
                        },
                        new SalesOrder
                        {
                            OrderNumber = "10431342",
                            SoDate = DateTime.Parse("2024-06-19"),
                            ShippingTerms = "Both seller and customer have aggreed that machines shoud be delivered to London, Ontario and the customer is respnsible for the goods",
                            AppDwgRcd = DateTime.Parse("2024-06-14"),
                            DwgIsDt = DateTime.Parse("2024-06-12"),
                            CustomerID = context.Customers.FirstOrDefault(static d => d.CompanyName == "Coloured Aggregates").ID
                        });
                        context.SaveChanges();
                    }

                    //Seed data for Vendors
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

                    //Seed datta for machine
                    if (!context.Machines.Any())
                    {
                        context.Machines.AddRange(
                            new Machine
                            {
                                Description = "Heavy-duty industrial cutting machine, used for precision metal cutting",
                                SerialNumber = "HDCM-12345",
                                Quantity = 3,
                                Size = "Large",
                                Class = "Industrial",
                                SizeDeck = "1500x2000",
                            },
                            new Machine
                            {
                                Description = "Precision milling machine for high-accuracy part fabrication",
                                SerialNumber = "PM-54321",
                                Quantity = 5,
                                Size = "Medium",
                                Class = "Fabrication",
                                SizeDeck = "1000x1500",
                            },
                            new Machine
                            {
                                Description = "Automated welding machine for large-scale manufacturing",
                                SerialNumber = "AWM-98765",
                                Quantity = 2,
                                Size = "Large",
                                Class = "Welding",
                                SizeDeck = "2000x3000",
                            },
                            new Machine
                            {
                                Description = "Compact CNC machine for small part production",
                                SerialNumber = "CNC-24680",
                                Quantity = 10,
                                Size = "Small",
                                Class = "CNC",
                                SizeDeck = "500x750",
                            });
                        context.SaveChanges();
                    }

                    //Seed data for notes
                    if (!context.Notes.Any())
                    {
                        var machineSchedule1 = context.MachineSchedules.FirstOrDefault(ms => ms.ID == 1);
                        var machineSchedule2 = context.MachineSchedules.FirstOrDefault(ms => ms.ID == 2);

                        if (machineSchedule1 != null)
                        {
                            context.Notes.Add(new Note
                            {
                                PreOrder = "Pre-order for machine assembly.",
                                Scope = "Assembly of base components.",
                                AssemblyHours = 15.5m,  // Example decimal value for hours
                                ReworkHours = 2.0m,     // Example decimal value for hours
                                BudgetHours = 20.0m,    // Example decimal value for hours
                                NamePlate = NamePlate.Required,  // Using enum value for NamePlate
                                MachineSchedule = machineSchedule1  // Associating with a MachineSchedule
                            });
                        }

                        if (machineSchedule2 != null)
                        {
                            context.Notes.Add(new Note
                            {
                                PreOrder = "Pre-order for advanced assembly.",
                                Scope = "Assembly of additional components.",
                                AssemblyHours = 18.0m,  // Example decimal value for hours
                                ReworkHours = 3.5m,     // Example decimal value for hours
                                BudgetHours = 22.0m,    // Example decimal value for hours
                                NamePlate = NamePlate.Received,  // Using enum value for NamePlate
                                MachineSchedule = machineSchedule2  // Associating with a MachineSchedule
                            });
                        }
                        context.SaveChanges();
                    }

                    //Seed data for machine schedule
                    if (!context.MachineSchedules.Any())
                    {
                        var packageRelease1 = context.PackageReleases.FirstOrDefault(pr => pr.Name == "Package 101 Release");
                        var packageRelease2 = context.PackageReleases.FirstOrDefault(pr => pr.Name == "Package 102 Release");

                        context.MachineSchedules.AddRange(
                            new MachineSchedule
                            {
                                StartDate = DateTime.Now,
                                DueDate = DateTime.Parse("2024-07-15"),
                                EndDate = DateTime.Parse("2024-07-20"),
                                PackageRDate = DateTime.Parse("2024-07-16"),
                                PODueDate = DateTime.Parse("2024-07-17"),
                                DeliveryDate = DateTime.Parse("2024-07-21"),
                                Media = true, 
                                SpareParts = false,
                                SparePMedia = true, 
                                Base = true, 
                                AirSeal = true, 
                                CoatingLining = true, 
                                Dissembly = false,
                                NoteID = context.Notes.FirstOrDefault().ID, 
                                MachineID = context.Machines.FirstOrDefault().ID, 
                                PackageRelease = packageRelease1
                            },
                            new MachineSchedule
                            {
                                StartDate = DateTime.Now,
                                DueDate = DateTime.Parse("2024-08-01"),
                                EndDate = DateTime.Parse("2024-08-05"),
                                PackageRDate = DateTime.Parse("2024-08-02"),
                                PODueDate = DateTime.Parse("2024-08-03"),
                                DeliveryDate = DateTime.Parse("2024-08-06"),
                                Media = false,
                                SpareParts = true,
                                SparePMedia = false,
                                Base = false,
                                AirSeal = true,
                                CoatingLining = true,
                                Dissembly = true, 
                                NoteID = context.Notes.Skip(1).FirstOrDefault().ID, 
                                MachineID = context.Machines.Skip(1).FirstOrDefault().ID, 
                                PackageRelease = packageRelease2
                            });
                        context.SaveChanges();
                    }

                    

                    //Seed data for Package Release class
                    if (!context.PackageReleases.Any())
                    {
                        var machineSchedule1 = context.MachineSchedules.FirstOrDefault(ms => ms.DueDate == DateTime.Parse("2024-07-15"));
                        var machineSchedule2 = context.MachineSchedules.FirstOrDefault(ms => ms.DueDate == DateTime.Parse("2024-08-01"));

                        context.PackageReleases.AddRange(
                            new PackageRelease
                            {
                                Name = "Package 101 Release",
                                PReleaseDateP = DateTime.Parse("2024-07-01"),
                                PReleaseDateA = DateTime.Parse("2024-07-05"),
                                Notes = "This is the initial package release for engineering with all necessary components.",
                                MachineScheduleID = machineSchedule1?.ID?? 1
                            },
                            new PackageRelease
                            {
                                Name = "Package 102 Release",
                                PReleaseDateP = DateTime.Parse("2024-08-01"),
                                PReleaseDateA = DateTime.Parse("2024-08-03"),
                                Notes = "Updated package release with some components changed, ready for next phase.",
                                MachineScheduleID = machineSchedule2?.ID ?? 2
                            },
                            new PackageRelease
                            {
                                Name = "Package 103 Release",
                                PReleaseDateP = DateTime.Parse("2024-09-01"),
                                PReleaseDateA = DateTime.Parse("2024-09-05"),
                                Notes = "Final packaging release for the quarter with all final approvals.",
                                MachineScheduleID = machineSchedule2?.ID ?? 3
                            });
                        context.SaveChanges();
                    }

                    //seed data for Engineer
                    if (!context.Engineers.Any())
                    {
                        context.Engineers.AddRange(
                            new Engineer
                            {
                                FirstName = "John",
                                LastName = "Doe",
                                Phone = "1234567890",
                                Email = "johndoe@example.com"
                            },
                            new Engineer
                            {
                                FirstName = "Jane",
                                LastName = "Smith",
                                Phone = "2345678901",
                                Email = "janesmith@example.com"
                            },
                            new Engineer
                            {
                                FirstName = "David",
                                LastName = "Johnson",
                                Phone = "3456789012",
                                Email = "davidjohnson@example.com"
                            },
                            new Engineer
                            {
                                FirstName = "Emily",
                                LastName = "Davis",
                                Phone = "4567890123",
                                Email = "emilydavis@example.com"
                            },
                            new Engineer
                            {
                                FirstName = "Michael",
                                LastName = "Williams",
                                Phone = "5678901234",
                                Email = "michaelwilliams@example.com"
                            }
                        );
                        context.SaveChanges();
                    }

                    //Seed data for Machine scheduling engineer class
                    if (!context.MachineScheduleEngineers.Any())
                    {
                        var schedule1 = context.MachineSchedules.FirstOrDefault(s => s.ID == 1);
                        var schedule2 = context.MachineSchedules.FirstOrDefault(s => s.ID == 2);
                        var engineer1 = context.Engineers.FirstOrDefault(e => e.ID == 1);
                        var engineer2 = context.Engineers.FirstOrDefault(e => e.ID == 2);

                        if (schedule1 != null && engineer1 != null)
                        {
                            context.MachineScheduleEngineers.Add(new MachineScheduleEngineer
                            {
                                MachineScheduleID = schedule1.ID,
                                EngineerID = engineer1.ID
                            });
                        }

                        if (schedule2 != null && engineer2 != null)
                        {
                            context.MachineScheduleEngineers.Add(new MachineScheduleEngineer
                            {
                                MachineScheduleID = schedule2.ID,
                                EngineerID = engineer2.ID
                            });
                        }

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
