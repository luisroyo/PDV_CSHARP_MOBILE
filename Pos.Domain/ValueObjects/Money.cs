using System;
using Pos.Domain.Entities.Base;

namespace Pos.Domain.ValueObjects
{
    /// <summary>
    /// Value Object para representar valores monetários
    /// </summary>
    public class Money : ValueObject
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }

        private Money() { } // EF Core

        public Money(decimal amount, string currency = "BRL")
        {
            if (amount < 0)
                throw new ArgumentException("Valor monetário não pode ser negativo", nameof(amount));
            
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Moeda não pode ser vazia", nameof(currency));

            Amount = amount;
            Currency = currency;
        }

        public static Money operator +(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException("Não é possível somar valores de moedas diferentes");

            return new Money(left.Amount + right.Amount, left.Currency);
        }

        public static Money operator -(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException("Não é possível subtrair valores de moedas diferentes");

            return new Money(left.Amount - right.Amount, left.Currency);
        }

        public static Money operator *(Money money, decimal multiplier)
        {
            return new Money(money.Amount * multiplier, money.Currency);
        }

        public static Money operator /(Money money, decimal divisor)
        {
            if (divisor == 0)
                throw new DivideByZeroException("Divisor não pode ser zero");

            return new Money(money.Amount / divisor, money.Currency);
        }

        public static bool operator >(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException("Não é possível comparar valores de moedas diferentes");

            return left.Amount > right.Amount;
        }

        public static bool operator <(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException("Não é possível comparar valores de moedas diferentes");

            return left.Amount < right.Amount;
        }

        public static bool operator >=(Money left, Money right)
        {
            return left > right || left == right;
        }

        public static bool operator <=(Money left, Money right)
        {
            return left < right || left == right;
        }

        public static Money Zero(string currency = "BRL")
        {
            return new Money(0, currency);
        }

        public Money Round(int decimals = 2)
        {
            return new Money(Math.Round(Amount, decimals), Currency);
        }

        public override string ToString()
        {
            return $"{Amount:C} {Currency}";
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }
}
