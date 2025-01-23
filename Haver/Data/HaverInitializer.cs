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

                    // 4. Seed Machine Schedules
                    if (!context.MachineSchedules.Any())
                    {
                        context.MachineSchedules.Add(new MachineSchedule
                        {
                            StartDate = DateTime.Now,
                            DueDate = DateTime.Parse("2024-07-15"),
                            EndDate = DateTime.Parse("2024-07-20"),
                            PackageRDate = DateTime.Parse("2024-07-16"),
                            PODueDate = DateTime.Parse("2024-07-17"),
                            DeliveryDate = DateTime.Parse("2024-07-21"),
                            Media = true,
                            SpareParts = false,
                            MachineID = context.Machines.FirstOrDefault().ID
                        });
                        context.SaveChanges();
                    }

                    // 5. Seed Notes (One-to-One with MachineSchedule)
                    if (!context.Notes.Any())
                    {
                        var machineSchedule = context.MachineSchedules.FirstOrDefault();
                        if (machineSchedule != null)
                        {
                            context.Notes.Add(new Note
                            {
                                PreOrder = "Pre-order for machine assembly.",
                                Scope = "Assembly of base components.",
                                AssemblyHours = 15.5m,
                                ReworkHours = 2.0m,
                                BudgetHours = 20.0m,
                                NamePlate = NamePlate.Required,
                                MachineScheduleID = machineSchedule.ID
                            });
                            context.SaveChanges();
                        }
                    }

                    // 6. Seed Package Releases (One-to-One with MachineSchedule)
                    if (!context.PackageReleases.Any())
                    {
                        var machineSchedule = context.MachineSchedules.FirstOrDefault();
                        if (machineSchedule != null)
                        {
                            context.PackageReleases.Add(new PackageRelease
                            {
                                MachineScheduleID = machineSchedule.ID,
                                Name = "Package 101 Release",
                                PReleaseDateP = DateTime.Parse("2024-07-01"),
                                PReleaseDateA = DateTime.Parse("2024-07-05"),
                                Notes = "Initial package release."
                            });
                            context.SaveChanges();
                        }
                    }

                    // 7. Seed Sales Orders
                    if (!context.SalesOrders.Any())
                    {
                        var customer = context.Customers.FirstOrDefault(c => c.CompanyName == "Owens Corning");
                        var machineSchedule = context.MachineSchedules.FirstOrDefault();
                        var vendor = context.Vendors.FirstOrDefault();

                        if (customer != null && machineSchedule != null && vendor != null)
                        {
                            context.SalesOrders.Add(new SalesOrder
                            {
                                OrderNumber = "10439607",
                                SoDate = DateTime.Parse("2024-07-23"),
                                ShippingTerms = "Delivered to the customer.",
                                AppDwgRcd = DateTime.Parse("2024-07-17"),
                                DwgIsDt = DateTime.Parse("2024-07-12"),
                                CustomerID = customer.ID,
                                MachineScheduleID = machineSchedule.ID,
                                VendorID = vendor.ID
                            });
                            context.SaveChanges();
                        }
                    }

                    // 8. Seed Machine Schedule Engineers
                    if (!context.MachineScheduleEngineers.Any())
                    {
                        var machineSchedule = context.MachineSchedules.FirstOrDefault();
                        var engineer = context.Engineers.FirstOrDefault();

                        if (machineSchedule != null && engineer != null)
                        {
                            context.MachineScheduleEngineers.Add(new MachineScheduleEngineer
                            {
                                MachineScheduleID = machineSchedule.ID,
                                EngineerID = engineer.ID
                            });
                            context.SaveChanges();
                        }
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
