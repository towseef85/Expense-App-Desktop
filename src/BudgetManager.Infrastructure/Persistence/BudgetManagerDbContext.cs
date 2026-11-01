using BudgetManager.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BudgetManager.Infrastructure.Persistence;

public sealed class BudgetManagerDbContext(DbContextOptions<BudgetManagerDbContext> options)
    : DbContext(options)
{
    public DbSet<CurrencyRecord> Currencies => Set<CurrencyRecord>();

    public DbSet<AccountRecord> Accounts => Set<AccountRecord>();

    public DbSet<CategoryRecord> Categories => Set<CategoryRecord>();

    public DbSet<FinancialTransactionRecord> FinancialTransactions => Set<FinancialTransactionRecord>();

    public DbSet<TransactionLegRecord> TransactionLegs => Set<TransactionLegRecord>();

    public DbSet<TransactionSplitRecord> TransactionSplits => Set<TransactionSplitRecord>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Guid>().HaveConversion<GuidToBytesConverter>();
        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BudgetManagerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        IncrementConcurrencyTokens();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        IncrementConcurrencyTokens();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    private void IncrementConcurrencyTokens()
    {
        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<AccountRecord> entry
                 in ChangeTracker.Entries<AccountRecord>())
        {
            Increment(entry.State, entry.Entity.RowVersion, value => entry.Entity.RowVersion = value);
        }

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<CategoryRecord> entry
                 in ChangeTracker.Entries<CategoryRecord>())
        {
            Increment(entry.State, entry.Entity.RowVersion, value => entry.Entity.RowVersion = value);
        }

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<FinancialTransactionRecord> entry
                 in ChangeTracker.Entries<FinancialTransactionRecord>())
        {
            Increment(entry.State, entry.Entity.RowVersion, value => entry.Entity.RowVersion = value);
        }
    }

    private static void Increment(EntityState state, long currentValue, Action<long> assign)
    {
        if (state == EntityState.Added)
        {
            assign(1);
        }
        else if (state == EntityState.Modified)
        {
            assign(checked(currentValue + 1));
        }
    }
}
