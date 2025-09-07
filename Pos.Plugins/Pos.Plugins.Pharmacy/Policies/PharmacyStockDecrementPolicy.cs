using System;
using System.Linq;
using System.Threading.Tasks;
using Pos.Domain.Entities;
using Pos.Domain.Interfaces;
using Pos.Plugins.Pharmacy.Entities;
using Pos.Plugins.Pharmacy.Interfaces;

namespace Pos.Plugins.Pharmacy.Policies
{
    /// <summary>
    /// Política de decremento de estoque específica para farmácia (FEFO)
    /// </summary>
    public class PharmacyStockDecrementPolicy : IStockDecrementPolicy
    {
        private readonly IBatchRepository _batchRepository;

        public PharmacyStockDecrementPolicy(IBatchRepository batchRepository)
        {
            _batchRepository = batchRepository;
        }

        public async Task ApplyAsync(Order order, IStockRepository stockRepository)
        {
            foreach (var item in order.Items)
            {
                // Obtém os lotes do produto ordenados por validade (FEFO)
                var batches = await _batchRepository.GetByProductOrderedByExpiryAsync(item.ProductId);
                
                var remainingQuantity = item.Quantity;
                
                foreach (var batch in batches)
                {
                    if (remainingQuantity <= 0)
                        break;

                    // Verifica se o lote não está vencido
                    if (batch.IsExpired())
                        continue;

                    // Calcula quanto pode ser retirado deste lote
                    var quantityToTake = Math.Min(remainingQuantity, batch.AvailableQuantity);
                    
                    if (quantityToTake > 0)
                    {
                        // Remove do lote
                        batch.RemoveQuantity(quantityToTake);
                        await _batchRepository.UpdateAsync(batch);
                        
                        // Atualiza o estoque geral
                        // TODO: Implementar quando tivermos LocationId na entidade Batch
                        // await stockRepository.RemoveQuantityAsync(item.ProductId, batch.LocationId, quantityToTake);
                        
                        remainingQuantity -= quantityToTake;
                    }
                }

                // Se ainda há quantidade restante, significa que não há estoque suficiente
                if (remainingQuantity > 0)
                {
                    throw new InvalidOperationException($"Estoque insuficiente para o produto {item.ProductName}. Faltam {remainingQuantity} unidades.");
                }
            }
        }

        public async Task<bool> HasEnoughStockAsync(Order order, IStockRepository stockRepository)
        {
            foreach (var item in order.Items)
            {
                var batches = await _batchRepository.GetByProductOrderedByExpiryAsync(item.ProductId);
                var availableQuantity = batches
                    .Where(b => !b.IsExpired())
                    .Sum(b => b.AvailableQuantity);

                if (availableQuantity < item.Quantity)
                    return false;
            }

            return true;
        }

        public async Task ReserveStockAsync(Order order, IStockRepository stockRepository)
        {
            foreach (var item in order.Items)
            {
                var batches = await _batchRepository.GetByProductOrderedByExpiryAsync(item.ProductId);
                var remainingQuantity = item.Quantity;
                
                foreach (var batch in batches)
                {
                    if (remainingQuantity <= 0)
                        break;

                    if (batch.IsExpired())
                        continue;

                    var quantityToReserve = Math.Min(remainingQuantity, batch.AvailableQuantity);
                    
                    if (quantityToReserve > 0)
                    {
                        batch.ReserveQuantity(quantityToReserve);
                        await _batchRepository.UpdateAsync(batch);
                        
                        // TODO: Implementar quando tivermos LocationId na entidade Batch
                        // await stockRepository.ReserveQuantityAsync(item.ProductId, batch.LocationId, quantityToReserve);
                        
                        remainingQuantity -= quantityToReserve;
                    }
                }
            }
        }
    }
}
