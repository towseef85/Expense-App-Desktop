using BudgetManager.Application;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BudgetManager.Application.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddApplicationReturnsTheSameServiceCollection()
    {
        ServiceCollection services = new();

        IServiceCollection result = services.AddApplication();

        Assert.Same(services, result);
    }
}
