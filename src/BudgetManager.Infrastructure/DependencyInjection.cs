using BudgetManager.Application.Abstractions;
using BudgetManager.Infrastructure.Persistence;
using BudgetManager.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IApplicationDataPaths, ApplicationDataPaths>();
        services.AddSingleton<IApplicationDataDirectoryInitializer, ApplicationDataDirectoryInitializer>();
        services.AddSingleton<SqliteConnectionConfigurationInterceptor>();
        services.AddDbContext<BudgetManagerDbContext>((provider, options) =>
        {
            IApplicationDataPaths paths = provider.GetRequiredService<IApplicationDataPaths>();
            options
                .UseSqlite($"Data Source={paths.DatabaseFile};Foreign Keys=True;Default Timeout=10")
                .AddInterceptors(provider.GetRequiredService<SqliteConnectionConfigurationInterceptor>());
        });
        services.AddScoped<IAccountPostingProfileReader, EfAccountPostingProfileReader>();
        services.AddScoped<IFinancialTransactionStore, EfFinancialTransactionStore>();

        return services;
    }
}
