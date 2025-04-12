using System.Diagnostics;
using Hikkaba.Web.Utils;
using Hikkaba.Web.ViewModels.ErrorViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc.Base;

public abstract class BaseMvcController : Controller
{
    protected string UserAgent => Request.Headers.UserAgent.ToString();

    protected byte[]? UserIpAddressBytes => Request.HttpContext.Connection.RemoteIpAddress?.GetAddressBytes();

    protected string? GetLocalReferrerOrNull()
    {
        var refererUrl = Request.GetTypedHeaders().Referer?.ToString();

        if (string.IsNullOrEmpty(refererUrl) || !Url.IsLocalUrl(refererUrl))
        {
            refererUrl = null;
        }

        return refererUrl;
    }

    protected string? GetLocalReferrerOrRoute(string fallbackRoute, object? fallbackRouteValues = null)
    {
        var referrerUrl = GetLocalReferrerOrNull();

        return string.IsNullOrEmpty(referrerUrl)
            ? Url.RouteUrl(fallbackRoute, fallbackRouteValues)
            : referrerUrl;
    }

    protected IActionResult CustomErrorPage(
        int statusCode,
        string errorMessage,
        string? returnUrl)
    {
        var (statusCodeName, statusCodeDescription, eventId) = StatusCodeUtils.GetDetails(statusCode);
        var vm = new CustomErrorViewModel
        {
            EventId = eventId,
            ErrorMessage = errorMessage,
            ReturnUrl = returnUrl,
            StatusCode = statusCode,
            StatusCodeName = statusCodeName,
            StatusCodeDescription = statusCodeDescription,
            TraceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
        };
        Response.StatusCode = vm.StatusCode;
        return View("CustomError", vm);
    }
}
