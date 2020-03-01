using System.Net;
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
            return Details(PageNotFoundMessage, HttpStatusCode.NotFound);
        }

        public IActionResult Exception()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var statusCode = HttpContext.Response.StatusCode;
            var message = feature?.Error?.Message;
            return Details(message, (HttpStatusCode)statusCode);
        }

        public IActionResult Details(HttpStatusCode statusCode)
        {
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