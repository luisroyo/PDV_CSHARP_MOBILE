using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pos.Plugins.Pharmacy.Entities;
using Pos.Plugins.Pharmacy.Interfaces;

namespace Pos.Plugins.Pharmacy.Services
{
    /// <summary>
    /// Serviço para gerenciamento de prescrições
    /// </summary>
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IPrescriptionRepository _prescriptionRepository;

        public PrescriptionService(IPrescriptionRepository prescriptionRepository)
        {
            _prescriptionRepository = prescriptionRepository;
        }

        public async Task<Prescription> CreatePrescriptionAsync(Guid orderId, string number, string imageUrl, DateTime issuedAt, Guid tenantId)
        {
            var prescription = new Prescription(orderId, number, imageUrl, issuedAt, tenantId);
            return await _prescriptionRepository.AddAsync(prescription);
        }

        public async Task<Prescription> ApprovePrescriptionAsync(Guid prescriptionId)
        {
            var prescription = await _prescriptionRepository.GetByIdAsync(prescriptionId);
            if (prescription == null)
                throw new ArgumentException("Prescrição não encontrada", nameof(prescriptionId));

            prescription.Approve();
            await _prescriptionRepository.UpdateAsync(prescription);
            return prescription;
        }

        public async Task<Prescription> RejectPrescriptionAsync(Guid prescriptionId, string reason)
        {
            var prescription = await _prescriptionRepository.GetByIdAsync(prescriptionId);
            if (prescription == null)
                throw new ArgumentException("Prescrição não encontrada", nameof(prescriptionId));

            prescription.Reject(reason);
            await _prescriptionRepository.UpdateAsync(prescription);
            return prescription;
        }

        public async Task<IEnumerable<Prescription>> GetPendingPrescriptionsAsync(Guid tenantId)
        {
            return await _prescriptionRepository.GetPendingAsync(tenantId);
        }

        public async Task<bool> IsValidPrescriptionAsync(Guid orderId)
        {
            var prescription = await _prescriptionRepository.GetByOrderIdAsync(orderId);
            return prescription?.IsValid() == true;
        }
    }
}
