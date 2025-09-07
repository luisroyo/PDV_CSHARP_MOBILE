using System;
using Pos.Domain.Events.Base;

namespace Pos.Domain.Events
{
    /// <summary>
    /// Evento disparado quando um pedido é criado
    /// </summary>
    public class OrderCreatedEvent : DomainEvent
    {
        public Guid OrderId { get; private set; }
        public string OrderNumber { get; private set; }
        public Guid? CustomerId { get; private set; }
        public decimal Total { get; private set; }

        public OrderCreatedEvent(Guid orderId, string orderNumber, Guid? customerId, decimal total, Guid tenantId)
            : base(tenantId)
        {
            OrderId = orderId;
            OrderNumber = orderNumber;
            CustomerId = customerId;
            Total = total;
        }
    }

    /// <summary>
    /// Evento disparado quando um pedido é confirmado
    /// </summary>
    public class OrderConfirmedEvent : DomainEvent
    {
        public Guid OrderId { get; private set; }
        public string OrderNumber { get; private set; }
        public Guid? CustomerId { get; private set; }
        public decimal Total { get; private set; }

        public OrderConfirmedEvent(Guid orderId, string orderNumber, Guid? customerId, decimal total, Guid tenantId)
            : base(tenantId)
        {
            OrderId = orderId;
            OrderNumber = orderNumber;
            CustomerId = customerId;
            Total = total;
        }
    }

    /// <summary>
    /// Evento disparado quando um pedido é cancelado
    /// </summary>
    public class OrderCancelledEvent : DomainEvent
    {
        public Guid OrderId { get; private set; }
        public string OrderNumber { get; private set; }
        public string Reason { get; private set; }

        public OrderCancelledEvent(Guid orderId, string orderNumber, string reason, Guid tenantId)
            : base(tenantId)
        {
            OrderId = orderId;
            OrderNumber = orderNumber;
            Reason = reason;
        }
    }

    /// <summary>
    /// Evento disparado quando um pedido é finalizado
    /// </summary>
    public class OrderCompletedEvent : DomainEvent
    {
        public Guid OrderId { get; private set; }
        public string OrderNumber { get; private set; }
        public Guid? CustomerId { get; private set; }
        public decimal Total { get; private set; }

        public OrderCompletedEvent(Guid orderId, string orderNumber, Guid? customerId, decimal total, Guid tenantId)
            : base(tenantId)
        {
            OrderId = orderId;
            OrderNumber = orderNumber;
            CustomerId = customerId;
            Total = total;
        }
    }
}
