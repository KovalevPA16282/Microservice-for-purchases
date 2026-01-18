using MarketplaceSale.Domain.ValueObjects.Base;
using MarketplaceSale.Domain.ValueObjects.Exceptions;
using System;

namespace MarketplaceSale.Domain.ValueObjects.Validators
{
    internal sealed class MoneyValidator : IValidator<decimal>
    {
        public void Validate(decimal value)
        {
            if (value < 0)
                throw new MoneyNonPositiveException(
                    ExceptionMessages.MONEY_MUST_BE_POSITIVE,
                    nameof(value),
                    value);

            // Если Money всегда округляет сам (как в Money.cs), эта проверка не обязательна.
            // Оставить можно, но тогда лучше явно указать AwayFromZero:
            if (decimal.Round(value, 2, MidpointRounding.AwayFromZero) != value)
                throw new MoneyHasMoreThanTwoDecimalPlacesException(
                    ExceptionMessages.MONEY_MAX_TWO_DECIMAL_PLACES,
                    nameof(value),
                    value);
        }
    }
}
