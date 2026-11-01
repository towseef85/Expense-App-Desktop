namespace BudgetManager.Infrastructure.Persistence.Entities;

public sealed class TransactionSplitRecord
{
    public Guid Id { get; set; }

    public Guid TransactionId { get; set; }

    public Guid CategoryId { get; set; }

    public long AmountMinorUnits { get; set; }

    public required string CurrencyCode { get; set; }

    public long BaseAmountMinorUnits { get; set; }

    public required string BaseCurrencyCode { get; set; }

    public decimal QuoteUnitsPerBaseUnit { get; set; }

    public FinancialTransactionRecord? Transaction { get; set; }

    public CategoryRecord? Category { get; set; }
}
