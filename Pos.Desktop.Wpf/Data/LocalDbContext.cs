using Microsoft.EntityFrameworkCore;
using Pos.Desktop.Wpf.Models;

namespace Pos.Desktop.Wpf.Data
{
    public class LocalDbContext : DbContext
    {
        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options)
        {
        }

        public DbSet<CachedProduct> CachedProducts { get; set; }
        public DbSet<CachedOrder> CachedOrders { get; set; }
        public DbSet<CachedOrderItem> CachedOrderItems { get; set; }
        public DbSet<SyncQueue> SyncQueue { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CachedProduct
            modelBuilder.Entity<CachedProduct>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Sku).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Barcode).HasMaxLength(50);
                entity.Property(e => e.Unit).HasMaxLength(20);
                entity.Property(e => e.LastSync).IsRequired();
            });

            // CachedOrder
            modelBuilder.Entity<CachedOrder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Number).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Total).HasPrecision(18, 2);
                entity.Property(e => e.CustomerName).HasMaxLength(200);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.LastSync).IsRequired();
                entity.Property(e => e.IsPendingSync).IsRequired();
            });

            // CachedOrderItem
            modelBuilder.Entity<CachedOrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ProductSku).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.LastSync).IsRequired();

                entity.HasOne(e => e.Order)
                    .WithMany(o => o.Items)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // SyncQueue
            modelBuilder.Entity<SyncQueue>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Operation).IsRequired().HasMaxLength(20);
                entity.Property(e => e.EntityId).IsRequired();
                entity.Property(e => e.EntityData).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.RetryCount).IsRequired();
            });
        }
    }
}
