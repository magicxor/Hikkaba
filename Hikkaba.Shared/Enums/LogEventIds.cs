using Microsoft.Extensions.Logging;

namespace Hikkaba.Shared.Enums;

public static class LogEventIds
{
    // Random number to avoid collision with other event ids
    public const int StartId = 354210000;

    public static readonly EventId Unknown = new(StartId + 0, "Unknown event");
    public static readonly EventId BadRequest = new(StartId + 400, "Bad Request");
    public static readonly EventId Unauthorized = new(StartId + 401, "Unauthorized");
    public static readonly EventId Forbidden = new(StartId + 403, "Forbidden");
    public static readonly EventId NotFound = new(StartId + 404, "Not Found");
    public static readonly EventId TooManyRequests = new(StartId + 429, "Too Many Requests");
    public static readonly EventId UnavailableForLegalReasons = new(StartId + 451, "Unavailable For Legal Reasons");
    public static readonly EventId InternalServerError = new(StartId + 500, "Internal Server Error");
    public static readonly EventId DbQuery = new(StartId + 1000, "DB query");
    public static readonly EventId UserBanned = new(StartId + 1002, "User banned");
    public static readonly EventId MaintenanceMode = new(StartId + 1003, "Maintenance mode");
    public static readonly EventId ThreadCreated = new(StartId + 1100, "Thread created");
    public static readonly EventId ThreadCreateError = new(StartId + 1101, "Thread create error");
    public static readonly EventId ThreadEdited = new(StartId + 1102, "Thread edited");
    public static readonly EventId ThreadEditError = new(StartId + 1103, "Thread edit error");
    public static readonly EventId ThreadDeleted = new(StartId + 1104, "Thread deleted");
    public static readonly EventId ThreadDeleteError = new(StartId + 1105, "Thread delete error");
    public static readonly EventId PostCreated = new(StartId + 1200, "Post created");
    public static readonly EventId PostCreateError = new(StartId + 1201, "Post create error");
    public static readonly EventId PostEdited = new(StartId + 1202, "Post edited");
    public static readonly EventId PostEditError = new(StartId + 1203, "Post edit error");
    public static readonly EventId PostDeleted = new(StartId + 1204, "Post deleted");
    public static readonly EventId PostDeleteError = new(StartId + 1205, "Post delete error");
}
