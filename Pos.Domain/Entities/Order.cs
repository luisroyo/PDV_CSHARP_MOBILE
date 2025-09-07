using System;
using System.Collections.Generic;
using System.Linq;
using Pos.Domain.Entities.Base;
using Pos.Domain.ValueObjects;

namespace Pos.Domain.Entities
{
    /// <summary>
    /// Entidade Pedido - representa uma venda ou pedido no sistema
    /// </summary>
    public class Order : Entity
    {
        public string Number { get; private set; }
        public Guid? CustomerId { get; private set; }
        public OrderStatus Status { get; private set; }
        public decimal Subtotal { get; private set; }
        public decimal DiscountAmount { get; private set; }
        public decimal TaxAmount { get; private set; }
        public decimal Total { get; private set; }
        public string Notes { get; private set; }
        public Guid TenantId { get; private set; }
        public Guid? UserId { get; private set; } // Usuário que criou o pedido
        public DateTime? CompletedAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public string CancellationReason { get; private set; }

        // Navigation properties
        public Customer Customer { get; private set; }
        public List<OrderItem> Items { get; private set; } = new();
        public List<OrderPayment> Payments { get; private set; } = new();

        private Order() { } // EF Core

        public Order(string number, Guid tenantId, Guid? userId = null)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentException("Número do pedido não pode ser vazio", nameof(number));

            Number = number;
            TenantId = tenantId;
            UserId = userId;
            Status = OrderStatus.Draft;
            Subtotal = 0;
            DiscountAmount = 0;
            TaxAmount = 0;
            Total = 0;
        }

        public void SetCustomer(Guid? customerId)
        {
            CustomerId = customerId;
            MarkAsUpdated();
        }

        public void AddItem(OrderItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (Status != OrderStatus.Draft)
                throw new InvalidOperationException("Não é possível adicionar itens a um pedido que não está em rascunho");

            // Verifica se já existe um item com o mesmo produto
            var existingItem = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existingItem != null)
            {
                existingItem.IncreaseQuantity(item.Quantity);
            }
            else
            {
                item.SetOrderId(Id);
                Items.Add(item);
            }

            RecalculateTotals();
            MarkAsUpdated();
        }

        public void RemoveItem(Guid itemId)
        {
            if (Status != OrderStatus.Draft)
                throw new InvalidOperationException("Não é possível remover itens de um pedido que não está em rascunho");

            var item = Items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                Items.Remove(item);
                RecalculateTotals();
                MarkAsUpdated();
            }
        }

        public void UpdateItemQuantity(Guid itemId, decimal quantity)
        {
            if (Status != OrderStatus.Draft)
                throw new InvalidOperationException("Não é possível alterar itens de um pedido que não está em rascunho");

            var item = Items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                item.UpdateQuantity(quantity);
                RecalculateTotals();
                MarkAsUpdated();
            }
        }

        public void ApplyDiscount(decimal discountAmount, string reason = null)
        {
            if (discountAmount < 0)
                throw new ArgumentException("Desconto não pode ser negativo", nameof(discountAmount));

            if (discountAmount > Subtotal)
                throw new ArgumentException("Desconto não pode ser maior que o subtotal");

            DiscountAmount = discountAmount;
            RecalculateTotals();
            MarkAsUpdated();
        }

        public void ApplyTax(decimal taxAmount)
        {
            if (taxAmount < 0)
                throw new ArgumentException("Imposto não pode ser negativo", nameof(taxAmount));

            TaxAmount = taxAmount;
            RecalculateTotals();
            MarkAsUpdated();
        }

        public void SetNotes(string notes)
        {
            Notes = notes;
            MarkAsUpdated();
        }

        public void Confirm()
        {
            if (Status != OrderStatus.Draft)
                throw new InvalidOperationException("Apenas pedidos em rascunho podem ser confirmados");

            if (!Items.Any())
                throw new InvalidOperationException("Pedido deve ter pelo menos um item");

            Status = OrderStatus.Placed;
            MarkAsUpdated();
        }

        public void Complete()
        {
            if (Status != OrderStatus.Placed)
                throw new InvalidOperationException("Apenas pedidos confirmados podem ser finalizados");

            Status = OrderStatus.Fulfilled;
            CompletedAt = DateTime.UtcNow;
            MarkAsUpdated();
        }

        public void Cancel(string reason = null)
        {
            if (Status == OrderStatus.Fulfilled)
                throw new InvalidOperationException("Pedidos finalizados não podem ser cancelados");

            Status = OrderStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
            CancellationReason = reason;
            MarkAsUpdated();
        }

        public void AddPayment(OrderPayment payment)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            payment.SetOrderId(Id);
            Payments.Add(payment);
            MarkAsUpdated();
        }

        private void RecalculateTotals()
        {
            Subtotal = Items.Sum(i => i.Subtotal);
            Total = Subtotal - DiscountAmount + TaxAmount;
        }

        public decimal GetRemainingAmount()
        {
            var paidAmount = Payments.Sum(p => p.Amount);
            return Total - paidAmount;
        }

        public bool IsFullyPaid()
        {
            return GetRemainingAmount() <= 0;
        }
    }

    public enum OrderStatus
    {
        Draft,      // Rascunho
        Placed,     // Confirmado
        Cancelled,  // Cancelado
        Fulfilled   // Atendido/Finalizado
    }
}
