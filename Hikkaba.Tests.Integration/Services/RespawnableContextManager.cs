using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Hikkaba.Tests.Integration.Exceptions;
using Hikkaba.Tests.Integration.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nito.AsyncEx;
using Respawn;
using Table = Respawn.Graph.Table;

namespace Hikkaba.Tests.Integration.Services;

public sealed class RespawnableContextManager<TContext> : IDisposable, IAsyncDisposable
    where TContext : DbContext
{
    private readonly RespawnerOptions _respawnerOptions = new()
    {
        DbAdapter = DbAdapter.SqlServer,
        SchemasToInclude = ["dbo"],
        TablesToIgnore = [new Table("dbo", "__EFMigrationsHistory")],
    };

    private readonly string _dbConnectionString;
    private readonly Func<string, TContext> _dbContextCreator;
    private readonly AsyncLazy<Respawner> _respawnerLazy;

    private DbConnection? _dbConnection;
    private int _isFirstTimeRespawnCalled = 1; // 0 = false, 1 = true
    private int _disposed; // 0 = false, 1 = true

    public bool IsDisposed => Volatile.Read(ref _disposed) == 1;

    public RespawnableContextManager(
        string dbConnectionString,
        Func<string, TContext> dbContextCreator)
    {
        _dbConnectionString = dbConnectionString;
        _dbContextCreator = dbContextCreator;

        _respawnerLazy = new AsyncLazy<Respawner>(async () =>
        {
            await using var dbContext = _dbContextCreator(_dbConnectionString);
            _dbConnection = new SqlConnection(dbContext.Database.GetConnectionString());
            await _dbConnection.OpenAsync();
            return await Respawner.CreateAsync(_dbConnection, _respawnerOptions);
        });
    }

    private async Task<bool> IsDbPresentAsync()
    {
        try
        {
            await using var dbContext = _dbContextCreator(_dbConnectionString);
            return await dbContext.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            TestLogUtils.WriteProgressMessage($"Failed to connect to database for {typeof(TContext).Name} at connection string {_dbConnectionString}: {ex}");
            return false;
        }
    }

    /// <summary>
    /// Resets the database to its initial state.
    /// </summary>
    /// <exception cref="IntegrationTestException">Thrown when the database connection is null.</exception>
    private async Task ResetDatabaseAsync()
    {
        TestLogUtils.WriteProgressMessage($"{nameof(ResetDatabaseAsync)} on {typeof(TContext).Name}, connection = {_dbConnection?.ConnectionString}: running...");

        var isDbPresent = await IsDbPresentAsync();
        if (!isDbPresent)
        {
            TestLogUtils.WriteProgressMessage($"WARNING: Database is not present for {typeof(TContext).Name} at connection string {_dbConnectionString}. Skipping ResetDatabaseAsync.");
            return;
        }

        var respawner = await _respawnerLazy;
        await respawner.ResetAsync(_dbConnection ?? throw new IntegrationTestException("DbConnection passed to Respawner is null"));

        TestLogUtils.WriteProgressMessage($"{nameof(ResetDatabaseAsync)} on {typeof(TContext).Name}, connection = {_dbConnection.ConnectionString}: OK");
    }

    /// <summary>
    /// Creates or respawns the database and returns its connection string.
    /// </summary>
    /// <returns>The connection string to the created or respawned database.</returns>
    public async Task<string> CreateRespawnedDbConnectionStringAsync()
    {
        TestLogUtils.WriteProgressMessage($"{nameof(CreateRespawnedDbConnectionStringAsync)} on {typeof(TContext).Name}: running...");

        // Atomically check and reset the flag: returns 1 on first call, 0 on subsequent calls
        var wasFirstTime = Interlocked.Exchange(ref _isFirstTimeRespawnCalled, 0) == 1;

        if (!wasFirstTime)
        {
            TestLogUtils.WriteProgressMessage($"It's NOT the first time {nameof(CreateRespawnedDbConnectionStringAsync)} is called on {typeof(TContext).Name}; resetting the database...");

            await ResetDatabaseAsync();
        }
        else
        {
            TestLogUtils.WriteProgressMessage($"It's the FIRST time {nameof(CreateRespawnedDbConnectionStringAsync)} is called on {typeof(TContext).Name}; no need to reset the database");
        }

        return _dbConnectionString;
    }

    /// <summary>
    /// Disposes the context manager and deletes the database.
    /// <br />
    /// WARNING: Do NOT dispose pooled objects manually.
    /// ObjectPool will call Dispose automatically when needed.
    /// </summary>
    /// <remarks>
    /// see:
    /// https://github.com/dotnet/aspnetcore/blob/d8688781d3b15395b20a86cd0bca77065f538f0b/src/ObjectPool/src/DisposableObjectPool.cs
    /// https://github.com/dotnet/extensions/issues/973
    /// https://github.com/dotnet/extensions/pull/977
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        // Atomically check and set the disposed flag
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        TestLogUtils.WriteProgressMessage($"{nameof(DisposeAsync)} on {typeof(TContext).Name} {_dbConnection?.ConnectionString}: running...");

        if (_dbConnection != null)
        {
            try
            {
                if (_dbConnection.State == ConnectionState.Open)
                {
                    await _dbConnection.CloseAsync();
                }

                await _dbConnection.DisposeAsync();
            }
            catch (Exception ex)
            {
                // Dispose should not throw exceptions per Microsoft guidelines
                TestLogUtils.WriteProgressMessage($"{nameof(DisposeAsync)} on {typeof(TContext).Name}: failed to close/dispose connection: {ex}");
            }
            finally
            {
                _dbConnection = null;
            }
        }

        try
        {
            await using var dbContext = _dbContextCreator(_dbConnectionString);
            await dbContext.Database.EnsureDeletedAsync();
        }
        catch (Exception ex)
        {
            // Dispose should not throw exceptions per Microsoft guidelines
            TestLogUtils.WriteProgressMessage($"{nameof(DisposeAsync)} on {typeof(TContext).Name}: failed to delete database: {ex}");
        }

        TestLogUtils.WriteProgressMessage($"{nameof(DisposeAsync)} on {typeof(TContext).Name}: OK");
    }

    /// <summary>
    /// Disposes the context manager and deletes the database.
    /// <br />
    /// WARNING: Do NOT dispose pooled objects manually.
    /// ObjectPool will call Dispose automatically when needed.
    /// </summary>
    /// <remarks>
    /// see:
    /// https://github.com/dotnet/aspnetcore/blob/d8688781d3b15395b20a86cd0bca77065f538f0b/src/ObjectPool/src/DisposableObjectPool.cs
    /// https://github.com/dotnet/extensions/issues/973
    /// https://github.com/dotnet/extensions/pull/977
    /// </remarks>
    [SuppressMessage("Reliability", "CA2012:Use ValueTasks correctly", Justification = "Dispose calls DisposeAsync in a blocking way on purpose")]
    public void Dispose()
    {
        // DisposeAsync handles the atomicity check internally
        AsyncContext.Run(DisposeAsync);
    }
}
