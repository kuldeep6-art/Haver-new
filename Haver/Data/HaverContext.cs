using haver.Models;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace haver.Data
{
    public class HaverContext : DbContext
    {
        //To give access to IHttpContextAccessor for Audit Data with IAuditable
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Property to hold the UserName value
        public string UserName
        {
            get; private set;
        }

        public HaverContext(DbContextOptions<HaverContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            if (_httpContextAccessor.HttpContext != null)
            {
                //We have a HttpContext, but there might not be anyone Authenticated
                UserName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            }
            else
            {
                //No HttpContext so seeding data
                UserName = "Seed Data";
            }
        }

        public HaverContext(DbContextOptions<HaverContext> options)
            : base(options)
        {
            _httpContextAccessor = null!;
            UserName = "Seed Data";
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

            modelBuilder.Entity<Engineer>()
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

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is IAuditable trackable)
                {
                    var now = DateTime.UtcNow;
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;

                        case EntityState.Added:
                            trackable.CreatedOn = now;
                            trackable.CreatedBy = UserName;
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;
                    }
                }
            }
        }
    }

}