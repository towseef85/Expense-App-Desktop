using BudgetManager.Application;
using BudgetManager.Application.Abstractions;
using BudgetManager.Infrastructure;
using BudgetManager.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Serilog;

namespace BudgetManager.App;

public partial class App : Microsoft.UI.Xaml.Application
{
    private readonly IHost _host;
    private Window? _window;

    public App()
    {
        InitializeComponent();

        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure();
        builder.Services.AddSingleton<MainWindow>();

        ServiceProvider bootstrapProvider = builder.Services.BuildServiceProvider();
        IApplicationDataPaths paths = bootstrapProvider.GetRequiredService<IApplicationDataPaths>();
        Log.Logger = LoggingConfiguration.Create(paths).CreateLogger();
        bootstrapProvider.Dispose();

        builder.Services.AddSerilog(Log.Logger, dispose: true);
        _host = builder.Build();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            await _host.Services
                .GetRequiredService<IApplicationDataDirectoryInitializer>()
                .InitializeAsync();

            _window = _host.Services.GetRequiredService<MainWindow>();
            _window.Activate();
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "Application startup failed");
            throw;
        }
    }
}
