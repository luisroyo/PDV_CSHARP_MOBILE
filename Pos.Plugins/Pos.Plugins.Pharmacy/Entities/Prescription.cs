using System;
using Pos.Domain.Entities.Base;
using Pos.Domain.Entities;

namespace Pos.Plugins.Pharmacy.Entities
{
    /// <summary>
    /// Entidade Prescrição - representa uma prescrição médica
    /// </summary>
    public class Prescription : Entity
    {
        public Guid OrderId { get; private set; }
        public string Number { get; private set; }
        public string ImageUrl { get; private set; }
        public DateTime IssuedAt { get; private set; }
        public string DoctorName { get; private set; }
        public string DoctorCrm { get; private set; }
        public string PatientName { get; private set; }
        public string PatientDocument { get; private set; }
        public PrescriptionStatus Status { get; private set; }
        public string Notes { get; private set; }
        public Guid TenantId { get; private set; }

        // Navigation properties
        public Order Order { get; private set; }

        private Prescription() { } // EF Core

        public Prescription(Guid orderId, string number, string imageUrl, DateTime issuedAt, Guid tenantId)
        {
            if (orderId == Guid.Empty)
                throw new ArgumentException("ID do pedido não pode ser vazio", nameof(orderId));
            
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentException("Número da prescrição não pode ser vazio", nameof(number));
            
            if (issuedAt > DateTime.UtcNow)
                throw new ArgumentException("Data de emissão não pode ser futura", nameof(issuedAt));

            OrderId = orderId;
            Number = number;
            ImageUrl = imageUrl;
            IssuedAt = issuedAt;
            TenantId = tenantId;
            Status = PrescriptionStatus.Pending;
        }

        public void UpdateBasicInfo(string doctorName, string doctorCrm, string patientName, string patientDocument)
        {
            DoctorName = doctorName;
            DoctorCrm = doctorCrm;
            PatientName = patientName;
            PatientDocument = patientDocument;
            MarkAsUpdated();
        }

        public void SetNotes(string notes)
        {
            Notes = notes;
            MarkAsUpdated();
        }

        public void Approve()
        {
            if (Status != PrescriptionStatus.Pending)
                throw new InvalidOperationException("Apenas prescrições pendentes podem ser aprovadas");

            Status = PrescriptionStatus.Approved;
            MarkAsUpdated();
        }

        public void Reject(string reason)
        {
            if (Status != PrescriptionStatus.Pending)
                throw new InvalidOperationException("Apenas prescrições pendentes podem ser rejeitadas");

            Status = PrescriptionStatus.Rejected;
            Notes = reason;
            MarkAsUpdated();
        }

        public void Cancel()
        {
            if (Status == PrescriptionStatus.Approved)
                throw new InvalidOperationException("Prescrições aprovadas não podem ser canceladas");

            Status = PrescriptionStatus.Cancelled;
            MarkAsUpdated();
        }

        public bool IsValid()
        {
            return Status == PrescriptionStatus.Approved && !IsExpired();
        }

        public bool IsExpired()
        {
            // Prescrições expiram em 30 dias
            return IssuedAt.AddDays(30) < DateTime.UtcNow;
        }

        public int DaysUntilExpiry()
        {
            return (IssuedAt.AddDays(30) - DateTime.UtcNow).Days;
        }
    }

    public enum PrescriptionStatus
    {
        Pending,    // Pendente
        Approved,   // Aprovada
        Rejected,   // Rejeitada
        Cancelled   // Cancelada
    }
}
