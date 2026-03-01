using DotNet.Testcontainers.Containers;
using Hikkaba.Tests.Integration.Constants;
using Hikkaba.Tests.Integration.Models;
using Hikkaba.Tests.Integration.Utils;
using Testcontainers.MsSql;

namespace Hikkaba.Tests.Integration.Services;

public sealed class DbContainerManager
{
    private IContainer? _container;

    private const string MssqlCommandText =
    """
    USE master;

    -- 1. Set Simple Recovery to minimize log overhead
    ALTER DATABASE model SET RECOVERY SIMPLE;

    -- 2. Turn off full ACID compliance (transactions won't wait for disk write)
    ALTER DATABASE model SET DELAYED_DURABILITY = FORCED;

    -- 3. Disable page checksums to save CPU cycles on every write
    ALTER DATABASE model SET PAGE_VERIFY NONE;
    """;

    public async Task<ContainerInfo> StartAsync()
    {
        TestLogUtils.WriteProgressMessage("Starting the DB container...");

        var containerName = "hikkaba_test_db_" + Guid.NewGuid().ToString("D");

        var containerBuilder = new MsSqlBuilder("magicxor/mssql-fts:2025-latest")
            .WithName(containerName)
            .WithStartupCallback(async (container, ct) => await container.ExecScriptAsync(MssqlCommandText, ct))
            .WithExposedPort(TestInfraDefaults.DbPort)
            .WithPassword(TestInfraDefaults.DbPassword)
            .WithAutoRemove(true)
            .WithCleanUp(true);

        _container = containerBuilder.Build();

        using (var cancellationTokenSource = new CancellationTokenSource(TestInfraDefaults.DbStartTimeout))
        {
            var cancellationToken = cancellationTokenSource.Token;
            await _container.StartAsync(cancellationToken);
        }

        var containerHostPort = _container.GetMappedPublicPort(TestInfraDefaults.DbPort);

        TestLogUtils.WriteProgressMessage($"The MsSql container started successfully ({containerName})");

        return new ContainerInfo(containerHostPort, _container.Hostname);
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        TestLogUtils.WriteProgressMessage($"The container is about to stop ({_container?.Name}).");

        if (_container != null)
        {
            _ = Task.Run(() => _container.DisposeAsync(), ct);
        }

        return Task.CompletedTask;
    }
}
