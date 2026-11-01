using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BudgetManager.Infrastructure.Persistence;

public sealed class SqliteConnectionConfigurationInterceptor : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        base.ConnectionOpened(connection, eventData);

        if (connection is not SqliteConnection)
        {
            return;
        }

        ExecutePragma(connection, "PRAGMA foreign_keys = ON;");
        ExecutePragma(connection, "PRAGMA busy_timeout = 10000;");
        ExecutePragma(connection, "PRAGMA journal_mode = WAL;");
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken).ConfigureAwait(false);

        if (connection is not SqliteConnection)
        {
            return;
        }

        await ExecutePragmaAsync(connection, "PRAGMA foreign_keys = ON;", cancellationToken)
            .ConfigureAwait(false);
        await ExecutePragmaAsync(connection, "PRAGMA busy_timeout = 10000;", cancellationToken)
            .ConfigureAwait(false);
        await ExecutePragmaAsync(connection, "PRAGMA journal_mode = WAL;", cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task ExecutePragmaAsync(
        DbConnection connection,
        string commandText,
        CancellationToken cancellationToken)
    {
        await using DbCommand command = connection.CreateCommand();
        command.CommandText = commandText;
        await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void ExecutePragma(DbConnection connection, string commandText)
    {
        using DbCommand command = connection.CreateCommand();
        command.CommandText = commandText;
        command.ExecuteScalar();
    }
}
