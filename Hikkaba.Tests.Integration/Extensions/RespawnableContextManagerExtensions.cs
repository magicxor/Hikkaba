using System.Threading.Tasks;
using Hikkaba.Tests.Integration.Services;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Tests.Integration.Extensions;

internal static class RespawnableContextManagerExtensions
{
    public static async Task StopIfNotNullAsync<T>(this RespawnableContextManager<T>? respawnableContextManager)
        where T : DbContext
    {
        if (respawnableContextManager != null)
        {
            await respawnableContextManager.DeinitializeAsync();
            respawnableContextManager.Dispose();
        }
    }
}
