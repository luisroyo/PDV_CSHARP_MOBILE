using System;
using Pos.Domain.Entities.Base;

namespace Pos.Domain.Entities
{
    /// <summary>
    /// Entidade Pagamento do Pedido - representa um pagamento realizado em um pedido
    /// </summary>
    public class OrderPayment : Entity
    {
        public Guid OrderId { get; private set; }
        public PaymentMethod Method { get; private set; }
        public decimal Amount { get; private set; }
        public string Reference { get; private set; } // Número do cartão, cheque, etc.
        public string Notes { get; private set; }
        public PaymentStatus Status { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public Guid TenantId { get; private set; }

        private OrderPayment() { } // EF Core

        public OrderPayment(Guid orderId, PaymentMethod method, decimal amount, Guid tenantId)
        {
            if (orderId == Guid.Empty)
                throw new ArgumentException("ID do pedido não pode ser vazio", nameof(orderId));
            
            if (amount <= 0)
                throw new ArgumentException("Valor deve ser maior que zero", nameof(amount));

            OrderId = orderId;
            Method = method;
            Amount = amount;
            TenantId = tenantId;
            Status = PaymentStatus.Pending;
        }

        public void SetOrderId(Guid orderId)
        {
            if (orderId == Guid.Empty)
                throw new ArgumentException("ID do pedido não pode ser vazio", nameof(orderId));

            OrderId = orderId;
        }

        public void SetReference(string reference)
        {
            Reference = reference;
            MarkAsUpdated();
        }

        public void SetNotes(string notes)
        {
            Notes = notes;
            MarkAsUpdated();
        }

        public void Process()
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidOperationException("Apenas pagamentos pendentes podem ser processados");

            Status = PaymentStatus.Processed;
            ProcessedAt = DateTime.UtcNow;
            MarkAsUpdated();
        }

        public void Cancel(string reason = null)
        {
            if (Status == PaymentStatus.Processed)
                throw new InvalidOperationException("Pagamentos processados não podem ser cancelados");

            Status = PaymentStatus.Cancelled;
            Notes = reason;
            MarkAsUpdated();
        }

        public void Refund(string reason = null)
        {
            if (Status != PaymentStatus.Processed)
                throw new InvalidOperationException("Apenas pagamentos processados podem ser estornados");

            Status = PaymentStatus.Refunded;
            Notes = reason;
            MarkAsUpdated();
        }
    }

    public enum PaymentMethod
    {
        Cash,           // Dinheiro
        CreditCard,     // Cartão de crédito
        DebitCard,      // Cartão de débito
        Pix,            // PIX
        BankTransfer,   // Transferência bancária
        Check,          // Cheque
        Voucher,        // Vale
        Credit          // Crédito do cliente
    }

    public enum PaymentStatus
    {
        Pending,        // Pendente
        Processed,      // Processado
        Cancelled,      // Cancelado
        Refunded        // Estornado
    }
}
