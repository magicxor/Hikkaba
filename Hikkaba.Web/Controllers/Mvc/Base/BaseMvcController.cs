using System.Globalization;
using System.Net;
using Hikkaba.Shared.Exceptions;
using Hikkaba.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc.Base;

internal abstract class BaseMvcController : Controller
{
    private UserManager<ApplicationUser> UserManager { get; set; }

    protected BaseMvcController(UserManager<ApplicationUser> userManager)
    {
        UserManager = userManager;
    }

    protected int GetCurrentUserId()
    {
        if (User.Identity?.IsAuthenticated == true
            && UserManager.GetUserId(User) is { } uid)
        {
            return int.Parse(uid, CultureInfo.InvariantCulture);
        }
        else
        {
            throw new HikkabaHttpResponseException(HttpStatusCode.Unauthorized, "User is not authenticated");
        }
    }

    protected string UserAgent => Request.Headers.UserAgent.ToString();

    protected string? UserIpAddressStr => Request.HttpContext.Connection.RemoteIpAddress?.ToString();

    protected byte[]? UserIpAddressBytes => Request.HttpContext.Connection.RemoteIpAddress?.GetAddressBytes();

    protected IPAddress? UserIpAddress => Request.HttpContext.Connection.RemoteIpAddress;
}
