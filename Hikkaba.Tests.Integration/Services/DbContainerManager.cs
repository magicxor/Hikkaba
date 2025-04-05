using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using Hikkaba.Tests.Integration.Constants;
using Hikkaba.Tests.Integration.Models;
using Hikkaba.Tests.Integration.Utils;
using Testcontainers.MsSql;

namespace Hikkaba.Tests.Integration.Services;

public sealed class DbContainerManager
{
    private IContainer? _container;

    public async Task<ContainerInfo> StartAsync()
    {
        TestLogUtils.WriteProgressMessage("Starting the DB container...");

        var containerName = "hikkaba_test_db_" + Guid.NewGuid().ToString("D");

        var containerBuilder = new MsSqlBuilder()
            .WithName(containerName)
            .WithImage("magicxor/mssql-fts:2022-latest")
            .WithExposedPort(TestDefaults.DbPort)
            .WithPassword(TestDefaults.DbPassword)
            .WithAutoRemove(true)
            .WithCleanUp(true);

        _container = containerBuilder.Build();

        using (var cancellationTokenSource = new CancellationTokenSource(TestDefaults.DbStartTimeout))
        {
            var cancellationToken = cancellationTokenSource.Token;
            await _container.StartAsync(cancellationToken);
        }

        var containerHostPort = _container.GetMappedPublicPort(TestDefaults.DbPort);

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
