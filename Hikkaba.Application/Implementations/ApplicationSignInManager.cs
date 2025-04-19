using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hikkaba.Application.Implementations;

public sealed class ApplicationSignInManager : SignInManager<ApplicationUser>
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly TimeProvider _timeProvider;

    public ApplicationSignInManager(
        ApplicationDbContext applicationDbContext,
        TimeProvider timeProvider,
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILoggerFactory loggerFactory,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<ApplicationUser> confirmation)
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, loggerFactory.CreateLogger<SignInManager<ApplicationUser>>(), schemes, confirmation)
    {
        _applicationDbContext = applicationDbContext;
        _timeProvider = timeProvider;
    }

    public override async Task<SignInResult> PasswordSignInAsync(
        string userName,
        string password,
        bool isPersistent,
        bool lockoutOnFailure)
    {
        var user = await UserManager.FindByNameAsync(userName);
        if (user is null || user.IsDeleted)
        {
            return SignInResult.Failed;
        }

        var result = await PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        if (result.Succeeded)
        {
            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
            await _applicationDbContext.Users
                .Where(u => u.Id == user.Id)
                .ExecuteUpdateAsync(setProp =>
                    setProp.SetProperty(x => x.LastLoginAt, utcNow));
        }

        return result;
    }
}
