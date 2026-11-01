namespace BudgetManager.Infrastructure.Persistence.Entities;

public sealed class TransactionLegRecord
{
    public Guid Id { get; set; }

    public Guid TransactionId { get; set; }

    public Guid AccountId { get; set; }

    public long AmountMinorUnits { get; set; }

    public required string CurrencyCode { get; set; }

    public long BaseAmountMinorUnits { get; set; }

    public required string BaseCurrencyCode { get; set; }

    public decimal QuoteUnitsPerBaseUnit { get; set; }

    public FinancialTransactionRecord? Transaction { get; set; }

    public AccountRecord? Account { get; set; }
}
