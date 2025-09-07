using System;
using Pos.Domain.Entities.Base;

namespace Pos.Domain.Entities
{
    /// <summary>
    /// Entidade Tenant - representa uma empresa/loja no sistema multi-tenant
    /// </summary>
    public class Tenant : Entity
    {
        public string Name { get; private set; }
        public string Code { get; private set; }
        public string Document { get; private set; } // CNPJ
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public string Address { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string ZipCode { get; private set; }
        public BusinessProfile BusinessProfile { get; private set; }
        public bool Active { get; private set; }
        public string SettingsJson { get; private set; } // Configurações específicas do tenant

        // Navigation properties
        public List<User> Users { get; private set; } = new();
        public List<Location> Locations { get; private set; } = new();

        private Tenant() { } // EF Core

        public Tenant(string name, string code, string document, BusinessProfile businessProfile)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome não pode ser vazio", nameof(name));
            
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Código não pode ser vazio", nameof(code));
            
            if (string.IsNullOrWhiteSpace(document))
                throw new ArgumentException("Documento não pode ser vazio", nameof(document));

            Name = name;
            Code = code;
            Document = document;
            BusinessProfile = businessProfile;
            Active = true;
            SettingsJson = "{}";
        }

        public void UpdateBasicInfo(string name, string email, string phone)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome não pode ser vazio", nameof(name));

            Name = name;
            Email = email;
            Phone = phone;
            MarkAsUpdated();
        }

        public void UpdateAddress(string address, string city, string state, string zipCode)
        {
            Address = address;
            City = city;
            State = state;
            ZipCode = zipCode;
            MarkAsUpdated();
        }

        public void UpdateDocument(string document)
        {
            if (string.IsNullOrWhiteSpace(document))
                throw new ArgumentException("Documento não pode ser vazio", nameof(document));

            Document = document;
            MarkAsUpdated();
        }

        public void ChangeBusinessProfile(BusinessProfile businessProfile)
        {
            BusinessProfile = businessProfile;
            MarkAsUpdated();
        }

        public void UpdateSettings(string settingsJson)
        {
            if (string.IsNullOrWhiteSpace(settingsJson))
                throw new ArgumentException("Configurações não podem ser vazias", nameof(settingsJson));

            SettingsJson = settingsJson;
            MarkAsUpdated();
        }

        public void Activate()
        {
            Active = true;
            MarkAsUpdated();
        }

        public void Deactivate()
        {
            Active = false;
            MarkAsUpdated();
        }
    }

    public enum BusinessProfile
    {
        Generic,        // Genérico
        Pharmacy,       // Farmácia
        Construction,   // Material de construção
        Grocery,        // Supermercado
        FoodService     // Lanchonete/Food service
    }
}
