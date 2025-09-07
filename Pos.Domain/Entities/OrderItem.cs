using System;
using Pos.Domain.Entities.Base;

namespace Pos.Domain.Entities
{
    /// <summary>
    /// Item do pedido - representa um produto específico em um pedido
    /// </summary>
    public class OrderItem : Entity
    {
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public string ProductName { get; private set; }
        public string ProductSku { get; private set; }
        public decimal Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal Subtotal { get; private set; }
        public decimal? DiscountAmount { get; private set; }
        public string Notes { get; private set; }

        // Navigation properties
        public Product Product { get; private set; }

        private OrderItem() { } // EF Core

        public OrderItem(Guid productId, string productName, string productSku, decimal quantity, decimal unitPrice)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("ID do produto não pode ser vazio", nameof(productId));
            
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("Nome do produto não pode ser vazio", nameof(productName));
            
            if (quantity <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantity));
            
            if (unitPrice < 0)
                throw new ArgumentException("Preço unitário não pode ser negativo", nameof(unitPrice));

            ProductId = productId;
            ProductName = productName;
            ProductSku = productSku;
            Quantity = quantity;
            UnitPrice = unitPrice;
            Subtotal = quantity * unitPrice;
        }

        public void SetOrderId(Guid orderId)
        {
            if (orderId == Guid.Empty)
                throw new ArgumentException("ID do pedido não pode ser vazio", nameof(orderId));

            OrderId = orderId;
        }

        public void UpdateQuantity(decimal quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantity));

            Quantity = quantity;
            RecalculateSubtotal();
            MarkAsUpdated();
        }

        public void IncreaseQuantity(decimal additionalQuantity)
        {
            if (additionalQuantity <= 0)
                throw new ArgumentException("Quantidade adicional deve ser maior que zero", nameof(additionalQuantity));

            Quantity += additionalQuantity;
            RecalculateSubtotal();
            MarkAsUpdated();
        }

        public void DecreaseQuantity(decimal quantityToRemove)
        {
            if (quantityToRemove <= 0)
                throw new ArgumentException("Quantidade a remover deve ser maior que zero", nameof(quantityToRemove));

            if (quantityToRemove >= Quantity)
                throw new ArgumentException("Quantidade a remover não pode ser maior ou igual à quantidade atual");

            Quantity -= quantityToRemove;
            RecalculateSubtotal();
            MarkAsUpdated();
        }

        public void UpdateUnitPrice(decimal unitPrice)
        {
            if (unitPrice < 0)
                throw new ArgumentException("Preço unitário não pode ser negativo", nameof(unitPrice));

            UnitPrice = unitPrice;
            RecalculateSubtotal();
            MarkAsUpdated();
        }

        public void ApplyDiscount(decimal discountAmount)
        {
            if (discountAmount < 0)
                throw new ArgumentException("Desconto não pode ser negativo", nameof(discountAmount));

            if (discountAmount > Subtotal)
                throw new ArgumentException("Desconto não pode ser maior que o subtotal");

            DiscountAmount = discountAmount;
            RecalculateSubtotal();
            MarkAsUpdated();
        }

        public void RemoveDiscount()
        {
            DiscountAmount = null;
            RecalculateSubtotal();
            MarkAsUpdated();
        }

        public void SetNotes(string notes)
        {
            Notes = notes;
            MarkAsUpdated();
        }

        private void RecalculateSubtotal()
        {
            Subtotal = Quantity * UnitPrice;
            if (DiscountAmount.HasValue)
            {
                Subtotal -= DiscountAmount.Value;
            }
        }

        public decimal GetDiscountPercentage()
        {
            if (!DiscountAmount.HasValue || Subtotal == 0)
                return 0;

            return (DiscountAmount.Value / (Quantity * UnitPrice)) * 100;
        }
    }
}
