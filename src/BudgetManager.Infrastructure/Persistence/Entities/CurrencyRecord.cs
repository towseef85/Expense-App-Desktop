namespace BudgetManager.Infrastructure.Persistence.Entities;

public sealed class CurrencyRecord
{
    public required string Code { get; set; }

    public int MinorUnitDigits { get; set; }
}
