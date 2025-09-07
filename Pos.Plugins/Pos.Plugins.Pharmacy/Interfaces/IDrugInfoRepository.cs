using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pos.Domain.Interfaces;
using Pos.Plugins.Pharmacy.Entities;

namespace Pos.Plugins.Pharmacy.Interfaces
{
    /// <summary>
    /// Interface para repositório de informações de medicamentos
    /// </summary>
    public interface IDrugInfoRepository : IRepository<DrugInfo>
    {
        /// <summary>
        /// Obtém informações por produto
        /// </summary>
        Task<DrugInfo> GetByProductIdAsync(Guid productId);

        /// <summary>
        /// Obtém medicamentos que exigem prescrição
        /// </summary>
        Task<IEnumerable<DrugInfo>> GetRequiringPrescriptionAsync(Guid tenantId);

        /// <summary>
        /// Obtém medicamentos controlados
        /// </summary>
        Task<IEnumerable<DrugInfo>> GetControlledAsync(Guid tenantId);

        /// <summary>
        /// Obtém medicamentos por princípio ativo
        /// </summary>
        Task<IEnumerable<DrugInfo>> GetByActiveIngredientAsync(string activeIngredient, Guid tenantId);

        /// <summary>
        /// Obtém medicamentos por classe terapêutica
        /// </summary>
        Task<IEnumerable<DrugInfo>> GetByDrugClassAsync(string drugClass, Guid tenantId);
    }
}
