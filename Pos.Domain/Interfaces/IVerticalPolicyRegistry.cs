namespace Pos.Domain.Interfaces
{
    /// <summary>
    /// Registry para políticas verticais
    /// </summary>
    public interface IVerticalPolicyRegistry
    {
        /// <summary>
        /// Registra uma política de preços
        /// </summary>
        void RegisterPricingPolicy<T>() where T : class, IPricingPolicy;

        /// <summary>
        /// Registra uma política de decremento de estoque
        /// </summary>
        void RegisterStockDecrementPolicy<T>() where T : class, IStockDecrementPolicy;

        /// <summary>
        /// Registra uma política de fluxo de pedidos
        /// </summary>
        void RegisterOrderWorkflow<T>() where T : class, IOrderWorkflow;

        /// <summary>
        /// Registra uma regra de validação
        /// </summary>
        void RegisterValidationRule<T>() where T : class, IValidationRule;

        /// <summary>
        /// Registra uma política de desconto
        /// </summary>
        void RegisterDiscountPolicy<T>() where T : class, IDiscountPolicy;
    }
}
