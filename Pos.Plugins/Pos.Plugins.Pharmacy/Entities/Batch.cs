using System;
using Pos.Domain.Entities.Base;
using Pos.Domain.Entities;

namespace Pos.Plugins.Pharmacy.Entities
{
    /// <summary>
    /// Entidade Lote - representa um lote de medicamento com validade
    /// </summary>
    public class Batch : Entity
    {
        public Guid ProductId { get; private set; }
        public string LotCode { get; private set; }
        public DateTime ExpiryDate { get; private set; }
        public decimal Quantity { get; private set; }
        public decimal ReservedQuantity { get; private set; }
        public decimal AvailableQuantity => Quantity - ReservedQuantity;
        public DateTime? ManufacturedDate { get; private set; }
        public string Manufacturer { get; private set; }
        public Guid TenantId { get; private set; }

        // Navigation properties
        public Product Product { get; private set; }

        private Batch() { } // EF Core

        public Batch(Guid productId, string lotCode, DateTime expiryDate, decimal quantity, Guid tenantId)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("ID do produto não pode ser vazio", nameof(productId));
            
            if (string.IsNullOrWhiteSpace(lotCode))
                throw new ArgumentException("Código do lote não pode ser vazio", nameof(lotCode));
            
            if (expiryDate <= DateTime.Today)
                throw new ArgumentException("Data de validade deve ser futura", nameof(expiryDate));
            
            if (quantity <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantity));

            ProductId = productId;
            LotCode = lotCode;
            ExpiryDate = expiryDate;
            Quantity = quantity;
            TenantId = tenantId;
            ReservedQuantity = 0;
        }

        public void UpdateQuantity(decimal quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("Quantidade não pode ser negativa", nameof(quantity));

            Quantity = quantity;
            MarkAsUpdated();
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
                throw new InvalidOperationException("Quantidade insuficiente no lote");

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

        public void SetManufacturer(string manufacturer, DateTime? manufacturedDate)
        {
            Manufacturer = manufacturer;
            ManufacturedDate = manufacturedDate;
            MarkAsUpdated();
        }

        public bool IsExpired()
        {
            return ExpiryDate <= DateTime.Today;
        }

        public bool IsExpiringSoon(int days = 30)
        {
            return ExpiryDate <= DateTime.Today.AddDays(days);
        }

        public int DaysUntilExpiry()
        {
            return (ExpiryDate - DateTime.Today).Days;
        }

        public bool HasAvailableQuantity(decimal amount)
        {
            return AvailableQuantity >= amount;
        }
    }
}
