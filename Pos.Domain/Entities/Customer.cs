using System;
using Pos.Domain.Entities.Base;
using Pos.Domain.ValueObjects;

namespace Pos.Domain.Entities
{
    /// <summary>
    /// Entidade Cliente - representa um cliente do sistema
    /// </summary>
    public class Customer : Entity
    {
        public string Name { get; private set; }
        public string Document { get; private set; } // CPF/CNPJ
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public string Address { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string ZipCode { get; private set; }
        public DateTime? BirthDate { get; private set; }
        public bool Active { get; private set; }
        public Guid TenantId { get; private set; }
        public decimal CreditLimit { get; private set; }
        public decimal CurrentBalance { get; private set; }

        // Navigation properties
        public List<Order> Orders { get; private set; } = new();

        private Customer() { } // EF Core

        public Customer(string name, string document, Guid tenantId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome não pode ser vazio", nameof(name));
            
            if (string.IsNullOrWhiteSpace(document))
                throw new ArgumentException("Documento não pode ser vazio", nameof(document));

            Name = name;
            Document = document;
            TenantId = tenantId;
            Active = true;
            CreditLimit = 0;
            CurrentBalance = 0;
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

        public void SetBirthDate(DateTime? birthDate)
        {
            if (birthDate.HasValue && birthDate.Value > DateTime.Today)
                throw new ArgumentException("Data de nascimento não pode ser futura", nameof(birthDate));

            BirthDate = birthDate;
            MarkAsUpdated();
        }

        public void SetCreditLimit(decimal creditLimit)
        {
            if (creditLimit < 0)
                throw new ArgumentException("Limite de crédito não pode ser negativo", nameof(creditLimit));

            CreditLimit = creditLimit;
            MarkAsUpdated();
        }

        public void AddToBalance(decimal amount)
        {
            CurrentBalance += amount;
            MarkAsUpdated();
        }

        public void SubtractFromBalance(decimal amount)
        {
            if (amount < 0)
                throw new ArgumentException("Valor não pode ser negativo", nameof(amount));

            if (CurrentBalance - amount < -CreditLimit)
                throw new InvalidOperationException("Operação excederia o limite de crédito");

            CurrentBalance -= amount;
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

        public bool CanMakePurchase(decimal amount)
        {
            return Active && (CurrentBalance - amount) >= -CreditLimit;
        }

        public decimal GetAvailableCredit()
        {
            return CreditLimit + CurrentBalance;
        }
    }
}
