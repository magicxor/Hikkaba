using System;
using Hikkaba.Models.Configuration;

namespace Hikkaba.Models.Extensions;

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