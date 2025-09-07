using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pos.Plugins.Pharmacy.Entities;

namespace Pos.Plugins.Pharmacy.Services
{
    /// <summary>
    /// Interface para serviço de lotes
    /// </summary>
    public interface IBatchService
    {
        /// <summary>
        /// Cria um novo lote
        /// </summary>
        Task<Batch> CreateBatchAsync(Guid productId, string lotCode, DateTime expiryDate, decimal quantity, Guid tenantId);

        /// <summary>
        /// Obtém lotes próximos do vencimento
        /// </summary>
        Task<IEnumerable<Batch>> GetExpiringSoonAsync(int days, Guid tenantId);

        /// <summary>
        /// Obtém lotes vencidos
        /// </summary>
        Task<IEnumerable<Batch>> GetExpiredAsync(Guid tenantId);

        /// <summary>
        /// Aplica FEFO para decremento de estoque
        /// </summary>
        Task ApplyFEFOAsync(Guid productId, decimal quantity, Guid tenantId);

        /// <summary>
        /// Verifica se há estoque suficiente considerando FEFO
        /// </summary>
        Task<bool> HasEnoughStockFEFOAsync(Guid productId, decimal quantity, Guid tenantId);
    }
}
