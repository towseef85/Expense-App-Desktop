using BudgetManager.Application.Abstractions;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace BudgetManager.Infrastructure.Logging;

public static class LoggingConfiguration
{
    public static LoggerConfiguration Create(IApplicationDataPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "BudgetManager")
            .WriteTo.File(
                new CompactJsonFormatter(),
                Path.Combine(paths.LogsDirectory, "budget-manager-.json"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                restrictedToMinimumLevel: LogEventLevel.Information,
                shared: false);
    }
}
