using System;
using System.Threading.Tasks;
using Pos.Domain.Entities;
using Pos.Domain.Interfaces;

namespace Pos.Plugins.Pharmacy.Policies
{
    /// <summary>
    /// Política de preços específica para farmácia
    /// </summary>
    public class PharmacyPricingPolicy : IPricingPolicy
    {
        public async Task<decimal> GetPriceAsync(Product product, Customer customer, DateTime when)
        {
            // Preço base do produto
            var price = product.Price;

            // Aplica regras específicas da farmácia
            // Ex: Desconto para medicamentos genéricos
            // TODO: Implementar verificação de atributos quando o sistema EAV estiver pronto
            // if (product.HasAttribute("IsGeneric") && product.GetAttributeValue("IsGeneric") == "true")
            // {
            //     price = ApplyGenericDiscount(price);
            // }

            // Desconto para clientes com plano de saúde
            // TODO: Implementar verificação de atributos quando o sistema EAV estiver pronto
            // if (customer != null && customer.HasAttribute("HealthPlan"))
            // {
            //     price = ApplyHealthPlanDiscount(price, customer.GetAttributeValue("HealthPlan"));
            // }

            return RoundPrice(price);
        }

        public async Task<bool> CanApplyDiscountAsync(Product product, decimal discountAmount, Customer customer)
        {
            // Medicamentos controlados não podem ter desconto
            // TODO: Implementar verificação de atributos quando o sistema EAV estiver pronto
            // if (product.HasAttribute("IsControlled") && product.GetAttributeValue("IsControlled") == "true")
            // {
            //     return false;
            // }

            // Desconto máximo de 20% para medicamentos
            var maxDiscount = product.Price * 0.20m;
            return discountAmount <= maxDiscount;
        }

        public decimal RoundPrice(decimal price)
        {
            // Arredonda para 2 casas decimais
            return Math.Round(price, 2, MidpointRounding.AwayFromZero);
        }

        private decimal ApplyGenericDiscount(decimal price)
        {
            // Desconto de 10% para medicamentos genéricos
            return price * 0.90m;
        }

        private decimal ApplyHealthPlanDiscount(decimal price, string healthPlan)
        {
            // Desconto baseado no plano de saúde
            return healthPlan switch
            {
                "SUS" => price * 0.50m, // 50% de desconto para SUS
                "Particular" => price * 0.95m, // 5% de desconto para particular
                _ => price
            };
        }
    }
}
