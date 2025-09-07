using System;
using System.Threading.Tasks;
using Pos.Domain.Entities;

namespace Pos.Domain.Interfaces
{
    /// <summary>
    /// Interface para políticas de preços por vertical
    /// </summary>
    public interface IPricingPolicy
    {
        /// <summary>
        /// Calcula o preço de um produto considerando as regras específicas da vertical
        /// </summary>
        Task<decimal> GetPriceAsync(Product product, Customer customer, DateTime when);

        /// <summary>
        /// Verifica se um desconto pode ser aplicado
        /// </summary>
        Task<bool> CanApplyDiscountAsync(Product product, decimal discountAmount, Customer customer);

        /// <summary>
        /// Aplica regras de arredondamento específicas da vertical
        /// </summary>
        decimal RoundPrice(decimal price);
    }
}
