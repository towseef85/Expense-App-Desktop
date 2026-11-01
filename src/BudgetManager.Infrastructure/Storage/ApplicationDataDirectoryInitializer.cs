using BudgetManager.Application.Abstractions;

namespace BudgetManager.Infrastructure.Storage;

public sealed class ApplicationDataDirectoryInitializer(IApplicationDataPaths paths)
    : IApplicationDataDirectoryInitializer
{
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string? databaseDirectory = Path.GetDirectoryName(paths.DatabaseFile);
        if (string.IsNullOrWhiteSpace(databaseDirectory))
        {
            throw new InvalidOperationException("The configured database file has no parent directory.");
        }

        string[] directories =
        [
            paths.RootDirectory,
            databaseDirectory,
            paths.AttachmentsDirectory,
            paths.LogsDirectory,
            paths.BackupsDirectory,
            paths.ImportStagingDirectory,
            paths.RestoreStagingDirectory,
        ];

        foreach (string directory in directories)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Directory.CreateDirectory(directory);
        }

        return Task.CompletedTask;
    }
}
