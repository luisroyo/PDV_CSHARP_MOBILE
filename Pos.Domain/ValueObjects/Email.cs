using System;
using System.Text.RegularExpressions;
using Pos.Domain.Entities.Base;

namespace Pos.Domain.ValueObjects
{
    /// <summary>
    /// Value Object para representar endereços de email
    /// </summary>
    public class Email : ValueObject
    {
        public string Value { get; private set; }

        private static readonly Regex EmailRegex = new(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private Email() { } // EF Core

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email não pode ser vazio", nameof(value));

            if (!IsValid(value))
                throw new ArgumentException("Formato de email inválido", nameof(value));

            Value = value.ToLowerInvariant();
        }

        public static bool IsValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return EmailRegex.IsMatch(email);
        }

        public static implicit operator string(Email email)
        {
            return email?.Value;
        }

        public static implicit operator Email(string email)
        {
            return email == null ? null : new Email(email);
        }

        public override string ToString()
        {
            return Value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
