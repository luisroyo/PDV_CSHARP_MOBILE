using System;
using Pos.Domain.Entities.Base;
using Pos.Domain.Entities;

namespace Pos.Plugins.Pharmacy.Entities
{
    /// <summary>
    /// Entidade Informações do Medicamento - dados específicos de medicamentos
    /// </summary>
    public class DrugInfo : Entity
    {
        public Guid ProductId { get; private set; }
        public string ActiveIngredient { get; private set; }
        public string Dosage { get; private set; }
        public string Form { get; private set; } // Comprimido, cápsula, xarope, etc.
        public bool RequiresPrescription { get; private set; }
        public string DrugClass { get; private set; } // Classe terapêutica
        public string Indication { get; private set; } // Indicação
        public string Contraindication { get; private set; } // Contraindicação
        public string SideEffects { get; private set; } // Efeitos colaterais
        public string StorageConditions { get; private set; } // Condições de armazenamento
        public bool IsControlled { get; private set; } // Medicamento controlado
        public string ControlClass { get; private set; } // Classe de controle (A1, A2, B1, B2, C1, C2)
        public Guid TenantId { get; private set; }

        // Navigation properties
        public Product Product { get; private set; }

        private DrugInfo() { } // EF Core

        public DrugInfo(Guid productId, string activeIngredient, bool requiresPrescription, Guid tenantId)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("ID do produto não pode ser vazio", nameof(productId));
            
            if (string.IsNullOrWhiteSpace(activeIngredient))
                throw new ArgumentException("Princípio ativo não pode ser vazio", nameof(activeIngredient));

            ProductId = productId;
            ActiveIngredient = activeIngredient;
            RequiresPrescription = requiresPrescription;
            TenantId = tenantId;
            IsControlled = false;
        }

        public void UpdateBasicInfo(string activeIngredient, string dosage, string form)
        {
            if (string.IsNullOrWhiteSpace(activeIngredient))
                throw new ArgumentException("Princípio ativo não pode ser vazio", nameof(activeIngredient));

            ActiveIngredient = activeIngredient;
            Dosage = dosage;
            Form = form;
            MarkAsUpdated();
        }

        public void UpdatePrescriptionRequirement(bool requiresPrescription)
        {
            RequiresPrescription = requiresPrescription;
            MarkAsUpdated();
        }

        public void UpdateDrugClass(string drugClass)
        {
            DrugClass = drugClass;
            MarkAsUpdated();
        }

        public void UpdateIndication(string indication)
        {
            Indication = indication;
            MarkAsUpdated();
        }

        public void UpdateContraindication(string contraindication)
        {
            Contraindication = contraindication;
            MarkAsUpdated();
        }

        public void UpdateSideEffects(string sideEffects)
        {
            SideEffects = sideEffects;
            MarkAsUpdated();
        }

        public void UpdateStorageConditions(string storageConditions)
        {
            StorageConditions = storageConditions;
            MarkAsUpdated();
        }

        public void SetControlled(bool isControlled, string controlClass = null)
        {
            IsControlled = isControlled;
            ControlClass = controlClass;
            MarkAsUpdated();
        }

        public bool IsHighRisk()
        {
            return IsControlled && (ControlClass == "A1" || ControlClass == "A2");
        }

        public bool RequiresSpecialHandling()
        {
            return IsControlled || !string.IsNullOrWhiteSpace(StorageConditions);
        }
    }
}
