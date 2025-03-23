using System;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Data.Utils;
using Hikkaba.Tests.Integration.Constants;
using Hikkaba.Tests.Integration.Services;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Tests.Integration.Utils;

public static class TestDbUtils
{
    public static string GetExactConnectionString(string host, ushort port, string db, string password)
    {
        return $"Server=tcp:{host},{port};Encrypt=False;Database={db};MultipleActiveResultSets=true;User ID=SA;Password={password};Persist Security Info=False;TrustServerCertificate=True;MultiSubnetFailover=True";
    }

    private static string GetRandomizedConnectionString(string host, ushort port, string db, string password = TestDefaults.DbPassword)
    {
        return GetExactConnectionString(host, port, db + Guid.NewGuid().ToString("D"), password);
    }

    [MustDisposeResource]
    private static ApplicationDbContext CreateApplicationDbContext(string connectionString)
    {
        var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString, ContextConfiguration.SqlServerOptionsAction)
            .LogTo((eventId, logLevel) => logLevel >= LogLevel.Trace,
                eventData =>
                {
                    TestLogUtils.WriteProgressMessage(eventData.ToString());
                    TestLogUtils.WriteConsoleMessage(eventData.ToString());
                })
            .Options;
        return new ApplicationDbContext(dbContextOptions);
    }

    private static string CreateNewRandomConnectionString()
    {
        return GetRandomizedConnectionString(GlobalSetUp.DbHost, GlobalSetUp.DbPort, TestDefaults.DbName);
    }

    /// <summary>
    /// Creates an empty <see cref="ApplicationDbContext"/> database with a random name.
    /// </summary>
    public static async Task<RespawnableContextManager<ApplicationDbContext>> CreateNewRandomDbContextManagerAsync()
    {
        var connectionString = CreateNewRandomConnectionString();
        var respawnableContextMgr = new RespawnableContextManager<ApplicationDbContext>(connectionString, CreateApplicationDbContext);
        return respawnableContextMgr;
    }
}
