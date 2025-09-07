using System;
using Pos.Domain.Entities.Base;

namespace Pos.Domain.Entities
{
    /// <summary>
    /// Entidade Estoque - representa o estoque de um produto em uma localização
    /// </summary>
    public class Stock : Entity
    {
        public Guid ProductId { get; private set; }
        public Guid LocationId { get; private set; }
        public decimal Quantity { get; private set; }
        public decimal ReservedQuantity { get; private set; }
        public decimal AvailableQuantity => Quantity - ReservedQuantity;
        public decimal MinQuantity { get; private set; }
        public decimal MaxQuantity { get; private set; }
        public Guid TenantId { get; private set; }

        // Navigation properties
        public Product Product { get; private set; }
        public Location Location { get; private set; }

        private Stock() { } // EF Core

        public Stock(Guid productId, Guid locationId, decimal quantity, Guid tenantId)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("ID do produto não pode ser vazio", nameof(productId));
            
            if (locationId == Guid.Empty)
                throw new ArgumentException("ID da localização não pode ser vazio", nameof(locationId));

            ProductId = productId;
            LocationId = locationId;
            Quantity = quantity;
            TenantId = tenantId;
            ReservedQuantity = 0;
            MinQuantity = 0;
            MaxQuantity = decimal.MaxValue;
        }

        public void AddQuantity(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero", nameof(amount));

            Quantity += amount;
            MarkAsUpdated();
        }

        public void RemoveQuantity(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero", nameof(amount));

            if (amount > AvailableQuantity)
                throw new InvalidOperationException("Quantidade insuficiente em estoque");

            Quantity -= amount;
            MarkAsUpdated();
        }

        public void ReserveQuantity(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero", nameof(amount));

            if (amount > AvailableQuantity)
                throw new InvalidOperationException("Quantidade insuficiente disponível para reserva");

            ReservedQuantity += amount;
            MarkAsUpdated();
        }

        public void ReleaseReservation(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero", nameof(amount));

            if (amount > ReservedQuantity)
                throw new InvalidOperationException("Quantidade a liberar excede a quantidade reservada");

            ReservedQuantity -= amount;
            MarkAsUpdated();
        }

        public void ConfirmReservation(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero", nameof(amount));

            if (amount > ReservedQuantity)
                throw new InvalidOperationException("Quantidade a confirmar excede a quantidade reservada");

            ReservedQuantity -= amount;
            Quantity -= amount;
            MarkAsUpdated();
        }

        public void SetMinQuantity(decimal minQuantity)
        {
            if (minQuantity < 0)
                throw new ArgumentException("Quantidade mínima não pode ser negativa", nameof(minQuantity));

            MinQuantity = minQuantity;
            MarkAsUpdated();
        }

        public void SetMaxQuantity(decimal maxQuantity)
        {
            if (maxQuantity < 0)
                throw new ArgumentException("Quantidade máxima não pode ser negativa", nameof(maxQuantity));

            MaxQuantity = maxQuantity;
            MarkAsUpdated();
        }

        public bool IsBelowMinimum()
        {
            return Quantity < MinQuantity;
        }

        public bool IsAboveMaximum()
        {
            return Quantity > MaxQuantity;
        }

        public bool HasAvailableQuantity(decimal amount)
        {
            return AvailableQuantity >= amount;
        }
    }
}
