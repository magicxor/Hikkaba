using System;
using Microsoft.Extensions.Time.Testing;

namespace Hikkaba.Tests.Integration;

internal static class FakeTimeProviderFactory
{
    private const string LocalTimeZoneId = "Etc/GMT-11";

    public static FakeTimeProvider Create()
    {
        return Create(new DateTime(1999, 12, 31, 23, 59, 39, DateTimeKind.Utc));
    }

    public static FakeTimeProvider Create(DateTime currentUtcDateTime, string localTimeZoneId = LocalTimeZoneId)
    {
        if (currentUtcDateTime.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The provided DateTime must be in UTC.", nameof(currentUtcDateTime));
        }

        var timeProvider = new FakeTimeProvider(new DateTimeOffset(currentUtcDateTime, TimeSpan.Zero));
        timeProvider.SetLocalTimeZone(TimeZoneInfo.FindSystemTimeZoneById(localTimeZoneId));
        return timeProvider;
    }
}
