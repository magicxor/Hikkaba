using Hikkaba.Infrastructure.Models.Configuration;

namespace Hikkaba.Infrastructure.Models.Extensions;

public static class HikkabaConfigurationExtensions
{
    public static int GetCacheMaxAgeSecondsOrDefault(this HikkabaConfiguration settings)
    {
        if (settings == null || settings.CacheMaxAgeSeconds <= 0)
        {
            return Convert.ToInt32(TimeSpan.FromDays(365).TotalSeconds);
        }
        else
        {
            return settings.CacheMaxAgeSeconds;
        }
    }
}
