using Microsoft.EntityFrameworkCore;
using Pos.Mobile.Maui.Models;

namespace Pos.Mobile.Maui.Data
{
    public class LocalDbContext : DbContext
    {
        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options)
        {
        }

        public DbSet<CachedProduct> Products { get; set; }
        public DbSet<CachedOrder> Orders { get; set; }
        public DbSet<CachedDashboard> Dashboards { get; set; }
        public DbSet<SyncQueue> SyncQueue { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar CachedProduct
            modelBuilder.Entity<CachedProduct>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Barcode).HasMaxLength(50);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.JsonData).IsRequired();
                entity.Property(e => e.LastSync).IsRequired();
            });

            // Configurar CachedOrder
            modelBuilder.Entity<CachedOrder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Number).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CustomerName).HasMaxLength(200);
                entity.Property(e => e.JsonData).IsRequired();
                entity.Property(e => e.LastSync).IsRequired();
            });

            // Configurar CachedDashboard
            modelBuilder.Entity<CachedDashboard>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalSales).HasColumnType("decimal(18,2)");
                entity.Property(e => e.JsonData).IsRequired();
                entity.Property(e => e.LastSync).IsRequired();
            });

            // Configurar SyncQueue
            modelBuilder.Entity<SyncQueue>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Operation).IsRequired().HasMaxLength(50);
                entity.Property(e => e.JsonData).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.RetryCount).HasDefaultValue(0);
            });

            // Índices para performance
            modelBuilder.Entity<CachedProduct>()
                .HasIndex(e => e.Barcode)
                .HasDatabaseName("IX_Products_Barcode");

            modelBuilder.Entity<CachedProduct>()
                .HasIndex(e => e.Name)
                .HasDatabaseName("IX_Products_Name");

            modelBuilder.Entity<CachedOrder>()
                .HasIndex(e => e.Number)
                .HasDatabaseName("IX_Orders_Number");

            modelBuilder.Entity<CachedOrder>()
                .HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_Orders_CreatedAt");

            modelBuilder.Entity<SyncQueue>()
                .HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_SyncQueue_CreatedAt");

            modelBuilder.Entity<SyncQueue>()
                .HasIndex(e => new { e.EntityType, e.Operation })
                .HasDatabaseName("IX_SyncQueue_EntityType_Operation");
        }
    }
}
