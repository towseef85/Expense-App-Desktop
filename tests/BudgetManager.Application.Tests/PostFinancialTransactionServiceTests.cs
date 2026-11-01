using BudgetManager.Application.Abstractions;
using BudgetManager.Application.Transactions;
using BudgetManager.Domain.Accounts;
using BudgetManager.Domain.Money;
using BudgetManager.Domain.Transactions;
using Xunit;
using MonetaryAmount = BudgetManager.Domain.Money.Money;

namespace BudgetManager.Application.Tests;

public sealed class PostFinancialTransactionServiceTests
{
    [Fact]
    public async Task PostAsyncValidatesAndSavesCompletePostedAggregate()
    {
        Currency sar = new("SAR", 2);
        Guid accountId = Guid.NewGuid();
        FinancialTransaction transaction = FinancialTransaction.CreateDraft(
            Guid.NewGuid(),
            TransactionKind.Expense,
            new DateOnly(2026, 11, 1),
            [
                new TransactionLeg(
                    Guid.NewGuid(),
                    accountId,
                    HistoricalValuation.InBaseCurrency(new MonetaryAmount(-1_000, sar))),
            ],
            [
                new CategorySplit(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    HistoricalValuation.InBaseCurrency(new MonetaryAmount(1_000, sar))),
            ],
            new DateTimeOffset(2026, 11, 1, 10, 0, 0, TimeSpan.Zero));
        RecordingStore store = new();
        StubAccountReader accountReader = new(new AccountPostingProfile(accountId, sar, isSavings: false));
        PostFinancialTransactionService service = new(accountReader, store);

        await service.PostAsync(
            transaction,
            new DateTimeOffset(2026, 11, 1, 10, 1, 0, TimeSpan.Zero),
            TestContext.Current.CancellationToken);

        Assert.Same(transaction, store.SavedTransaction);
        Assert.Equal(TransactionStatus.Posted, store.SavedTransaction?.Status);
    }

    private sealed class StubAccountReader(AccountPostingProfile profile) : IAccountPostingProfileReader
    {
        public Task<IReadOnlyDictionary<Guid, AccountPostingProfile>> GetAsync(
            IReadOnlyCollection<Guid> accountIds,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.Contains(profile.AccountId, accountIds);
            IReadOnlyDictionary<Guid, AccountPostingProfile> result =
                new Dictionary<Guid, AccountPostingProfile> { [profile.AccountId] = profile };
            return Task.FromResult(result);
        }
    }

    private sealed class RecordingStore : IFinancialTransactionStore
    {
        public FinancialTransaction? SavedTransaction { get; private set; }

        public Task SavePostedAsync(
            FinancialTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.Equal(TransactionStatus.Posted, transaction.Status);
            SavedTransaction = transaction;
            return Task.CompletedTask;
        }
    }
}
