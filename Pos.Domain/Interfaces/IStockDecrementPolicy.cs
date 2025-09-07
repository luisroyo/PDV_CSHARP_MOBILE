using System.Threading.Tasks;
using Pos.Domain.Entities;

namespace Pos.Domain.Interfaces
{
    /// <summary>
    /// Interface para políticas de decremento de estoque por vertical
    /// </summary>
    public interface IStockDecrementPolicy
    {
        /// <summary>
        /// Aplica a política de decremento de estoque para um pedido
        /// </summary>
        Task ApplyAsync(Order order, IStockRepository stockRepository);

        /// <summary>
        /// Verifica se há estoque suficiente para o pedido
        /// </summary>
        Task<bool> HasEnoughStockAsync(Order order, IStockRepository stockRepository);

        /// <summary>
        /// Reserva estoque para um pedido
        /// </summary>
        Task ReserveStockAsync(Order order, IStockRepository stockRepository);
    }
}
