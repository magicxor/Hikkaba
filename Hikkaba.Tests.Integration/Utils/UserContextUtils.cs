using Hikkaba.Shared.Models;
using Hikkaba.Shared.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Utils;

internal static class UserContextUtils
{
    /// <summary>
    ///     Sets up the user context with an admin user for testing.
    /// </summary>
    /// <param name="scope">The service scope.</param>
    /// <param name="adminId">The admin user ID.</param>
    /// <param name="userName">The admin username. Defaults to "admin".</param>
    public static void SetupUserContext(IServiceScope scope, int adminId, string userName = "admin")
    {
        var userContext = scope.ServiceProvider.GetRequiredService<IUserContext>();
        userContext.SetUser(new CurrentUser
        {
            Id = adminId,
            UserName = userName,
            Roles = ["Administrator"],
            ModeratedCategories = [],
        });
    }
}
