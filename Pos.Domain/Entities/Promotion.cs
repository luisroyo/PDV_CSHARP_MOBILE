using System;
using Pos.Domain.Entities.Base;

namespace Pos.Domain.Entities
{
    /// <summary>
    /// Entidade Promoção - representa uma promoção ou desconto no sistema
    /// </summary>
    public class Promotion : Entity
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public PromotionType Type { get; private set; }
        public string RulesJson { get; private set; } // Regras específicas da promoção
        public decimal DiscountValue { get; private set; }
        public decimal? MinOrderValue { get; private set; }
        public decimal? MaxDiscountAmount { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public bool Active { get; private set; }
        public int? MaxUses { get; private set; }
        public int CurrentUses { get; private set; }
        public Guid TenantId { get; private set; }

        private Promotion() { } // EF Core

        public Promotion(string name, PromotionType type, decimal discountValue, DateTime startDate, DateTime endDate, Guid tenantId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome não pode ser vazio", nameof(name));
            
            if (discountValue <= 0)
                throw new ArgumentException("Valor do desconto deve ser maior que zero", nameof(discountValue));
            
            if (startDate >= endDate)
                throw new ArgumentException("Data de início deve ser anterior à data de fim", nameof(startDate));

            Name = name;
            Type = type;
            DiscountValue = discountValue;
            StartDate = startDate;
            EndDate = endDate;
            TenantId = tenantId;
            Active = true;
            CurrentUses = 0;
            RulesJson = "{}";
        }

        public void UpdateBasicInfo(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome não pode ser vazio", nameof(name));

            Name = name;
            Description = description;
            MarkAsUpdated();
        }

        public void UpdateDiscount(decimal discountValue, decimal? maxDiscountAmount)
        {
            if (discountValue <= 0)
                throw new ArgumentException("Valor do desconto deve ser maior que zero", nameof(discountValue));

            DiscountValue = discountValue;
            MaxDiscountAmount = maxDiscountAmount;
            MarkAsUpdated();
        }

        public void UpdateValidity(DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
                throw new ArgumentException("Data de início deve ser anterior à data de fim", nameof(startDate));

            StartDate = startDate;
            EndDate = endDate;
            MarkAsUpdated();
        }

        public void SetMinOrderValue(decimal? minOrderValue)
        {
            if (minOrderValue.HasValue && minOrderValue < 0)
                throw new ArgumentException("Valor mínimo do pedido não pode ser negativo", nameof(minOrderValue));

            MinOrderValue = minOrderValue;
            MarkAsUpdated();
        }

        public void SetMaxUses(int? maxUses)
        {
            if (maxUses.HasValue && maxUses <= 0)
                throw new ArgumentException("Número máximo de usos deve ser maior que zero", nameof(maxUses));

            MaxUses = maxUses;
            MarkAsUpdated();
        }

        public void UpdateRules(string rulesJson)
        {
            if (string.IsNullOrWhiteSpace(rulesJson))
                throw new ArgumentException("Regras não podem ser vazias", nameof(rulesJson));

            RulesJson = rulesJson;
            MarkAsUpdated();
        }

        public void Activate()
        {
            Active = true;
            MarkAsUpdated();
        }

        public void Deactivate()
        {
            Active = false;
            MarkAsUpdated();
        }

        public void RecordUse()
        {
            if (MaxUses.HasValue && CurrentUses >= MaxUses.Value)
                throw new InvalidOperationException("Promoção atingiu o limite máximo de usos");

            CurrentUses++;
            MarkAsUpdated();
        }

        public bool IsValid()
        {
            var now = DateTime.UtcNow;
            return Active && 
                   now >= StartDate && 
                   now <= EndDate && 
                   (!MaxUses.HasValue || CurrentUses < MaxUses.Value);
        }

        public bool CanApplyToOrder(decimal orderValue)
        {
            if (!IsValid())
                return false;

            if (MinOrderValue.HasValue && orderValue < MinOrderValue.Value)
                return false;

            return true;
        }

        public decimal CalculateDiscount(decimal orderValue)
        {
            if (!CanApplyToOrder(orderValue))
                return 0;

            decimal discount = Type switch
            {
                PromotionType.Percentage => orderValue * (DiscountValue / 100),
                PromotionType.FixedAmount => DiscountValue,
                _ => 0
            };

            if (MaxDiscountAmount.HasValue && discount > MaxDiscountAmount.Value)
                discount = MaxDiscountAmount.Value;

            return Math.Min(discount, orderValue);
        }
    }

    public enum PromotionType
    {
        Percentage,     // Desconto percentual
        FixedAmount,    // Desconto em valor fixo
        MixAndMatch,    // Leve X pague Y
        Combo,          // Combo de produtos
        HappyHour       // Promoção por horário
    }
}
