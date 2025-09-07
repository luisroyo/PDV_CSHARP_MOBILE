using Pos.Domain.Entities;
using Pos.Domain.Interfaces;

namespace Pos.Plugins.Pharmacy.Policies
{
    /// <summary>
    /// Fluxo de trabalho específico para farmácia
    /// </summary>
    public class PharmacyOrderWorkflow : IOrderWorkflow
    {
        public OrderStatus GetNextStatus(Order order, string action)
        {
            return action switch
            {
                "confirm" when order.Status == OrderStatus.Draft => OrderStatus.Placed,
                "complete" when order.Status == OrderStatus.Placed => OrderStatus.Fulfilled,
                "cancel" when order.Status != OrderStatus.Fulfilled => OrderStatus.Cancelled,
                _ => order.Status
            };
        }

        public bool CanExecuteAction(Order order, string action)
        {
            return action switch
            {
                "confirm" => CanConfirm(order),
                "complete" => CanComplete(order),
                "cancel" => CanCancel(order),
                _ => false
            };
        }

        public bool CanConfirm(Order order)
        {
            // Verifica se o pedido tem itens
            if (!order.Items.Any())
                return false;

            // Verifica se todos os medicamentos que exigem prescrição têm prescrição válida
            // Esta validação será feita pelas regras de validação específicas
            return order.Status == OrderStatus.Draft;
        }

        public bool CanCancel(Order order)
        {
            return order.Status != OrderStatus.Fulfilled;
        }

        public bool CanComplete(Order order)
        {
            // Verifica se o pedido foi confirmado
            if (order.Status != OrderStatus.Placed)
                return false;

            // Verifica se todos os itens têm estoque suficiente
            // Esta validação será feita pelas regras de validação específicas
            return true;
        }
    }
}
