using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Hikkaba.Tests.Integration.Exceptions;
using Hikkaba.Tests.Integration.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nito.AsyncEx;
using Respawn;
using Table = Respawn.Graph.Table;

namespace Hikkaba.Tests.Integration.Services;

public sealed class RespawnableContextManager<TContext> : IDisposable
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
    private bool _isFirstTimeRespawnCalled = true;

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

    private async Task ResetDatabaseAsync()
    {
        TestLogUtils.WriteProgressMessage($"{nameof(ResetDatabaseAsync)} on {typeof(TContext).Name} {_dbConnection?.ConnectionString}: running...");

        var respawner = await _respawnerLazy;
        await respawner.ResetAsync(_dbConnection ?? throw new IntegrationTestException("DbConnection passed to Respawner is null"));

        TestLogUtils.WriteProgressMessage($"{nameof(ResetDatabaseAsync)} on {typeof(TContext).Name} {_dbConnection?.ConnectionString}: OK");
    }

    public async Task<string> CreateRespawnedDbConnectionStringAsync()
    {
        TestLogUtils.WriteProgressMessage($"{nameof(CreateRespawnedDbConnectionStringAsync)} on {typeof(TContext).Name}: running...");

        // no need to reset the database on the first run
        if (!_isFirstTimeRespawnCalled)
        {
            TestLogUtils.WriteProgressMessage($"It's NOT the first time {nameof(CreateRespawnedDbConnectionStringAsync)} is called on {typeof(TContext).Name}; resetting the database...");

            await ResetDatabaseAsync();
        }
        else
        {
            TestLogUtils.WriteProgressMessage($"It's the FIRST time {nameof(CreateRespawnedDbConnectionStringAsync)} is called on {typeof(TContext).Name}; no need to reset the database");

            _isFirstTimeRespawnCalled = false;
        }

        return _dbConnectionString;
    }

    public async Task DeinitializeAsync()
    {
        TestLogUtils.WriteProgressMessage($"{nameof(DeinitializeAsync)} on {typeof(TContext).Name} {_dbConnection?.ConnectionString}: running...");

        if (_dbConnection != null)
        {
            if (_dbConnection.State == ConnectionState.Open)
            {
                await _dbConnection.CloseAsync();
            }

            await _dbConnection.DisposeAsync();
            _dbConnection = null;
        }

        await using var dbContext = _dbContextCreator(_dbConnectionString);
        await dbContext.Database.EnsureDeletedAsync();

        TestLogUtils.WriteProgressMessage($"{nameof(DeinitializeAsync)} on {typeof(TContext).Name}: OK");
    }

    public void Dispose()
    {
        _dbConnection?.Dispose();
    }
}
