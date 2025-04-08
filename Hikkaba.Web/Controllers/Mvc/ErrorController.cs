using System.Net;
using Hikkaba.Shared.Enums;
using Hikkaba.Shared.Exceptions;
using Hikkaba.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.Controllers.Mvc;

[AllowAnonymous]
[Route("error")]
public sealed class ErrorController : Controller
{
    private const string DefaultErrorMessage = "Something went wrong";
    private const string PageNotFoundMessage = "Page not found";

    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult PageNotFound()
    {
        return Details(PageNotFoundMessage, HttpStatusCode.NotFound);
    }

    [HttpGet]
    public IActionResult Exception()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = feature?.Error;
        var statusCode = HttpContext.Response.StatusCode;
        var eventId = statusCode == HttpStatusCode.OK.ToInt() ? LogEventIds.InternalError : new EventId(statusCode);

        if (exception != null)
        {
            _logger.LogError(eventId, exception, "Unhandled exception");
        }
        else
        {
            _logger.LogError(eventId, "Unhandled exception");
        }

        var message = exception?.Message ?? string.Empty;
        return Details(message, (HttpStatusCode)statusCode);
    }

    [HttpGet]
    public IActionResult Details(HttpStatusCode statusCode)
    {
        _logger.LogDebug(statusCode.ToEventId(), "Return status page for {StatusCodeName}={StatusCode}", nameof(statusCode), statusCode);
        return Details(DefaultErrorMessage, statusCode);
    }

    private IActionResult Details(string? message = null, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        if (string.IsNullOrEmpty(message))
        {
            message = DefaultErrorMessage;
        }
        var exception = statusCode == HttpStatusCode.OK
            ? new HikkabaHttpResponseException(HttpStatusCode.InternalServerError, message)
            : new HikkabaHttpResponseException(statusCode, message);
        return View("Details", exception);
    }
}
