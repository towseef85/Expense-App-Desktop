namespace BudgetManager.Infrastructure.Persistence.Entities;

public sealed class FinancialTransactionRecord
{
    public Guid Id { get; set; }

    public required string Kind { get; set; }

    public required string Status { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public DateTimeOffset CreatedUtc { get; set; }

    public DateTimeOffset ModifiedUtc { get; set; }

    public DateTimeOffset? PostedUtc { get; set; }

    public DateTimeOffset? VoidedUtc { get; set; }

    public Guid? CorrectionOfTransactionId { get; set; }

    public Guid? ReconciliationId { get; set; }

    public long SplitBaseRoundingDifferenceMinorUnits { get; set; }

    public bool IsSavingsTransfer { get; set; }

    public long RowVersion { get; set; }

    public FinancialTransactionRecord? CorrectionOfTransaction { get; set; }

    public List<TransactionLegRecord> Legs { get; } = [];

    public List<TransactionSplitRecord> Splits { get; } = [];
}
