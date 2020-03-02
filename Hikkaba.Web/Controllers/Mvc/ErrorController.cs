using System.Net;
using Hikkaba.Common.Constants;
using Hikkaba.Common.Extensions;
using Hikkaba.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.Controllers.Mvc
{
    public class ErrorController : Controller
    {
        private const string DefaultErrorMessage = "Something went wrong";
        private const string PageNotFoundMessage = "Page not found";

        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        public IActionResult PageNotFound()
        {
            _logger.LogDebug(EventIdentifiers.HttpNotFound.ToEventId(), "Page not found");

            return Details(PageNotFoundMessage, HttpStatusCode.NotFound);
        }

        public IActionResult Exception()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = feature?.Error;
            var statusCode = HttpContext.Response.StatusCode;
            var eventId = statusCode == HttpStatusCode.OK.ToInt() ? EventIdentifiers.HttpInternalError.ToEventId() : new EventId(statusCode);

            if (exception != null)
            {
                _logger.LogError(eventId, exception, "Unhandled exception");
            }
            else
            {
                _logger.LogError(eventId, "Unhandled exception");
            }

            var message = exception?.Message;
            return Details(message, (HttpStatusCode)statusCode);
        }

        public IActionResult Details(HttpStatusCode statusCode)
        {
            _logger.LogDebug(statusCode.ToEventId(), $"Return status page for {nameof(statusCode)}={statusCode}");

            var message = DefaultErrorMessage;
            return Details(message, statusCode);
        }

        private IActionResult Details(string message = null, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            if (string.IsNullOrEmpty(message))
            {
                message = DefaultErrorMessage;
            }
            var exception = statusCode == HttpStatusCode.OK
                ? new HttpResponseException(HttpStatusCode.InternalServerError, message)
                : new HttpResponseException(statusCode, message);
            return View("Details", exception);
        }
    }
}