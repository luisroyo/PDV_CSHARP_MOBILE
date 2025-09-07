using Microsoft.EntityFrameworkCore;
using Pos.Domain.Entities;

namespace Pos.Api.Data
{
    public class PosDbContext : DbContext
    {
        public PosDbContext(DbContextOptions<PosDbContext> options) : base(options)
        {
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Stock> Stock { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<OutboxEvent> OutboxEvents { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<OrderPayment> OrderPayments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações básicas - será expandido quando necessário
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Stock>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<OutboxEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<OrderPayment>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }
    }
}