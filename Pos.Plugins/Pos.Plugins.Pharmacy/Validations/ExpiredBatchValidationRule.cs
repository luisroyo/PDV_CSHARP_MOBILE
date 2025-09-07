using System.Linq;
using System.Threading.Tasks;
using Pos.Domain.Entities;
using Pos.Domain.Interfaces;
using Pos.Plugins.Pharmacy.Entities;
using Pos.Plugins.Pharmacy.Interfaces;

namespace Pos.Plugins.Pharmacy.Validations
{
    /// <summary>
    /// Validação que verifica se há lotes vencidos no estoque
    /// </summary>
    public class ExpiredBatchValidationRule : IValidationRule
    {
        private readonly IBatchRepository _batchRepository;

        public ExpiredBatchValidationRule(IBatchRepository batchRepository)
        {
            _batchRepository = batchRepository;
        }

        public bool CanApply(Order order)
        {
            // Aplica para todos os pedidos
            return true;
        }

        public async Task<ValidationResult> ValidateAsync(Order order)
        {
            var errors = new List<string>();

            foreach (var item in order.Items)
            {
                // Obtém os lotes do produto
                var batches = await _batchRepository.GetByProductOrderedByExpiryAsync(item.ProductId);
                
                // Verifica se há lotes vencidos
                var expiredBatches = batches.Where(b => b.IsExpired()).ToList();
                
                if (expiredBatches.Any())
                {
                    var expiredLotCodes = string.Join(", ", expiredBatches.Select(b => b.LotCode));
                    errors.Add($"Produto '{item.ProductName}' possui lotes vencidos: {expiredLotCodes}");
                }

                // Verifica se há lotes próximos do vencimento (30 dias)
                var expiringSoonBatches = batches
                    .Where(b => b.IsExpiringSoon(30) && !b.IsExpired())
                    .ToList();
                
                if (expiringSoonBatches.Any())
                {
                    var expiringLotCodes = string.Join(", ", expiringSoonBatches.Select(b => b.LotCode));
                    // Apenas aviso, não bloqueia a venda
                    errors.Add($"Atenção: Produto '{item.ProductName}' possui lotes próximos do vencimento: {expiringLotCodes}");
                }
            }

            if (errors.Any())
            {
                return ValidationResult.Failure(
                    string.Join("; ", errors),
                    "EXPIRED_BATCH"
                );
            }

            return ValidationResult.Success();
        }
    }
}
