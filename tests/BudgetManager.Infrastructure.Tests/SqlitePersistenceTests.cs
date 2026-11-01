using System.Data.Common;
using BudgetManager.Application.Transactions;
using BudgetManager.Domain.Money;
using BudgetManager.Domain.Transactions;
using BudgetManager.Infrastructure.Persistence;
using BudgetManager.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MonetaryAmount = BudgetManager.Domain.Money.Money;

namespace BudgetManager.Infrastructure.Tests;

public sealed class SqlitePersistenceTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task InitialMigrationCreatesExpectedSchemaAndValidatedPragmas()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        await using BudgetManagerDbContext context = database.CreateContext();
        await context.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1L, await ExecuteScalarInt64Async(context, "PRAGMA foreign_keys;"));
        Assert.Equal(10_000L, await ExecuteScalarInt64Async(context, "PRAGMA busy_timeout;"));
        Assert.Equal("wal", await ExecuteScalarStringAsync(context, "PRAGMA journal_mode;"));
        Assert.Equal(6L, await ExecuteScalarInt64Async(
            context,
            """
            SELECT count(*)
            FROM sqlite_master
            WHERE type = 'table'
              AND name IN ('currency', 'account', 'category', 'financial_transaction', 'transaction_leg', 'transaction_split');
            """));
        Assert.Equal(0L, await ExecuteScalarInt64Async(context, "SELECT count(*) FROM pragma_foreign_key_check;"));
    }

    [Fact]
    public async Task ForeignKeysRejectAccountWithUnknownCurrency()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        await using BudgetManagerDbContext context = database.CreateContext();
        context.Accounts.Add(new AccountRecord
        {
            Id = Guid.NewGuid(),
            Name = "Invalid account",
            AccountType = "Checking",
            CurrencyCode = "ZZZ",
        });

        await Assert.ThrowsAsync<DbUpdateException>(
            () => context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task PostingPersistsTransactionLegsAndSplitsAtomically()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        await using BudgetManagerDbContext context = database.CreateContext();
        (Guid accountId, Guid categoryId) = await SeedPostingReferencesAsync(
            context,
            TestContext.Current.CancellationToken);
        Currency sar = new("SAR", 2);
        FinancialTransaction transaction = FinancialTransaction.CreateDraft(
            Guid.NewGuid(),
            TransactionKind.Expense,
            new DateOnly(2026, 11, 1),
            [
                new TransactionLeg(
                    Guid.NewGuid(),
                    accountId,
                    HistoricalValuation.InBaseCurrency(new MonetaryAmount(-12_345, sar))),
            ],
            [
                new CategorySplit(
                    Guid.NewGuid(),
                    categoryId,
                    HistoricalValuation.InBaseCurrency(new MonetaryAmount(12_345, sar))),
            ],
            UtcNow);
        PostFinancialTransactionService service = new(
            new EfAccountPostingProfileReader(context),
            new EfFinancialTransactionStore(context));

        await service.PostAsync(
            transaction,
            UtcNow.AddMinutes(1),
            TestContext.Current.CancellationToken);
        context.ChangeTracker.Clear();

        FinancialTransactionRecord stored = await context.FinancialTransactions
            .AsNoTracking()
            .Include(record => record.Legs)
            .Include(record => record.Splits)
            .SingleAsync(record => record.Id == transaction.Id, TestContext.Current.CancellationToken);
        Assert.Equal("Posted", stored.Status);
        Assert.Equal(1, stored.RowVersion);
        Assert.Equal(-12_345, Assert.Single(stored.Legs).AmountMinorUnits);
        Assert.Equal(12_345, Assert.Single(stored.Splits).AmountMinorUnits);
    }

    [Fact]
    public async Task OptimisticConcurrencyRejectsStaleAccountUpdate()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        Guid accountId;
        await using (BudgetManagerDbContext seedContext = database.CreateContext())
        {
            (accountId, _) = await SeedPostingReferencesAsync(
                seedContext,
                TestContext.Current.CancellationToken);
        }

        await using BudgetManagerDbContext firstContext = database.CreateContext();
        await using BudgetManagerDbContext secondContext = database.CreateContext();
        AccountRecord first = await firstContext.Accounts.SingleAsync(
            account => account.Id == accountId,
            TestContext.Current.CancellationToken);
        AccountRecord stale = await secondContext.Accounts.SingleAsync(
            account => account.Id == accountId,
            TestContext.Current.CancellationToken);

        first.Name = "Primary checking";
        await firstContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        stale.Name = "Stale checking";

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => secondContext.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ActiveRootCategoryUniquenessIsEnforcedWhenParentIsNull()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        await using BudgetManagerDbContext context = database.CreateContext();
        context.Categories.AddRange(
            new CategoryRecord
            {
                Id = Guid.NewGuid(),
                Name = "Groceries",
                Kind = "Expense",
            },
            new CategoryRecord
            {
                Id = Guid.NewGuid(),
                Name = "Groceries",
                Kind = "Expense",
            });

        await Assert.ThrowsAsync<DbUpdateException>(
            () => context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ReapplyingCurrentMigrationsPreservesExistingRecords()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        await using BudgetManagerDbContext context = database.CreateContext();
        context.Currencies.Add(new CurrencyRecord { Code = "SAR", MinorUnitDigits = 2 });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        await context.Database.MigrateAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, await context.Currencies.CountAsync(TestContext.Current.CancellationToken));
    }

    private static async Task<(Guid AccountId, Guid CategoryId)> SeedPostingReferencesAsync(
        BudgetManagerDbContext context,
        CancellationToken cancellationToken)
    {
        Guid accountId = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid();
        context.Currencies.Add(new CurrencyRecord { Code = "SAR", MinorUnitDigits = 2 });
        context.Accounts.Add(new AccountRecord
        {
            Id = accountId,
            Name = "Checking",
            AccountType = "Checking",
            CurrencyCode = "SAR",
        });
        context.Categories.Add(new CategoryRecord
        {
            Id = categoryId,
            Name = "Groceries",
            Kind = "Expense",
        });
        await context.SaveChangesAsync(cancellationToken);
        return (accountId, categoryId);
    }

    private static async Task<long> ExecuteScalarInt64Async(
        BudgetManagerDbContext context,
        string commandText)
    {
        object? result = await ExecuteScalarAsync(context, commandText);
        return Convert.ToInt64(result, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static async Task<string> ExecuteScalarStringAsync(
        BudgetManagerDbContext context,
        string commandText)
    {
        object? result = await ExecuteScalarAsync(context, commandText);
        return Convert.ToString(result, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static async Task<object?> ExecuteScalarAsync(
        BudgetManagerDbContext context,
        string commandText)
    {
        DbConnection connection = context.Database.GetDbConnection();
        await using DbCommand command = connection.CreateCommand();
        command.CommandText = commandText;
        return await command.ExecuteScalarAsync(TestContext.Current.CancellationToken);
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly string _directory;
        private readonly string _connectionString;

        private TestDatabase(string directory)
        {
            _directory = directory;
            _connectionString = $"Data Source={Path.Combine(directory, "test.db")};Foreign Keys=True;Default Timeout=10;Pooling=False";
        }

        public static async Task<TestDatabase> CreateAsync(CancellationToken cancellationToken)
        {
            string directory = Path.Combine(
                Path.GetTempPath(),
                "BudgetManager.Tests",
                Guid.NewGuid().ToString("N", System.Globalization.CultureInfo.InvariantCulture));
            Directory.CreateDirectory(directory);
            TestDatabase database = new(directory);
            await using BudgetManagerDbContext context = database.CreateContext();
            await context.Database.MigrateAsync(cancellationToken);
            return database;
        }

        public BudgetManagerDbContext CreateContext()
        {
            DbContextOptionsBuilder<BudgetManagerDbContext> options = new();
            options
                .UseSqlite(_connectionString)
                .AddInterceptors(new SqliteConnectionConfigurationInterceptor());
            return new BudgetManagerDbContext(options.Options);
        }

        public ValueTask DisposeAsync()
        {
            string expectedParent = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "BudgetManager.Tests"));
            string resolvedDirectory = Path.GetFullPath(_directory);
            string relative = Path.GetRelativePath(expectedParent, resolvedDirectory);
            if (Path.IsPathRooted(relative) || relative.StartsWith("..", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Refusing to delete a test directory outside the expected root.");
            }

            if (Directory.Exists(resolvedDirectory))
            {
                Directory.Delete(resolvedDirectory, recursive: true);
            }

            return ValueTask.CompletedTask;
        }
    }
}
