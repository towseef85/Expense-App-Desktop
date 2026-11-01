using BudgetManager.Application.Abstractions;
using BudgetManager.Domain.Accounts;
using BudgetManager.Domain.Transactions;

namespace BudgetManager.Application.Transactions;

public sealed class PostFinancialTransactionService(
    IAccountPostingProfileReader accountReader,
    IFinancialTransactionStore transactionStore)
{
    public async Task PostAsync(
        FinancialTransaction transaction,
        DateTimeOffset postedUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        Guid[] accountIds = transaction.Legs
            .Select(leg => leg.AccountId)
            .Distinct()
            .ToArray();
        IReadOnlyDictionary<Guid, AccountPostingProfile> accounts =
            await accountReader.GetAsync(accountIds, cancellationToken).ConfigureAwait(false);

        transaction.Post(accounts, postedUtc);
        await transactionStore.SavePostedAsync(transaction, cancellationToken).ConfigureAwait(false);
    }
}
