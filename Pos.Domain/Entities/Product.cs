using System;
using System.Collections.Generic;
using Pos.Domain.Entities.Base;
using Pos.Domain.ValueObjects;

namespace Pos.Domain.Entities
{
    /// <summary>
    /// Entidade Produto - representa um item vendável no sistema
    /// </summary>
    public class Product : Entity
    {
        public string Sku { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public decimal Price { get; private set; }
        public decimal? CostPrice { get; private set; }
        public bool Active { get; private set; }
        public string Category { get; private set; }
        public string Barcode { get; private set; }
        public decimal? Weight { get; private set; }
        public string Unit { get; private set; }
        public int? MinStock { get; private set; }
        public int? MaxStock { get; private set; }
        public Guid TenantId { get; private set; }

        // Navigation properties
        public List<ProductAttribute> Attributes { get; private set; } = new();
        public List<Stock> Stock { get; private set; } = new();

        private Product() { } // EF Core

        public Product(string sku, string name, decimal price, Guid tenantId)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("SKU não pode ser vazio", nameof(sku));
            
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome não pode ser vazio", nameof(name));
            
            if (price < 0)
                throw new ArgumentException("Preço não pode ser negativo", nameof(price));

            Sku = sku;
            Name = name;
            Price = price;
            TenantId = tenantId;
            Active = true;
            Unit = "UN"; // Unidade padrão
        }

        public void UpdateBasicInfo(string name, string description, decimal price, decimal? costPrice)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome não pode ser vazio", nameof(name));
            
            if (price < 0)
                throw new ArgumentException("Preço não pode ser negativo", nameof(price));

            Name = name;
            Description = description;
            Price = price;
            CostPrice = costPrice;
            MarkAsUpdated();
        }

        public void UpdateStockInfo(int? minStock, int? maxStock)
        {
            if (minStock.HasValue && maxStock.HasValue && minStock > maxStock)
                throw new ArgumentException("Estoque mínimo não pode ser maior que o máximo");

            MinStock = minStock;
            MaxStock = maxStock;
            MarkAsUpdated();
        }

        public void UpdateBarcode(string barcode)
        {
            Barcode = barcode;
            MarkAsUpdated();
        }

        public void UpdateWeight(decimal? weight, string unit)
        {
            if (weight.HasValue && weight < 0)
                throw new ArgumentException("Peso não pode ser negativo", nameof(weight));

            Weight = weight;
            Unit = unit ?? "UN";
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

        public void AddAttribute(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome do atributo não pode ser vazio", nameof(name));

            var attribute = new ProductAttribute(Id, name, value);
            Attributes.Add(attribute);
            MarkAsUpdated();
        }

        public void RemoveAttribute(Guid attributeId)
        {
            var attribute = Attributes.Find(a => a.Id == attributeId);
            if (attribute != null)
            {
                Attributes.Remove(attribute);
                MarkAsUpdated();
            }
        }

        public bool HasAttribute(string name)
        {
            return Attributes.Exists(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public string GetAttributeValue(string name)
        {
            var attribute = Attributes.Find(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return attribute?.Value;
        }
    }
}
