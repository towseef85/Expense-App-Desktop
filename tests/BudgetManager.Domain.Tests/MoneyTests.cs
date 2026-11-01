using BudgetManager.Domain.Common;
using BudgetManager.Domain.Money;
using Xunit;
using MonetaryAmount = BudgetManager.Domain.Money.Money;

namespace BudgetManager.Domain.Tests;

public sealed class MoneyTests
{
    private static readonly Currency Sar = new("SAR", 2);

    [Theory]
    [InlineData("1.005", 101)]
    [InlineData("-1.005", -101)]
    [InlineData("1.004", 100)]
    [InlineData("-1.004", -100)]
    public void FromMajorUnitsRoundsMidpointsAwayFromZero(string input, long expectedMinorUnits)
    {
        MonetaryAmount result = MonetaryAmount.FromMajorUnits(
            decimal.Parse(input, System.Globalization.CultureInfo.InvariantCulture),
            Sar);

        Assert.Equal(expectedMinorUnits, result.MinorUnits);
    }

    [Fact]
    public void AddRejectsDifferentCurrencies()
    {
        MonetaryAmount sar = new(100, Sar);
        MonetaryAmount usd = new(100, new Currency("USD", 2));

        DomainRuleViolationException exception = Assert.Throws<DomainRuleViolationException>(() => sar.Add(usd));

        Assert.Equal("money.currency_mismatch", exception.Rule);
    }

    [Fact]
    public void HistoricalValuationPreservesPostingRateAndBaseAmount()
    {
        Currency usd = new("USD", 2);
        HistoricalValuation valuation = HistoricalValuation.Convert(new MonetaryAmount(-1_000, usd), Sar, 0.25m);

        Assert.Equal(-1_000, valuation.OriginalAmount.MinorUnits);
        Assert.Equal(usd, valuation.OriginalAmount.Currency);
        Assert.Equal(-4_000, valuation.BaseAmountMinorUnits);
        Assert.Equal(0.25m, valuation.QuoteUnitsPerBaseUnit);
    }
}
