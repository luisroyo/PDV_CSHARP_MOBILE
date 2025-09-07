using Pos.Domain.Entities;

namespace Pos.Domain.Interfaces
{
    /// <summary>
    /// Interface para fluxos de trabalho de pedidos por vertical
    /// </summary>
    public interface IOrderWorkflow
    {
        /// <summary>
        /// Obtém o próximo status do pedido baseado na ação
        /// </summary>
        OrderStatus GetNextStatus(Order order, string action);

        /// <summary>
        /// Verifica se uma ação é válida para o status atual do pedido
        /// </summary>
        bool CanExecuteAction(Order order, string action);

        /// <summary>
        /// Valida se o pedido pode ser confirmado
        /// </summary>
        bool CanConfirm(Order order);

        /// <summary>
        /// Valida se o pedido pode ser cancelado
        /// </summary>
        bool CanCancel(Order order);

        /// <summary>
        /// Valida se o pedido pode ser finalizado
        /// </summary>
        bool CanComplete(Order order);
    }
}
