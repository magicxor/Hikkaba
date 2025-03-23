using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Common.Models;
using Hikkaba.Common.Services.Contracts;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Web.Middleware;

public class SetAuthenticatedUserMiddleware
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
                    .ToHashSetAsync();

                authenticatedUserService.SetUser(new CurrentUser
                {
                    Id = userId,
                    UserName = userName,
                    ModeratedCategories = moderatedCategories,
                });
            }
        }

        await _next(httpContext);
    }
}
