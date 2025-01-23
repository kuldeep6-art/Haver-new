using haver.Models;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace haver.Data
{
    public class HaverContext : DbContext
    {
        public HaverContext(DbContextOptions<HaverContext> options)
           : base(options)
        {
        }

        // DbSets for all tables
        public DbSet<Customer> Customers { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<Engineer> Engineers { get; set; }
        public DbSet<MachineSchedule> MachineSchedules { get; set; }
        public DbSet<PackageRelease> PackageReleases { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<MachineScheduleEngineer> MachineScheduleEngineers { get; set; }
        public DbSet<Vendor> Vendors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relationships with Cascade Delete Restriction
            modelBuilder.Entity<SalesOrder>()
                .HasOne<Customer>(so => so.Customer)
                .WithMany(c => c.SalesOrders)
                .HasForeignKey(so => so.CustomerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesOrder>()
                .HasOne<Vendor>(so => so.Vendor)
                .WithMany(v => v.SalesOrders)
                .HasForeignKey(so => so.VendorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MachineSchedule>()
               .HasOne<Machine>(so => so.Machine)
               .WithMany(v => v.MachineSchedules)
               .HasForeignKey(so => so.MachineID)
               .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<MachineScheduleEngineer>()
                .HasOne<Engineer>(mse => mse.Engineer)
                .WithMany(e => e.MachineScheduleEngineers)
                .HasForeignKey(mse => mse.EngineerID)
                .OnDelete(DeleteBehavior.Restrict);


            //unique fields

            modelBuilder.Entity<Customer>()
               .HasIndex(c => c.Phone)
               .IsUnique();

            modelBuilder.Entity<SalesOrder>()
                .HasIndex(so => so.OrderNumber)
                .IsUnique();

            modelBuilder.Entity<Machine>()
                .HasIndex(m => m.SerialNumber)
                .IsUnique();


            modelBuilder.Entity<MachineScheduleEngineer>()
                .HasIndex(mse => new { mse.MachineScheduleID, mse.EngineerID })
                .IsUnique();

            modelBuilder.Entity<PackageRelease>()
                .HasIndex(pr => new { pr.Name, pr.MachineScheduleID })
                .IsUnique();

            //composite keys for many to many     

           modelBuilder.Entity<MachineScheduleEngineer>()
            .HasKey(mse => new { mse.MachineScheduleID, mse.EngineerID });



        }
    }

}