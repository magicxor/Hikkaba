using TPrimaryKey = System.Guid;
using System.Net;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Exceptions;
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

    public TPrimaryKey GetCurrentUserId()
    {
        if (User.Identity?.IsAuthenticated == true
            && UserManager.GetUserId(User) is { } uid)
        {
            return TPrimaryKey.Parse(uid);
        }
        else
        {
            throw new HttpResponseException(HttpStatusCode.Unauthorized, "User is not authenticated");
        }
    }

    protected string UserAgent => Request.Headers.TryGetValue("User-Agent", out Microsoft.Extensions.Primitives.StringValues value) ? value.ToString() : "";

    protected IPAddress UserIpAddress => Request.HttpContext.Connection.RemoteIpAddress;
}
