using Hikkaba.Web.Utils;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.Filters
{
    public class ExceptionLoggingFilter : ExceptionFilterAttribute
    {
        private readonly ILogger _logger;

        public ExceptionLoggingFilter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ExceptionLoggingFilter>();
        }

        public override void OnException(ExceptionContext context)
        {
            var ex = context.Exception;
            var actionName = context.ActionDescriptor.DisplayName;
            var isModelValid = context.ModelState.IsValid;
            var modelErrors = context.ModelState.ModelErrorsToString();
            var displayUrl = context.HttpContext.Request.GetDisplayUrl();

            _logger?.LogError(ex, $"{nameof(isModelValid)}={isModelValid} | {nameof(modelErrors)}={modelErrors} | {nameof(context.HttpContext.Response.StatusCode)}={context.HttpContext.Response.StatusCode}");

            context.ExceptionHandled = false;
            base.OnException(context);
        }
    }
}