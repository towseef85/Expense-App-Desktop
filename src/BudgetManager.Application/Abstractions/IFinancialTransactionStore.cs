using BudgetManager.Domain.Transactions;

namespace BudgetManager.Application.Abstractions;

public interface IFinancialTransactionStore
{
    /// <summary>
    /// Persists the complete posted transaction aggregate atomically.
    /// </summary>
    Task SavePostedAsync(
        FinancialTransaction transaction,
        CancellationToken cancellationToken = default);
}
