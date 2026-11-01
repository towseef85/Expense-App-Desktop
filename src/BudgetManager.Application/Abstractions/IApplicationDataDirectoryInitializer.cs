namespace BudgetManager.Application.Abstractions;

public interface IApplicationDataDirectoryInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
