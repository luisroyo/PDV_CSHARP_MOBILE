using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pos.Domain.Entities;
using Pos.Domain.Interfaces;
using Pos.Domain.Events;
using Pos.Plugins.Pharmacy.Entities;
using Pos.Plugins.Pharmacy.Policies;
using Pos.Plugins.Pharmacy.Validations;
using Pos.Plugins.Pharmacy.Services;
using Pos.Plugins.Pharmacy.Handlers;

namespace Pos.Plugins.Pharmacy
{
    /// <summary>
    /// Plugin para vertical de Farmácia
    /// </summary>
    public class PharmacyPlugin : IVerticalPlugin
    {
        public string Key => "Pharmacy";
        public string Name => "Farmácia";
        public string Version => "1.0.0";

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Registra as políticas específicas da farmácia
            services.AddScoped<IPricingPolicy, PharmacyPricingPolicy>();
            services.AddScoped<IStockDecrementPolicy, PharmacyStockDecrementPolicy>();
            services.AddScoped<IOrderWorkflow, PharmacyOrderWorkflow>();
            services.AddScoped<IDiscountPolicy, PharmacyDiscountPolicy>();

            // Registra as validações específicas da farmácia
            services.AddScoped<IValidationRule, PrescriptionRequiredValidationRule>();
            services.AddScoped<IValidationRule, ExpiredBatchValidationRule>();

            // Registra os serviços específicos da farmácia
            services.AddScoped<IBatchService, BatchService>();
            services.AddScoped<IPrescriptionService, PrescriptionService>();
        }

        public void ConfigureModel(ModelBuilder modelBuilder)
        {
            // Configura as entidades específicas da farmácia
            modelBuilder.Entity<Batch>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LotCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ExpiryDate).IsRequired();
                entity.Property(e => e.Quantity).HasPrecision(18, 4);
                entity.HasIndex(e => new { e.ProductId, e.LotCode }).IsUnique();
            });

            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Number).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.IssuedAt).IsRequired();
            });

            modelBuilder.Entity<DrugInfo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ActiveIngredient).IsRequired().HasMaxLength(200);
                entity.Property(e => e.RequiresPrescription).IsRequired();
            });
        }

        public void RegisterPolicies(IVerticalPolicyRegistry registry)
        {
            registry.RegisterPricingPolicy<PharmacyPricingPolicy>();
            registry.RegisterStockDecrementPolicy<PharmacyStockDecrementPolicy>();
            registry.RegisterOrderWorkflow<PharmacyOrderWorkflow>();
            registry.RegisterDiscountPolicy<PharmacyDiscountPolicy>();
        }

        public void RegisterEventHandlers(IEventHandlerRegistry registry)
        {
            // Registra handlers específicos da farmácia
            registry.RegisterEventHandler<OrderCreatedEvent, PharmacyOrderCreatedHandler>();
            registry.RegisterEventHandler<StockUpdatedEvent, PharmacyStockUpdatedHandler>();
        }
    }
}
