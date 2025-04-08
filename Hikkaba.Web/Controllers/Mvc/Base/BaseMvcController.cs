using Hikkaba.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc.Base;

public abstract class BaseMvcController : Controller
{
    private UserManager<ApplicationUser> UserManager { get; set; }

    protected BaseMvcController(UserManager<ApplicationUser> userManager)
    {
        UserManager = userManager;
    }

    protected string UserAgent => Request.Headers.UserAgent.ToString();

    protected byte[]? UserIpAddressBytes => Request.HttpContext.Connection.RemoteIpAddress?.GetAddressBytes();
}
