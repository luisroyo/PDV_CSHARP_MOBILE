using System.Threading.Tasks;
using Pos.Domain.Events.Base;

namespace Pos.Domain.Interfaces
{
    /// <summary>
    /// Interface para publicação de eventos de domínio
    /// </summary>
    public interface IDomainEventPublisher
    {
        /// <summary>
        /// Publica um evento de domínio
        /// </summary>
        Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : DomainEvent;

        /// <summary>
        /// Publica múltiplos eventos de domínio
        /// </summary>
        Task PublishAsync<TEvent>(IEnumerable<TEvent> domainEvents) where TEvent : DomainEvent;
    }
}
