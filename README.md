# Budget Manager

Budget Manager is an offline-first Windows personal-finance application built with WinUI 3, .NET 10, EF Core, and SQLite. The repository currently contains the solution foundation and the Milestone 2 financial domain. Persistence and end-user ledger workflows are not implemented yet.

## Prerequisites

- Windows 10 version 1809 or later; use a currently supported Windows edition for production.
- .NET SDK 10.0.403 or a compatible 10.0 patch SDK.
- Visual Studio 2026 18.0 or later with Windows application development tools for XAML editing, debugging, and later MSIX packaging.
- Windows SDK 10.0.26100 or later.

The application is designed for self-contained release deployment. End users will not need to install .NET, SQLite, or a database server.

## Build and test

```powershell
dotnet restore BudgetManager.slnx
dotnet build BudgetManager.slnx --configuration Release --no-restore
dotnet test BudgetManager.slnx --configuration Release --no-build
dotnet format BudgetManager.slnx --verify-no-changes --no-restore
```

## Run

```powershell
dotnet run --project src/BudgetManager.App/BudgetManager.App.csproj --configuration Debug --framework net10.0-windows10.0.26100.0
```

The shell contains navigation placeholders only. Running it creates the application directories but does not create a database or financial records.

## Data locations

All application data is below `%LOCALAPPDATA%\BudgetManager`:

- `database\budget-manager.db`
- `attachments`
- `logs`
- `backups`
- `staging\imports`
- `staging\restore`

Logs are local structured JSON with rolling retention. Code must not log authentication data, complete account identifiers, transaction descriptions, imported statement rows, or attachment contents.

## Database migrations

The context shell deliberately has no financial entities or migration. The initial migration will be created in Milestone 3 after the Milestone 2 domain model is approved. Production startup must never use `EnsureCreated`.

Planned migration commands will use `BudgetManager.Infrastructure` as the migration project and `BudgetManager.App` as the startup project.

## Packaging direction

The approved release format is self-contained MSIX for `win-x64` and `win-arm64`. The app already opts into self-contained Windows App SDK deployment. Package identity, signing, update channel, final assets, and clean-machine installation tests are deliberately deferred to Milestone 11.

## Architecture

See [ADR 0001](docs/architecture/ADR-0001-foundation-decisions.md) for the foundation decisions and constraints.
