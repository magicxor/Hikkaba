using Hikkaba.Common.Constants;
using Hikkaba.Infrastructure.Models.Configuration;

namespace Hikkaba.Infrastructure.Models.Extensions;

public static class HikkabaConfigurationExtensions
{
    public static int GetCacheMaxAgeSecondsOrDefault(this HikkabaConfiguration settings)
    {
        return settings.CacheMaxAgeSeconds <= 0
            ? Convert.ToInt32(Defaults.CacheMaxAgeSeconds)
            : settings.CacheMaxAgeSeconds;
    }
}
