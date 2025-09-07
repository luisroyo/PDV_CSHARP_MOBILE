using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pos.Domain.Interfaces;
using Pos.Plugins.Pharmacy.Entities;

namespace Pos.Plugins.Pharmacy.Interfaces
{
    /// <summary>
    /// Interface para repositório de prescrições
    /// </summary>
    public interface IPrescriptionRepository : IRepository<Prescription>
    {
        /// <summary>
        /// Obtém prescrição por pedido
        /// </summary>
        Task<Prescription> GetByOrderIdAsync(Guid orderId);

        /// <summary>
        /// Obtém prescrições por número
        /// </summary>
        Task<IEnumerable<Prescription>> GetByNumberAsync(string number, Guid tenantId);

        /// <summary>
        /// Obtém prescrições por médico
        /// </summary>
        Task<IEnumerable<Prescription>> GetByDoctorAsync(string doctorCrm, Guid tenantId);

        /// <summary>
        /// Obtém prescrições por paciente
        /// </summary>
        Task<IEnumerable<Prescription>> GetByPatientAsync(string patientDocument, Guid tenantId);

        /// <summary>
        /// Obtém prescrições pendentes
        /// </summary>
        Task<IEnumerable<Prescription>> GetPendingAsync(Guid tenantId);
    }
}
