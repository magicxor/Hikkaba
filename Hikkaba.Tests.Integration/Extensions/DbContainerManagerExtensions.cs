using System.Threading.Tasks;
using Hikkaba.Tests.Integration.Services;

namespace Hikkaba.Tests.Integration.Extensions;

internal static class DbContainerManagerExtensions
{
    public static async Task StopIfNotNullAsync(this DbContainerManager? containerManager)
    {
        if (containerManager != null)
        {
            await containerManager.StopAsync();
        }
    }
}
