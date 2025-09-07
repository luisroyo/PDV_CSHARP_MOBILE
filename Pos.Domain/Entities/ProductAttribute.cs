using System;
using Pos.Domain.Entities.Base;

namespace Pos.Domain.Entities
{
    /// <summary>
    /// Atributo de produto - sistema EAV (Entity-Attribute-Value) para flexibilidade
    /// </summary>
    public class ProductAttribute : Entity
    {
        public Guid ProductId { get; private set; }
        public string Name { get; private set; }
        public string Value { get; private set; }
        public string DataType { get; private set; } // String, Number, Boolean, Date

        private ProductAttribute() { } // EF Core

        public ProductAttribute(Guid productId, string name, string value, string dataType = "String")
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome do atributo não pode ser vazio", nameof(name));

            ProductId = productId;
            Name = name;
            Value = value;
            DataType = dataType;
        }

        public void UpdateValue(string value)
        {
            Value = value;
            MarkAsUpdated();
        }

        public void UpdateDataType(string dataType)
        {
            if (string.IsNullOrWhiteSpace(dataType))
                throw new ArgumentException("Tipo de dados não pode ser vazio", nameof(dataType));

            DataType = dataType;
            MarkAsUpdated();
        }
    }
}
