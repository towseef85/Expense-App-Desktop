using BudgetManager.Domain.Common;

namespace BudgetManager.Domain.Money;

public sealed record HistoricalValuation
{
    private HistoricalValuation(
        Money originalAmount,
        Currency baseCurrency,
        long baseAmountMinorUnits,
        decimal quoteUnitsPerBaseUnit)
    {
        OriginalAmount = originalAmount;
        BaseCurrency = baseCurrency;
        BaseAmountMinorUnits = baseAmountMinorUnits;
        QuoteUnitsPerBaseUnit = quoteUnitsPerBaseUnit;
    }

    public Money OriginalAmount { get; }

    public Currency BaseCurrency { get; }

    public long BaseAmountMinorUnits { get; }

    /// <summary>
    /// Number of original-currency major units per one base-currency major unit.
    /// </summary>
    public decimal QuoteUnitsPerBaseUnit { get; }

    public static HistoricalValuation InBaseCurrency(Money amount)
    {
        ArgumentNullException.ThrowIfNull(amount);
        return new HistoricalValuation(amount, amount.Currency, amount.MinorUnits, 1m);
    }

    public static HistoricalValuation Convert(
        Money originalAmount,
        Currency baseCurrency,
        decimal quoteUnitsPerBaseUnit)
    {
        ArgumentNullException.ThrowIfNull(originalAmount);
        ArgumentNullException.ThrowIfNull(baseCurrency);

        if (originalAmount.Currency == baseCurrency)
        {
            if (quoteUnitsPerBaseUnit != 1m)
            {
                throw new DomainRuleViolationException(
                    "valuation.base_rate_must_be_one",
                    "A base-currency valuation must use an exchange rate of one.");
            }

            return InBaseCurrency(originalAmount);
        }

        if (quoteUnitsPerBaseUnit <= 0m)
        {
            throw new DomainRuleViolationException(
                "valuation.rate_must_be_positive",
                "An exchange rate must be greater than zero.");
        }

        decimal baseMajorUnits = originalAmount.ToMajorUnits() / quoteUnitsPerBaseUnit;
        Money baseAmount = Money.FromMajorUnits(baseMajorUnits, baseCurrency);
        return new HistoricalValuation(
            originalAmount,
            baseCurrency,
            baseAmount.MinorUnits,
            quoteUnitsPerBaseUnit);
    }
}
