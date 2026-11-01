using BudgetManager.Application.Abstractions;

namespace BudgetManager.Infrastructure.Storage;

public sealed class ApplicationDataPaths : IApplicationDataPaths
{
    private const string ApplicationDirectoryName = "BudgetManager";

    public ApplicationDataPaths()
        : this(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
    {
    }

    internal ApplicationDataPaths(string localApplicationDataDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localApplicationDataDirectory);

        string fullLocalRoot = Path.GetFullPath(localApplicationDataDirectory);
        RootDirectory = Path.GetFullPath(Path.Combine(fullLocalRoot, ApplicationDirectoryName));

        if (!IsDescendantOf(RootDirectory, fullLocalRoot))
        {
            throw new InvalidOperationException("The application data root must remain under LocalApplicationData.");
        }

        DatabaseFile = Path.Combine(RootDirectory, "database", "budget-manager.db");
        AttachmentsDirectory = Path.Combine(RootDirectory, "attachments");
        LogsDirectory = Path.Combine(RootDirectory, "logs");
        BackupsDirectory = Path.Combine(RootDirectory, "backups");
        ImportStagingDirectory = Path.Combine(RootDirectory, "staging", "imports");
        RestoreStagingDirectory = Path.Combine(RootDirectory, "staging", "restore");
    }

    public string RootDirectory { get; }

    public string DatabaseFile { get; }

    public string AttachmentsDirectory { get; }

    public string LogsDirectory { get; }

    public string BackupsDirectory { get; }

    public string ImportStagingDirectory { get; }

    public string RestoreStagingDirectory { get; }

    private static bool IsDescendantOf(string candidate, string parent)
    {
        string relative = Path.GetRelativePath(parent, candidate);
        return !Path.IsPathRooted(relative)
            && !relative.Equals("..", StringComparison.Ordinal)
            && !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
    }
}
