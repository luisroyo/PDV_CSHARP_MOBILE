using System.Linq;
using System.Threading.Tasks;
using Pos.Domain.Entities;
using Pos.Domain.Interfaces;
using Pos.Plugins.Pharmacy.Entities;
using Pos.Plugins.Pharmacy.Interfaces;

namespace Pos.Plugins.Pharmacy.Validations
{
    /// <summary>
    /// Validação que verifica se medicamentos que exigem prescrição têm prescrição válida
    /// </summary>
    public class PrescriptionRequiredValidationRule : IValidationRule
    {
        private readonly IDrugInfoRepository _drugInfoRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;

        public PrescriptionRequiredValidationRule(
            IDrugInfoRepository drugInfoRepository,
            IPrescriptionRepository prescriptionRepository)
        {
            _drugInfoRepository = drugInfoRepository;
            _prescriptionRepository = prescriptionRepository;
        }

        public bool CanApply(Order order)
        {
            // Aplica apenas para pedidos confirmados
            return order.Status == OrderStatus.Placed;
        }

        public async Task<ValidationResult> ValidateAsync(Order order)
        {
            var errors = new List<string>();

            foreach (var item in order.Items)
            {
                // Verifica se o produto é um medicamento que exige prescrição
                var drugInfo = await _drugInfoRepository.GetByProductIdAsync(item.ProductId);
                
                if (drugInfo?.RequiresPrescription == true)
                {
                    // Verifica se existe prescrição válida para este pedido
                    var prescription = await _prescriptionRepository.GetByOrderIdAsync(order.Id);
                    
                    if (prescription == null)
                    {
                        errors.Add($"Medicamento '{item.ProductName}' exige prescrição médica");
                    }
                    else if (!prescription.IsValid())
                    {
                        if (prescription.IsExpired())
                        {
                            errors.Add($"Prescrição para '{item.ProductName}' está vencida");
                        }
                        else if (prescription.Status != PrescriptionStatus.Approved)
                        {
                            errors.Add($"Prescrição para '{item.ProductName}' não foi aprovada");
                        }
                    }
                }
            }

            if (errors.Any())
            {
                return ValidationResult.Failure(
                    string.Join("; ", errors),
                    "PRESCRIPTION_REQUIRED"
                );
            }

            return ValidationResult.Success();
        }
    }
}
