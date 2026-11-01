using BudgetManager.Domain.Accounts;

namespace BudgetManager.Application.Abstractions;

public interface IAccountPostingProfileReader
{
    Task<IReadOnlyDictionary<Guid, AccountPostingProfile>> GetAsync(
        IReadOnlyCollection<Guid> accountIds,
        CancellationToken cancellationToken = default);
}
