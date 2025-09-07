using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pos.Plugins.Pharmacy.Entities;
using Pos.Plugins.Pharmacy.Interfaces;

namespace Pos.Plugins.Pharmacy.Services
{
    /// <summary>
    /// Serviço para gerenciamento de lotes
    /// </summary>
    public class BatchService : IBatchService
    {
        private readonly IBatchRepository _batchRepository;

        public BatchService(IBatchRepository batchRepository)
        {
            _batchRepository = batchRepository;
        }

        public async Task<Batch> CreateBatchAsync(Guid productId, string lotCode, DateTime expiryDate, decimal quantity, Guid tenantId)
        {
            var batch = new Batch(productId, lotCode, expiryDate, quantity, tenantId);
            return await _batchRepository.AddAsync(batch);
        }

        public async Task<IEnumerable<Batch>> GetExpiringSoonAsync(int days, Guid tenantId)
        {
            return await _batchRepository.GetExpiringSoonAsync(days, tenantId);
        }

        public async Task<IEnumerable<Batch>> GetExpiredAsync(Guid tenantId)
        {
            return await _batchRepository.GetExpiredAsync(tenantId);
        }

        public async Task ApplyFEFOAsync(Guid productId, decimal quantity, Guid tenantId)
        {
            var batches = await _batchRepository.GetByProductOrderedByExpiryAsync(productId);
            var remainingQuantity = quantity;

            foreach (var batch in batches)
            {
                if (remainingQuantity <= 0)
                    break;

                if (batch.IsExpired())
                    continue;

                var quantityToTake = Math.Min(remainingQuantity, batch.AvailableQuantity);
                
                if (quantityToTake > 0)
                {
                    batch.RemoveQuantity(quantityToTake);
                    await _batchRepository.UpdateAsync(batch);
                    remainingQuantity -= quantityToTake;
                }
            }

            if (remainingQuantity > 0)
            {
                throw new InvalidOperationException($"Estoque insuficiente para aplicar FEFO. Faltam {remainingQuantity} unidades.");
            }
        }

        public async Task<bool> HasEnoughStockFEFOAsync(Guid productId, decimal quantity, Guid tenantId)
        {
            var batches = await _batchRepository.GetByProductOrderedByExpiryAsync(productId);
            var availableQuantity = batches
                .Where(b => !b.IsExpired())
                .Sum(b => b.AvailableQuantity);

            return availableQuantity >= quantity;
        }
    }
}
