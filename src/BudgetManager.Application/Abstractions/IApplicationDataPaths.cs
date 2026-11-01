namespace BudgetManager.Application.Abstractions;

public interface IApplicationDataPaths
{
    string RootDirectory { get; }

    string DatabaseFile { get; }

    string AttachmentsDirectory { get; }

    string LogsDirectory { get; }

    string BackupsDirectory { get; }

    string ImportStagingDirectory { get; }

    string RestoreStagingDirectory { get; }
}
