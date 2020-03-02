using TPrimaryKey = System.Guid;
using System.Net;
using Hikkaba.Common.Constants;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc.Base
{
    public class BaseMvcController: Controller
    {
        private UserManager<ApplicationUser> UserManager { get; set; }

        public BaseMvcController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public TPrimaryKey GetCurrentUserId()
        {
            if (User.Identity.IsAuthenticated)
            {
                return TPrimaryKey.Parse(UserManager.GetUserId(User));
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized, "User is not authenticated");
            }
        }
        public bool IsCurrentUserAdmin => User.Identity.IsAuthenticated && User.IsInRole(Defaults.AdministratorRoleName);

        public string UserAgent => Request.Headers.ContainsKey("User-Agent") ? Request.Headers["User-Agent"].ToString() : "";
        public IPAddress UserIpAddress => Request.HttpContext.Connection.RemoteIpAddress;
    }
}
