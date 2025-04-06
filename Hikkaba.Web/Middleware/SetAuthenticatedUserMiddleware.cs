using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Shared.Models;
using Hikkaba.Shared.Services.Contracts;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Web.Middleware;

internal class SetAuthenticatedUserMiddleware
{
    private readonly RequestDelegate _next;

    public SetAuthenticatedUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(
        HttpContext httpContext,
        IUserContext authenticatedUserService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext applicationDbContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var userIdStr = userManager.GetUserId(httpContext.User);
            var userName = userManager.GetUserName(httpContext.User);

            if (userIdStr is not null
                && userName is not null
                && int.TryParse(userIdStr, CultureInfo.InvariantCulture, out var userId))
            {
                var moderatedCategories = await applicationDbContext.CategoriesToModerators
                    .Where(mc => mc.ModeratorId == userId)
                    .Select(mc => mc.CategoryId)
                    .ToHashSetAsync(httpContext.RequestAborted);

                var roles = await applicationDbContext
                    .UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Join(applicationDbContext.Roles,
                        userToRole => userToRole.RoleId,
                        role => role.Id,
                        (userToRole, role) => role.Name)
                    .Where(roleName => roleName != null)
                    .Select(roleName => roleName!)
                    .ToHashSetAsync(httpContext.RequestAborted);

                authenticatedUserService.SetUser(new CurrentUser
                {
                    Id = userId,
                    UserName = userName,
                    Roles = roles,
                    ModeratedCategories = moderatedCategories,
                });
            }
        }

        await _next(httpContext);
    }
}
