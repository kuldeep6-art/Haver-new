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
                                CompanyName = "Owens Corning"
                            },
                            new Customer
                            {
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
                            Name = (Name?)2,
                            Phone = "9056583456",
                            Email = "purchaseorders@precisionmetals.com"
                        },
                        new Vendor
                        {
                            Name = (Name?)0,
                            Phone = "2876581056",
                            Email = "purchaseorders@hingstonmetal.com"
                        });
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
