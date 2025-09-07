using System;
using Pos.Domain.Entities.Base;

namespace Pos.Domain.Events.Base
{
    /// <summary>
    /// Classe base para todos os eventos de domínio
    /// </summary>
    public abstract class DomainEvent
    {
        public Guid Id { get; private set; }
        public DateTime OccurredAt { get; private set; }
        public Guid TenantId { get; private set; }

        protected DomainEvent(Guid tenantId)
        {
            Id = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            TenantId = tenantId;
        }
    }
}
