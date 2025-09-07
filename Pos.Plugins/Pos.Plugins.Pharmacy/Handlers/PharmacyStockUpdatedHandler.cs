using System.Threading.Tasks;
using Pos.Domain.Events;
using Pos.Domain.Interfaces;
using Pos.Plugins.Pharmacy.Services;

namespace Pos.Plugins.Pharmacy.Handlers
{
    /// <summary>
    /// Handler para evento de estoque atualizado na farmácia
    /// </summary>
    public class PharmacyStockUpdatedHandler : IEventHandler<StockUpdatedEvent>
    {
        private readonly IBatchService _batchService;

        public PharmacyStockUpdatedHandler(IBatchService batchService)
        {
            _batchService = batchService;
        }

        public async Task HandleAsync(StockUpdatedEvent domainEvent)
        {
            // Verifica se há lotes próximos do vencimento
            var expiringSoon = await _batchService.GetExpiringSoonAsync(30, domainEvent.TenantId);
            
            // Verifica se há lotes vencidos
            var expired = await _batchService.GetExpiredAsync(domainEvent.TenantId);
            
            // Aqui seria implementada a lógica para:
            // - Enviar alertas para lotes próximos do vencimento
            // - Bloquear vendas de lotes vencidos
            // - Atualizar status de produtos
        }
    }
}
