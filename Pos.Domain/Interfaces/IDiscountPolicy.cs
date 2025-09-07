using System.Threading.Tasks;
using Pos.Domain.Entities;

namespace Pos.Domain.Interfaces
{
    /// <summary>
    /// Interface para políticas de desconto por vertical
    /// </summary>
    public interface IDiscountPolicy
    {
        /// <summary>
        /// Calcula o desconto aplicável a um pedido
        /// </summary>
        Task<decimal> CalculateDiscountAsync(Order order, Customer customer);

        /// <summary>
        /// Verifica se um desconto pode ser aplicado
        /// </summary>
        Task<bool> CanApplyDiscountAsync(Order order, Customer customer, decimal discountAmount);

        /// <summary>
        /// Aplica regras específicas de desconto
        /// </summary>
        Task<DiscountResult> ApplyDiscountAsync(Order order, Customer customer, decimal discountAmount);
    }

    /// <summary>
    /// Resultado da aplicação de desconto
    /// </summary>
    public class DiscountResult
    {
        public bool CanApply { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Reason { get; set; }
        public string ErrorMessage { get; set; }

        public static DiscountResult Success(decimal discountAmount, string reason = null)
        {
            return new DiscountResult
            {
                CanApply = true,
                DiscountAmount = discountAmount,
                Reason = reason
            };
        }

        public static DiscountResult Failure(string errorMessage)
        {
            return new DiscountResult
            {
                CanApply = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
