using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CedarDelivery.Models;

namespace CedarDelivery.Data
{
    // EF Core database context. Inherits the Identity tables (AspNetUsers, AspNetRoles, ...).
    //
    // Delete rules used below:
    // - Cascade:  child rows only make sense with their parent (a user's addresses, an order's items).
    // - Restrict: keep history safe. A user or store that has orders can't be deleted;
    //             deactivate it instead (IsActive / IsOpen).
    // SQL Server also refuses two cascade paths to the same table, which is another
    // reason some relationships use Restrict.
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<DriverProfile> DriverProfiles { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<RoleApplication> RoleApplications { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        // Every decimal is money by default: decimal(18,2).
        // Coordinates override this in OnModelCreating.
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------- Users and addresses ----------

            modelBuilder.Entity<Address>()
                .HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------- Stores and products ----------

            modelBuilder.Entity<Store>()
                .HasOne(s => s.Owner)
                .WithMany(u => u.Stores)
                .HasForeignKey(s => s.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductCategory>()
                .HasOne(c => c.Store)
                .WithMany(s => s.ProductCategories)
                .HasForeignKey(c => c.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Store)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict: a category with products must be emptied before it is deleted.
            modelBuilder.Entity<Product>()
                .HasOne(p => p.ProductCategory)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.ProductCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- Cart ----------

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // The same product appears once per cart; adding it again raises Quantity.
            modelBuilder.Entity<CartItem>()
                .HasIndex(c => new { c.UserId, c.ProductId })
                .IsUnique();

            // ---------- Orders ----------

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Store)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // If a product is deleted, old order items keep their copied name and price.
            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(h => h.Order)
                .WithMany(o => o.StatusHistory)
                .HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- Drivers and deliveries ----------

            modelBuilder.Entity<DriverProfile>()
                .HasOne(d => d.User)
                .WithOne(u => u.DriverProfile)
                .HasForeignKey<DriverProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.Order)
                .WithOne(o => o.Delivery)
                .HasForeignKey<Delivery>(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.DriverProfile)
                .WithMany(p => p.Deliveries)
                .HasForeignKey(d => d.DriverProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- Role applications ----------

            modelBuilder.Entity<RoleApplication>()
                .HasOne(a => a.User)
                .WithMany(u => u.RoleApplications)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RoleApplication>()
                .HasOne(a => a.ReviewedByUser)
                .WithMany()
                .HasForeignKey(a => a.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- Reviews ----------

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Order)
                .WithMany(o => o.Reviews)
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Store)
                .WithMany(s => s.Reviews)
                .HasForeignKey(r => r.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.DriverProfile)
                .WithMany(d => d.Reviews)
                .HasForeignKey(r => r.DriverProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // At most one store review and one driver review per order.
            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.OrderId, r.TargetType })
                .IsUnique();

            // ---------- Notifications and activity log ----------

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ActivityLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------- Coordinates: decimal(9,6) ≈ 10 cm accuracy ----------

            modelBuilder.Entity<Address>().Property(a => a.Latitude).HasPrecision(9, 6);
            modelBuilder.Entity<Address>().Property(a => a.Longitude).HasPrecision(9, 6);
            modelBuilder.Entity<Store>().Property(s => s.Latitude).HasPrecision(9, 6);
            modelBuilder.Entity<Store>().Property(s => s.Longitude).HasPrecision(9, 6);
            modelBuilder.Entity<Order>().Property(o => o.DeliveryLatitude).HasPrecision(9, 6);
            modelBuilder.Entity<Order>().Property(o => o.DeliveryLongitude).HasPrecision(9, 6);
            modelBuilder.Entity<DriverProfile>().Property(d => d.CurrentLatitude).HasPrecision(9, 6);
            modelBuilder.Entity<DriverProfile>().Property(d => d.CurrentLongitude).HasPrecision(9, 6);
        }
    }
}
