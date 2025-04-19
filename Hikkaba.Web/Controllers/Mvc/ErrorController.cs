using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Hikkaba.Web.Utils;
using Hikkaba.Web.ViewModels.ErrorViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.Controllers.Mvc;

[AllowAnonymous]
[Route("error")]
public sealed class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    [Route("", Name = "ErrorStatusCode")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    [SuppressMessage("Security", "CA5395:Miss HttpVerb attribute for action methods", Justification = "This is acceptable for the error controller.")]
    public IActionResult Index([Required] [Range(1, 999)] int statusCode)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var (statusCodeName, statusCodeDescription, _) = StatusCodeUtils.GetDetails(statusCode);

        var vm = new StatusCodeViewModel
        {
            StatusCode = statusCode,
            StatusCodeName = statusCodeName,
            StatusCodeDescription = statusCodeDescription,
            TraceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
        };

        return View(vm);
    }

    [Route("details", Name = "ErrorException")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    [SuppressMessage("Security", "CA5395:Miss HttpVerb attribute for action methods", Justification = "This is acceptable for the error controller.")]
    public IActionResult Exception()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = feature?.Error;
        var statusCode = HttpContext.Response.StatusCode;
        var (statusCodeName, statusCodeDescription, eventId) = StatusCodeUtils.GetDetails(statusCode);

        if (exception != null)
        {
            _logger.LogError(eventId, exception, "Unhandled exception");
        }
        else
        {
            _logger.LogInformation(eventId, "Unhandled exception endpoint hit, but no exception was found");
        }

        var exceptionName = exception?.GetType().Name ?? "UnknownException";

        var vm = new ExceptionViewModel
        {
            ExceptionName = exceptionName,
            StatusCode = statusCode,
            StatusCodeName = statusCodeName,
            StatusCodeDescription = statusCodeDescription,
            TraceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
        };

        return View(vm);
    }
}
