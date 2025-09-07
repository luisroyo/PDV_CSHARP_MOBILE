using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pos.Domain.Interfaces;
using Pos.Plugins.Pharmacy.Entities;

namespace Pos.Plugins.Pharmacy.Interfaces
{
    /// <summary>
    /// Interface para repositório de lotes
    /// </summary>
    public interface IBatchRepository : IRepository<Batch>
    {
        /// <summary>
        /// Obtém lotes de um produto ordenados por validade (FEFO)
        /// </summary>
        Task<IEnumerable<Batch>> GetByProductOrderedByExpiryAsync(Guid productId);

        /// <summary>
        /// Obtém lotes próximos do vencimento
        /// </summary>
        Task<IEnumerable<Batch>> GetExpiringSoonAsync(int days, Guid tenantId);

        /// <summary>
        /// Obtém lotes vencidos
        /// </summary>
        Task<IEnumerable<Batch>> GetExpiredAsync(Guid tenantId);

        /// <summary>
        /// Obtém lote por código
        /// </summary>
        Task<Batch> GetByLotCodeAsync(string lotCode, Guid tenantId);
    }
}
