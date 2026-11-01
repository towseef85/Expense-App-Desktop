using BudgetManager.Domain.Money;
using MonetaryAmount = BudgetManager.Domain.Money.Money;

namespace BudgetManager.Domain.Transactions;

public sealed record CategorySplit
{
    public CategorySplit(Guid id, Guid categoryId, HistoricalValuation valuation)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A split identifier cannot be empty.", nameof(id));
        }

        if (categoryId == Guid.Empty)
        {
            throw new ArgumentException("A category identifier cannot be empty.", nameof(categoryId));
        }

        ArgumentNullException.ThrowIfNull(valuation);

        Id = id;
        CategoryId = categoryId;
        Valuation = valuation;
    }

    public Guid Id { get; }

    public Guid CategoryId { get; }

    public HistoricalValuation Valuation { get; }

    public MonetaryAmount Amount => Valuation.OriginalAmount;
}
