using Pos.Domain.Events.Base;

namespace Pos.Domain.Interfaces
{
    /// <summary>
    /// Registry para handlers de eventos
    /// </summary>
    public interface IEventHandlerRegistry
    {
        /// <summary>
        /// Registra um handler de evento
        /// </summary>
        void RegisterEventHandler<TEvent, THandler>()
            where TEvent : DomainEvent
            where THandler : class, IEventHandler<TEvent>;
    }
}
