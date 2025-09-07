using System;
using System.Threading.Tasks;
using Pos.Domain.Entities;
using Pos.Domain.Interfaces;

namespace Pos.Plugins.Pharmacy.Policies
{
    /// <summary>
    /// Política de desconto específica para farmácia
    /// </summary>
    public class PharmacyDiscountPolicy : IDiscountPolicy
    {
        public async Task<decimal> CalculateDiscountAsync(Order order, Customer customer)
        {
            decimal totalDiscount = 0;

            // Desconto para medicamentos genéricos
            // TODO: Implementar verificação de atributos quando o sistema EAV estiver pronto
            // foreach (var item in order.Items)
            // {
            //     if (item.Product.HasAttribute("IsGeneric") && item.Product.GetAttributeValue("IsGeneric") == "true")
            //     {
            //         var genericDiscount = item.Subtotal * 0.10m; // 10% de desconto
            //         totalDiscount += genericDiscount;
            //     }
            // }

            // Desconto para clientes com plano de saúde
            // TODO: Implementar verificação de atributos quando o sistema EAV estiver pronto
            // if (customer != null && customer.HasAttribute("HealthPlan"))
            // {
            //     var healthPlan = customer.GetAttributeValue("HealthPlan");
            //     var healthPlanDiscount = healthPlan switch
            //     {
            //         "SUS" => order.Subtotal * 0.20m, // 20% de desconto para SUS
            //         "Particular" => order.Subtotal * 0.05m, // 5% de desconto para particular
            //         _ => 0
            //     };
            //     totalDiscount += healthPlanDiscount;
            // }

            // Desconto por volume (compras acima de R$ 100)
            if (order.Subtotal >= 100)
            {
                totalDiscount += order.Subtotal * 0.05m; // 5% de desconto adicional
            }

            return totalDiscount;
        }

        public async Task<bool> CanApplyDiscountAsync(Order order, Customer customer, decimal discountAmount)
        {
            // Medicamentos controlados não podem ter desconto
            // TODO: Implementar verificação de atributos quando o sistema EAV estiver pronto
            // foreach (var item in order.Items)
            // {
            //     if (item.Product.HasAttribute("IsControlled") && item.Product.GetAttributeValue("IsControlled") == "true")
            //     {
            //         return false;
            //     }
            // }

            // Desconto máximo de 30% do valor total
            var maxDiscount = order.Subtotal * 0.30m;
            return discountAmount <= maxDiscount;
        }

        public async Task<DiscountResult> ApplyDiscountAsync(Order order, Customer customer, decimal discountAmount)
        {
            var canApply = await CanApplyDiscountAsync(order, customer, discountAmount);
            
            if (!canApply)
            {
                return DiscountResult.Failure("Desconto não pode ser aplicado devido a restrições da farmácia");
            }

            // Aplica o desconto
            order.ApplyDiscount(discountAmount, "Desconto farmácia");

            return DiscountResult.Success(discountAmount, "Desconto aplicado com sucesso");
        }
    }
}
