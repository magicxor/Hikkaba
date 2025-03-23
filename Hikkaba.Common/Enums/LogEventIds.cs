using Microsoft.Extensions.Logging;

namespace Hikkaba.Common.Enums;

public static class LogEventIds
{
    // Random number to avoid collision with other event ids
    public const int StartId = 354216000;

    public static readonly EventId Unknown = new(StartId + 0, "Unknown event");
    public static readonly EventId DbQuery = new(StartId + 1, "DB query executed");
}
