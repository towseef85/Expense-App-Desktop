using BudgetManager.Domain.Money;
using MonetaryAmount = BudgetManager.Domain.Money.Money;

namespace BudgetManager.Domain.Transactions;

public sealed record TransactionLeg
{
    public TransactionLeg(Guid id, Guid accountId, HistoricalValuation valuation)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A transaction-leg identifier cannot be empty.", nameof(id));
        }

        if (accountId == Guid.Empty)
        {
            throw new ArgumentException("An account identifier cannot be empty.", nameof(accountId));
        }

        ArgumentNullException.ThrowIfNull(valuation);

        Id = id;
        AccountId = accountId;
        Valuation = valuation;
    }

    public Guid Id { get; }

    public Guid AccountId { get; }

    public HistoricalValuation Valuation { get; }

    public MonetaryAmount Amount => Valuation.OriginalAmount;
}
