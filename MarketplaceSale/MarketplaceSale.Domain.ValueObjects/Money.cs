using MarketplaceSale.Domain.ValueObjects.Base;
using MarketplaceSale.Domain.ValueObjects.Validators;
using System;

namespace MarketplaceSale.Domain.ValueObjects
{
    public sealed class Money : ValueObject<decimal>
    {
        public Money(decimal value)
            : base(new MoneyValidator(),
                   Math.Round(value, 2, MidpointRounding.AwayFromZero))
        {
        }

        public decimal Amount => Value;

        public override string ToString() => Amount.ToString();

        public static Money operator +(Money m1, Money m2) => new(m1.Value + m2.Value);
        public static Money operator -(Money m1, Money m2) => new(m1.Value - m2.Value);

        public static Money operator *(Money money, int multiplier)
        {
            if (multiplier < 0) throw new ArgumentOutOfRangeException(nameof(multiplier));
            return new Money(money.Value * multiplier);
        }

        public static Money operator /(Money money, int divisor)
        {
            if (divisor <= 0) throw new ArgumentOutOfRangeException(nameof(divisor));
            return new Money(money.Value / divisor);
        }

        public static bool operator >(Money m1, Money m2) => m1.Value > m2.Value;
        public static bool operator <(Money m1, Money m2) => m1.Value < m2.Value;
        public static bool operator >=(Money m1, Money m2) => m1.Value >= m2.Value;
        public static bool operator <=(Money m1, Money m2) => m1.Value <= m2.Value;
    }
}
