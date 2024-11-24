using TPrimaryKey = System.Guid;
using System.Threading.Tasks;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Services;
using Hikkaba.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Hikkaba.Web.Middleware;

public class SetAuthenticatedUserMiddleware
{
    private readonly RequestDelegate _next;

    public SetAuthenticatedUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }
        
    public async Task Invoke(HttpContext httpContext, IAuthenticatedUserService authenticatedUserService, UserManager<ApplicationUser> userManager)
    {
        if (httpContext.User.Identity.IsAuthenticated)
        {
            authenticatedUserService.ApplicationUserClaims = new ApplicationUserClaimsDto
            {
                Id = TPrimaryKey.Parse(userManager.GetUserId(httpContext.User)),
                UserName = userManager.GetUserName(httpContext.User),
            };
        }
        await _next(httpContext);
    }
}