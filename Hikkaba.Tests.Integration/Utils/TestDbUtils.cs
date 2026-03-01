using Hikkaba.Data.Context;
using Hikkaba.Tests.Integration.Constants;
using Hikkaba.Tests.Integration.Services;
using Hikkaba.Web.Services;
using JetBrains.Annotations;
using Microsoft.Extensions.ObjectPool;

namespace Hikkaba.Tests.Integration.Utils;

public static class TestDbUtils
{
    private static readonly ObjectPoolProvider ObjectPoolProvider = new DefaultObjectPoolProvider();
    private static readonly ObjectPool<RespawnableContextManager<ApplicationDbContext>> ApplicationDbContextManagerPool = ObjectPoolProvider.Create(new ContextManagerPooledObjectPolicy<ApplicationDbContext>(CreateNewRandomMetaManager));

    private static string GetExactConnectionString(string host, ushort port, string db, string password)
    {
        return $"Server=tcp:{host},{port};Encrypt=False;Database={db};MultipleActiveResultSets=true;User ID=SA;Password={password};Persist Security Info=False;TrustServerCertificate=True;MultiSubnetFailover=True";
    }

    private static string GetRandomizedConnectionString(string host, ushort port, string db, string password = TestInfraDefaults.DbPassword)
    {
        return GetExactConnectionString(host, port, db + Guid.NewGuid().ToString("D"), password);
    }

    [MustDisposeResource]
    private static ApplicationDbContext CreateApplicationDbContext(string connectionString)
    {
        var applicationDbContextFactory = new MetaDesignTimeDbContextFactory(connectionString);
        return applicationDbContextFactory.CreateDbContext([]);
    }

    private static string CreateNewRandomConnectionString(string dbName)
    {
        return GetRandomizedConnectionString(GlobalSetUp.DbHost, GlobalSetUp.DbPort, dbName);
    }

    public static RespawnableContextManager<ApplicationDbContext> LeaseMetaManagerFromPool()
    {
        return ApplicationDbContextManagerPool.Get();
    }

    public static void ReturnMetaManagerToPool(RespawnableContextManager<ApplicationDbContext> applicationDbContextManager)
    {
        ApplicationDbContextManagerPool.Return(applicationDbContextManager);
    }

    public static void DisposePools()
    {
        if (ApplicationDbContextManagerPool is IDisposable metaCtxMgrPool)
            metaCtxMgrPool.Dispose();
    }

    /// <summary>
    /// Creates an empty <see cref="ApplicationDbContext"/> database with a random name.
    /// </summary>
    public static RespawnableContextManager<ApplicationDbContext> CreateNewRandomMetaManager()
    {
        var connectionString = CreateNewRandomConnectionString(TestInfraDefaults.DbName);
        var respawnableContextMgr = new RespawnableContextManager<ApplicationDbContext>(connectionString, CreateApplicationDbContext);
        return respawnableContextMgr;
    }
}
