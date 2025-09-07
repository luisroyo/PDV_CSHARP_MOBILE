using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pos.Domain.Entities;

namespace Pos.Domain.Interfaces
{
    /// <summary>
    /// Interface específica para repositório de estoque
    /// </summary>
    public interface IStockRepository : IRepository<Stock>
    {
        /// <summary>
        /// Obtém o estoque de um produto em uma localização
        /// </summary>
        Task<Stock> GetByProductAndLocationAsync(Guid productId, Guid locationId);

        /// <summary>
        /// Obtém todo o estoque de um produto
        /// </summary>
        Task<IEnumerable<Stock>> GetByProductAsync(Guid productId);

        /// <summary>
        /// Obtém todo o estoque de uma localização
        /// </summary>
        Task<IEnumerable<Stock>> GetByLocationAsync(Guid locationId);

        /// <summary>
        /// Obtém produtos com estoque baixo
        /// </summary>
        Task<IEnumerable<Stock>> GetLowStockProductsAsync(Guid tenantId);

        /// <summary>
        /// Obtém produtos sem estoque
        /// </summary>
        Task<IEnumerable<Stock>> GetOutOfStockProductsAsync(Guid tenantId);

        /// <summary>
        /// Atualiza a quantidade de estoque
        /// </summary>
        Task UpdateQuantityAsync(Guid productId, Guid locationId, decimal quantity);

        /// <summary>
        /// Adiciona quantidade ao estoque
        /// </summary>
        Task AddQuantityAsync(Guid productId, Guid locationId, decimal quantity);

        /// <summary>
        /// Remove quantidade do estoque
        /// </summary>
        Task RemoveQuantityAsync(Guid productId, Guid locationId, decimal quantity);

        /// <summary>
        /// Reserva quantidade do estoque
        /// </summary>
        Task ReserveQuantityAsync(Guid productId, Guid locationId, decimal quantity);

        /// <summary>
        /// Libera reserva do estoque
        /// </summary>
        Task ReleaseReservationAsync(Guid productId, Guid locationId, decimal quantity);

        /// <summary>
        /// Confirma reserva do estoque (remove do estoque disponível)
        /// </summary>
        Task ConfirmReservationAsync(Guid productId, Guid locationId, decimal quantity);
    }
}
