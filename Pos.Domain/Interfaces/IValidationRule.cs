using System.Threading.Tasks;
using Pos.Domain.Entities;

namespace Pos.Domain.Interfaces
{
    /// <summary>
    /// Interface para regras de validação por vertical
    /// </summary>
    public interface IValidationRule
    {
        /// <summary>
        /// Valida se uma regra pode ser aplicada
        /// </summary>
        bool CanApply(Order order);

        /// <summary>
        /// Valida uma regra específica
        /// </summary>
        Task<ValidationResult> ValidateAsync(Order order);
    }

    /// <summary>
    /// Resultado de uma validação
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }

        public static ValidationResult Success()
        {
            return new ValidationResult { IsValid = true };
        }

        public static ValidationResult Failure(string errorMessage, string errorCode = null)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode
            };
        }
    }
}
