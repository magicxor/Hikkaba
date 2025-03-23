using System.Globalization;
using System.Net;
using Hikkaba.Common.Exceptions;
using Hikkaba.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc.Base;

public class BaseMvcController: Controller
{
    private UserManager<ApplicationUser> UserManager { get; set; }

    public BaseMvcController(UserManager<ApplicationUser> userManager)
    {
        UserManager = userManager;
    }

    public int GetCurrentUserId()
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

    protected string? UserAgent => Request.Headers.UserAgent;

    protected IPAddress? UserIpAddress => Request.HttpContext.Connection.RemoteIpAddress;
}
