using System;
using System.Net;
using Hikkaba.Common.Constants;
using Hikkaba.Data.Entities;
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

        public Guid CurrentUserId => User.Identity.IsAuthenticated ? Guid.Parse(UserManager.GetUserId(User)) : default(Guid);
        public bool IsCurrentUserAdmin => User.Identity.IsAuthenticated && User.IsInRole(Defaults.AdministratorRoleName);

        public string UserAgent => Request.Headers.ContainsKey("User-Agent") ? Request.Headers["User-Agent"].ToString() : "";
        public IPAddress UserIpAddress => Request.HttpContext.Connection.RemoteIpAddress;
    }
}
