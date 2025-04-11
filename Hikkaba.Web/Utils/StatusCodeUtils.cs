using Hikkaba.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.Utils;

public static class StatusCodeUtils
{
    public static (string StatusCodeName, string StatusCodeDescription, EventId EventId) GetDetails(int statusCode)
    {
        return statusCode switch
        {
            400 => ("Bad Request", "The server could not understand the request", LogEventIds.BadRequest),
            401 => ("Unauthorized", "You need to be logged in to access this page", LogEventIds.Unauthorized),
            403 => ("Forbidden", "You don't have permission to access this page", LogEventIds.Forbidden),
            404 => ("Not Found", "The requested page was not found", LogEventIds.NotFound),
            405 => ("Method Not Allowed", "The request method is not supported for the requested resource", LogEventIds.MethodNotAllowed),
            408 => ("Request Timeout", "The server timed out waiting for the request", LogEventIds.RequestTimeout),
            409 => ("Conflict", "The request could not be completed due to a conflict with the current state of the resource", LogEventIds.Conflict),
            413 => ("Payload Too Large", "The request is larger than the server is willing to process", LogEventIds.PayloadTooLarge),
            414 => ("URI Too Long", "The request URI is longer than the server is willing to process", LogEventIds.UriTooLong),
            415 => ("Unsupported Media Type", "The request entity has a media type which the server or resource does not support", LogEventIds.UnsupportedMediaType),
            429 => ("Too Many Requests", "The user has sent too many requests in a given amount of time", LogEventIds.TooManyRequests),
            451 => ("Unavailable For Legal Reasons", "The server is denying access to the resource as a consequence of a legal demand", LogEventIds.UnavailableForLegalReasons),
            500 => ("Internal Server Error", "The server was unable to finish processing the request", LogEventIds.InternalServerError),
            _ => ("Unknown Error", "That’s odd... Something we didn’t expect happened", new EventId(LogEventIds.StartId + statusCode, "Unknown Error")),
        };
    }
}
