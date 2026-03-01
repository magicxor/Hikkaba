using Hikkaba.Tests.Integration.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ObjectPool;

namespace Hikkaba.Tests.Integration.Services;

public sealed class ContextManagerPooledObjectPolicy<TContext> : IPooledObjectPolicy<RespawnableContextManager<TContext>>
    where TContext : DbContext
{
    private readonly Func<RespawnableContextManager<TContext>> _contextManagerFactory;

    public ContextManagerPooledObjectPolicy(
        Func<RespawnableContextManager<TContext>> contextManagerFactory)
    {
        _contextManagerFactory = contextManagerFactory;
    }

    public RespawnableContextManager<TContext> Create()
    {
        return _contextManagerFactory();
    }

    public bool Return(RespawnableContextManager<TContext> obj)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(obj);

            if (obj.IsDisposed)
            {
                TestLogUtils.WriteProgressMessage($"Object {typeof(RespawnableContextManager<TContext>).Name} is disposed, not returning to pool");
                return false;
            }

            /*
             As obj.CreateRespawnedDbConnectionStringAsync is always called
             in the beginning of every test in IntegrationTestBase.CreateAppScopeAsync,
             we can skip calling it here. This will reduce the time needed to return the object to the pool.

             // AsyncContext.Run(obj.CreateRespawnedDbConnectionStringAsync);
             */

            return true;
        }
        catch (Exception ex)
        {
            TestLogUtils.WriteProgressMessage($"Error respawning database in {typeof(RespawnableContextManager<TContext>).Name}: {ex}");
            return false;
        }
    }
}
