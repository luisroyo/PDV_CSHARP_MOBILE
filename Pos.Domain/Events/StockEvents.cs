using System;
using Pos.Domain.Events.Base;

namespace Pos.Domain.Events
{
    /// <summary>
    /// Evento disparado quando o estoque de um produto é alterado
    /// </summary>
    public class StockUpdatedEvent : DomainEvent
    {
        public Guid ProductId { get; private set; }
        public Guid LocationId { get; private set; }
        public decimal OldQuantity { get; private set; }
        public decimal NewQuantity { get; private set; }
        public decimal ChangeAmount { get; private set; }
        public string Reason { get; private set; }

        public StockUpdatedEvent(Guid productId, Guid locationId, decimal oldQuantity, decimal newQuantity, string reason, Guid tenantId)
            : base(tenantId)
        {
            ProductId = productId;
            LocationId = locationId;
            OldQuantity = oldQuantity;
            NewQuantity = newQuantity;
            ChangeAmount = newQuantity - oldQuantity;
            Reason = reason;
        }
    }

    /// <summary>
    /// Evento disparado quando o estoque de um produto fica abaixo do mínimo
    /// </summary>
    public class StockBelowMinimumEvent : DomainEvent
    {
        public Guid ProductId { get; private set; }
        public Guid LocationId { get; private set; }
        public decimal CurrentQuantity { get; private set; }
        public decimal MinimumQuantity { get; private set; }

        public StockBelowMinimumEvent(Guid productId, Guid locationId, decimal currentQuantity, decimal minimumQuantity, Guid tenantId)
            : base(tenantId)
        {
            ProductId = productId;
            LocationId = locationId;
            CurrentQuantity = currentQuantity;
            MinimumQuantity = minimumQuantity;
        }
    }

    /// <summary>
    /// Evento disparado quando um produto fica sem estoque
    /// </summary>
    public class ProductOutOfStockEvent : DomainEvent
    {
        public Guid ProductId { get; private set; }
        public Guid LocationId { get; private set; }
        public string ProductName { get; private set; }
        public string ProductSku { get; private set; }

        public ProductOutOfStockEvent(Guid productId, Guid locationId, string productName, string productSku, Guid tenantId)
            : base(tenantId)
        {
            ProductId = productId;
            LocationId = locationId;
            ProductName = productName;
            ProductSku = productSku;
        }
    }
}
