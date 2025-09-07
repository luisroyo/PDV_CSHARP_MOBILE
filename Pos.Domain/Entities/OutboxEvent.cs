using System;
using Pos.Domain.Entities.Base;

namespace Pos.Domain.Entities
{
    /// <summary>
    /// Entidade OutboxEvent - implementa o padrão Outbox para sincronização confiável
    /// </summary>
    public class OutboxEvent : Entity
    {
        public string EventType { get; private set; }
        public string PayloadJson { get; private set; }
        public int Attempts { get; private set; }
        public DateTime? NextRunAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public string ErrorMessage { get; private set; }
        public Guid TenantId { get; private set; }

        private OutboxEvent() { } // EF Core

        public OutboxEvent(string eventType, string payloadJson, Guid tenantId)
        {
            if (string.IsNullOrWhiteSpace(eventType))
                throw new ArgumentException("Tipo do evento não pode ser vazio", nameof(eventType));
            
            if (string.IsNullOrWhiteSpace(payloadJson))
                throw new ArgumentException("Payload do evento não pode ser vazio", nameof(payloadJson));

            EventType = eventType;
            PayloadJson = payloadJson;
            TenantId = tenantId;
            Attempts = 0;
            NextRunAt = DateTime.UtcNow;
        }

        public void RecordAttempt()
        {
            Attempts++;
            
            // Backoff exponencial: 30s, 1min, 2min, 4min, 8min, 16min, 30min
            var delayMinutes = Math.Min(30, Math.Pow(2, Attempts - 1) * 0.5);
            NextRunAt = DateTime.UtcNow.AddMinutes(delayMinutes);
            
            MarkAsUpdated();
        }

        public void RecordSuccess()
        {
            ProcessedAt = DateTime.UtcNow;
            NextRunAt = null;
            ErrorMessage = null;
            MarkAsUpdated();
        }

        public void RecordFailure(string errorMessage)
        {
            ErrorMessage = errorMessage;
            RecordAttempt();
        }

        public bool ShouldRetry()
        {
            // Máximo de 7 tentativas
            if (Attempts >= 7)
                return false;

            // Se não tem próxima execução agendada, não deve tentar
            if (!NextRunAt.HasValue)
                return false;

            // Se a próxima execução ainda não chegou, não deve tentar
            return NextRunAt.Value <= DateTime.UtcNow;
        }

        public bool IsProcessed()
        {
            return ProcessedAt.HasValue;
        }

        public bool IsMaxAttemptsReached()
        {
            return Attempts >= 7;
        }
    }
}
