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

        public DbSet<GanttTask> GanttTasks { get; set; }
        public DbSet<GanttMilestone> GanttMilestones { get; set; }


        public DbSet<GanttData> GanttDatas { get; set; }



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

            modelBuilder.Entity<Engineer>()
             .HasIndex(m => m.Email)
             .IsUnique();

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

            ////Seed Data to run evertime the application starts
            //modelBuilder.Entity<Vendor>()
            // .HasData(
            //     new Vendor { ID = 1, Name = "HINGSTON METAL", Phone = "9056583456", Email = "purchaseorders@hingstonmetal.com" },
            //     new Vendor { ID = 2, Name = "PROCESS GROUP", Phone = "2876581056", Email = "orders@processgroup.com" },
            //     new Vendor { ID = 3, Name = "STEELWORKS INC.", Phone = "4167789988", Email = "sales@steelworks.com" },
            //     new Vendor { ID = 4, Name = "MINING SUPPLY CO.", Phone = "9056623344", Email = "info@miningsupply.com" },
            //     new Vendor { ID = 5, Name = "Rosta", Phone = "6478885540", Email = "orders@rosta.com" },
            //     new Vendor { ID = 6, Name = "VJ Pamensky", Phone = "6478885541", Email = "orders@vj-pamensky.com" },
            //     new Vendor { ID = 7, Name = "Precision MetalWorks", Phone = "6478885542", Email = "orders@precisionmw.com" },
            //     new Vendor { ID = 8, Name = "Teco-Westinghouse", Phone = "6478885543", Email = "orders@teco-westinghouse.com" },
            //     new Vendor { ID = 9, Name = "SKF Canada", Phone = "6478885544", Email = "orders@skfcanada.com" },
            //     new Vendor { ID = 10, Name = "METALZR PROFILES", Phone = "6478885545", Email = "orders@metalzrprofiles.com" },
            //     new Vendor { ID = 11, Name = "Right Machine", Phone = "6478885546", Email = "orders@rightmachine.com" },
            //     new Vendor { ID = 12, Name = "MEGA METALS", Phone = "6478885547", Email = "orders@megametals.com" },
            //     new Vendor { ID = 13, Name = "Tandem", Phone = "6478885548", Email = "orders@tandem.com" },
            //     new Vendor { ID = 14, Name = "Niagara Rubber Supply", Phone = "6478885549", Email = "orders@niagararubber.com" },
            //     new Vendor { ID = 15, Name = "Martin Sprocket", Phone = "6478885550", Email = "orders@martinsprocket.com" },
            //     new Vendor { ID = 16, Name = "HMFT Inc", Phone = "6478885551", Email = "orders@hmftinc.com" },
            //     new Vendor { ID = 17, Name = "HNG", Phone = "6478885552", Email = "orders@hng.com" },
            //     new Vendor { ID = 18, Name = "HBL", Phone = "6478885553", Email = "orders@hbl.com" },
            //     new Vendor { ID = 19, Name = "MAJOR WIRE", Phone = "6478885554", Email = "orders@majorwire.com" });

            //modelBuilder.Entity<Customer>()
            //    .HasData(
            //    new Customer { ID = 1, Phone = "4395509876", CompanyName = "Owens Corning" },
            //                new Customer { ID = 2, Phone = "4375509876", CompanyName = "Coloured Aggregates" },
            //                new Customer { ID = 3, Phone = "4162235566", CompanyName = "Bright Mining" },
            //                new Customer { ID = 4, Phone = "9058873321", CompanyName = "Rock Solid Ltd" },
            //                new Customer { ID = 5, Phone = "6479981123", CompanyName = "Granite Works" },
            //                new Customer { ID = 6, Phone = "6479901234", CompanyName = "FMI" },
            //                new Customer { ID = 7, Phone = "6479901235", CompanyName = "Teck Coal (EVR)" },
            //                new Customer { ID = 8, Phone = "6479901236", CompanyName = "Nutrien (PCS Lanigan)" },
            //                new Customer { ID = 9, Phone = "6479901237", CompanyName = "Commonwealth Equipment" },
            //                new Customer { ID = 10, Phone = "6479901238", CompanyName = "Rio Tinto Sorel" },
            //                new Customer { ID = 11, Phone = "6479901239", CompanyName = "Calidra La Laja (HBM)" },
            //                new Customer { ID = 12, Phone = "6479901240", CompanyName = "Coast Aggregates" },
            //                new Customer { ID = 13, Phone = "6479901241", CompanyName = "Direct Reduction Iron (HNG)" },
            //                new Customer { ID = 14, Phone = "6479901242", CompanyName = "Mosaic 4C (Motion)" },
            //                new Customer { ID = 15, Phone = "6479901243", CompanyName = "Lhoist NA Clifton, TX" },
            //                new Customer { ID = 16, Phone = "6479901244", CompanyName = "Kumtor Gold (HNG)" },
            //                new Customer { ID = 17, Phone = "6479901245", CompanyName = "Tehachapi Cement" });
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