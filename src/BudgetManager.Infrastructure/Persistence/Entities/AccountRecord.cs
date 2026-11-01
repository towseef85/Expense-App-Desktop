namespace BudgetManager.Infrastructure.Persistence.Entities;

public sealed class AccountRecord
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string AccountType { get; set; }

    public required string CurrencyCode { get; set; }

    public bool IsSavings { get; set; }

    public DateTimeOffset? ArchivedUtc { get; set; }

    public long RowVersion { get; set; }

    public CurrencyRecord? Currency { get; set; }
}
