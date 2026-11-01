using BudgetManager.Domain;
using Xunit;

namespace BudgetManager.Domain.Tests;

public sealed class ArchitectureTests
{
    [Fact]
    public void DomainAssemblyHasNoProjectDependencies()
    {
        string[] referencedAssemblies = typeof(DomainAssemblyMarker).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(referencedAssemblies, name => name.StartsWith("BudgetManager.", StringComparison.Ordinal));
    }
}
