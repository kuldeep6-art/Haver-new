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
        public DbSet<PackageRelease> PackageReleases { get; set; }
        public DbSet<SalesOrderEngineer> SalesOrderEngineers { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Procurement> Procurements { get; set; }

		public DbSet<MachineType> MachineTypes { get; set; }


		protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relationships with Cascade Delete Restriction


            modelBuilder.Entity<Machine>()
               .HasOne<SalesOrder>(so => so.SalesOrder)
               .WithMany(v => v.Machines)
               .HasForeignKey(so => so.SalesOrderID)
               .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<SalesOrderEngineer>()
                .HasOne<Engineer>(mse => mse.Engineer)
                .WithMany(e => e.SalesOrderEngineers)
                .HasForeignKey(mse => mse.EngineerID)
                .OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Procurement>()
			  .HasOne<Vendor>(mse => mse.Vendor)
			  .WithMany(e => e.Procurements)
			  .HasForeignKey(mse => mse.VendorID)
			  .OnDelete(DeleteBehavior.Restrict);



			modelBuilder.Entity<Machine>()
			  .HasOne<MachineType>(mse => mse.MachineType)
			  .WithMany(e => e.Machines)
			  .HasForeignKey(mse => mse.MachineTypeID)
			  .OnDelete(DeleteBehavior.Restrict);

			//unique fields

			modelBuilder.Entity<Customer>()
               .HasIndex(c => c.Phone)
               .IsUnique();

            modelBuilder.Entity<Vendor>()
               .HasIndex(c => c.Name)
               .IsUnique();

            modelBuilder.Entity<SalesOrder>()
                .HasIndex(so => so.OrderNumber)
                .IsUnique();

            modelBuilder.Entity<Machine>()
                .HasIndex(m => m.SerialNumber)
                .IsUnique();

            modelBuilder.Entity<Machine>()
               .HasIndex(m => m.ProductionOrderNumber)
               .IsUnique();

            //modelBuilder.Entity<Engineer>()
            // .HasIndex(m => m.Email)
            // .IsUnique();

            modelBuilder.Entity<MachineType>()
			 .HasIndex(m => m.Description)
			 .IsUnique();

            modelBuilder.Entity<Engineer>()
                .HasIndex(pr => new { pr.FirstName, pr.LastName })
                .IsUnique();

          

            modelBuilder.Entity<PackageRelease>()
                .HasIndex(pr => new { pr.Name, pr.SalesOrderID })
                .IsUnique();

            //composite keys for many to many     
            modelBuilder.Entity<SalesOrderEngineer>()
             .HasKey(mse => new { mse.SalesOrderID, mse.EngineerID });



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
        public DbSet<haver.Models.MachineType> MachineType { get; set; } = default!;
    }

}