namespace BudgetManager.Infrastructure.Persistence.Entities;

public sealed class CategoryRecord
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Kind { get; set; }

    public Guid? ParentId { get; set; }

    public DateTimeOffset? ArchivedUtc { get; set; }

    public long RowVersion { get; set; }

    public CategoryRecord? Parent { get; set; }
}
