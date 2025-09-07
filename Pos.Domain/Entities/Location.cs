using System;
using Pos.Domain.Entities.Base;

namespace Pos.Domain.Entities
{
    /// <summary>
    /// Entidade Localização - representa um local de estoque (loja, depósito, etc.)
    /// </summary>
    public class Location : Entity
    {
        public string Name { get; private set; }
        public string Code { get; private set; }
        public string Address { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string ZipCode { get; private set; }
        public bool Active { get; private set; }
        public bool IsDefault { get; private set; }
        public Guid TenantId { get; private set; }

        // Navigation properties
        public List<Stock> Stock { get; private set; } = new();

        private Location() { } // EF Core

        public Location(string name, string code, Guid tenantId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome não pode ser vazio", nameof(name));
            
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Código não pode ser vazio", nameof(code));

            Name = name;
            Code = code;
            TenantId = tenantId;
            Active = true;
            IsDefault = false;
        }

        public void UpdateBasicInfo(string name, string code)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome não pode ser vazio", nameof(name));
            
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Código não pode ser vazio", nameof(code));

            Name = name;
            Code = code;
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

        public void SetAsDefault()
        {
            IsDefault = true;
            MarkAsUpdated();
        }

        public void RemoveAsDefault()
        {
            IsDefault = false;
            MarkAsUpdated();
        }
    }
}
