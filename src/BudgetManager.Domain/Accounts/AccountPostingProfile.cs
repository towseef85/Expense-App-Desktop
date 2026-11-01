using BudgetManager.Domain.Money;

namespace BudgetManager.Domain.Accounts;

public sealed record AccountPostingProfile
{
    public AccountPostingProfile(Guid accountId, Currency currency, bool isSavings)
    {
        if (accountId == Guid.Empty)
        {
            throw new ArgumentException("An account identifier cannot be empty.", nameof(accountId));
        }

        ArgumentNullException.ThrowIfNull(currency);

        AccountId = accountId;
        Currency = currency;
        IsSavings = isSavings;
    }

    public Guid AccountId { get; }

    public Currency Currency { get; }

    public bool IsSavings { get; }
}
