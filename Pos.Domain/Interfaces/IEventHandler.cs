using System.Threading.Tasks;
using Pos.Domain.Events.Base;

namespace Pos.Domain.Interfaces
{
    /// <summary>
    /// Interface para handlers de eventos de domínio
    /// </summary>
    public interface IEventHandler<in TEvent> where TEvent : DomainEvent
    {
        /// <summary>
        /// Processa um evento de domínio
        /// </summary>
        Task HandleAsync(TEvent domainEvent);
    }
}
