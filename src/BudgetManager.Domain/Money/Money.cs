using BudgetManager.Domain.Common;

namespace BudgetManager.Domain.Money;

public sealed record Money
{
    public Money(long minorUnits, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);
        MinorUnits = minorUnits;
        Currency = currency;
    }

    public long MinorUnits { get; }

    public Currency Currency { get; }

    public static Money Zero(Currency currency) => new(0, currency);

    public static Money FromMajorUnits(decimal amount, Currency currency)
    {
        decimal scaled = amount * currency.MinorUnitsPerMajorUnit;
        decimal rounded = decimal.Round(scaled, 0, MidpointRounding.AwayFromZero);
        return new Money(checked((long)rounded), currency);
    }

    public decimal ToMajorUnits() => MinorUnits / (decimal)Currency.MinorUnitsPerMajorUnit;

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(checked(MinorUnits + other.MinorUnits), Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(checked(MinorUnits - other.MinorUnits), Currency);
    }

    public Money Negate() => new(checked(-MinorUnits), Currency);

    public Money Absolute()
    {
        if (MinorUnits == long.MinValue)
        {
            throw new OverflowException("The minimum Int64 monetary amount cannot be made absolute.");
        }

        return new Money(Math.Abs(MinorUnits), Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new DomainRuleViolationException(
                "money.currency_mismatch",
                $"Money in {Currency.Code} cannot be combined with money in {other.Currency.Code}.");
        }
    }
}
