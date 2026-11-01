using BudgetManager.Application.Abstractions;
using BudgetManager.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BudgetManager.Infrastructure.Tests;

public sealed class ApplicationDataPathsTests
{
    [Fact]
    public async Task InitializeAsyncCreatesOnlyExpectedDirectoriesUnderLocalApplicationData()
    {
        ServiceCollection services = new();
        services.AddInfrastructure();
        await using ServiceProvider provider = services.BuildServiceProvider();
        IApplicationDataPaths paths = provider.GetRequiredService<IApplicationDataPaths>();

        await provider
            .GetRequiredService<IApplicationDataDirectoryInitializer>()
            .InitializeAsync(TestContext.Current.CancellationToken);

        string localApplicationData = Path.GetFullPath(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        string relativeRoot = Path.GetRelativePath(localApplicationData, paths.RootDirectory);
        Assert.False(Path.IsPathRooted(relativeRoot));
        Assert.DoesNotContain("..", relativeRoot, StringComparison.Ordinal);
        Assert.True(Directory.Exists(Path.GetDirectoryName(paths.DatabaseFile)));
        Assert.True(Directory.Exists(paths.AttachmentsDirectory));
        Assert.True(Directory.Exists(paths.LogsDirectory));
        Assert.True(Directory.Exists(paths.BackupsDirectory));
        Assert.True(Directory.Exists(paths.ImportStagingDirectory));
        Assert.True(Directory.Exists(paths.RestoreStagingDirectory));
    }
}
