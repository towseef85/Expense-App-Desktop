using BudgetManager.Application.Abstractions;
using BudgetManager.Domain.Accounts;
using BudgetManager.Domain.Money;
using Microsoft.EntityFrameworkCore;

namespace BudgetManager.Infrastructure.Persistence;

public sealed class EfAccountPostingProfileReader(BudgetManagerDbContext dbContext)
    : IAccountPostingProfileReader
{
    public async Task<IReadOnlyDictionary<Guid, AccountPostingProfile>> GetAsync(
        IReadOnlyCollection<Guid> accountIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(accountIds);

        var records = await dbContext.Accounts
            .AsNoTracking()
            .Where(account => accountIds.Contains(account.Id))
            .Join(
                dbContext.Currencies,
                account => account.CurrencyCode,
                currency => currency.Code,
                (account, currency) => new
                {
                    account.Id,
                    account.IsSavings,
                    currency.Code,
                    currency.MinorUnitDigits,
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records.ToDictionary(
            record => record.Id,
            record => new AccountPostingProfile(
                record.Id,
                new Currency(record.Code, record.MinorUnitDigits),
                record.IsSavings));
    }
}
