using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pos.Plugins.Pharmacy.Entities;

namespace Pos.Plugins.Pharmacy.Services
{
    /// <summary>
    /// Interface para serviço de prescrições
    /// </summary>
    public interface IPrescriptionService
    {
        /// <summary>
        /// Cria uma nova prescrição
        /// </summary>
        Task<Prescription> CreatePrescriptionAsync(Guid orderId, string number, string imageUrl, DateTime issuedAt, Guid tenantId);

        /// <summary>
        /// Aprova uma prescrição
        /// </summary>
        Task<Prescription> ApprovePrescriptionAsync(Guid prescriptionId);

        /// <summary>
        /// Rejeita uma prescrição
        /// </summary>
        Task<Prescription> RejectPrescriptionAsync(Guid prescriptionId, string reason);

        /// <summary>
        /// Obtém prescrições pendentes
        /// </summary>
        Task<IEnumerable<Prescription>> GetPendingPrescriptionsAsync(Guid tenantId);

        /// <summary>
        /// Valida se uma prescrição é válida para um pedido
        /// </summary>
        Task<bool> IsValidPrescriptionAsync(Guid orderId);
    }
}
