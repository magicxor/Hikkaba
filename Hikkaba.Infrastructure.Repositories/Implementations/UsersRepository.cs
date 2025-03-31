using System.Linq.Expressions;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.ApplicationUser;
using Hikkaba.Shared.Exceptions;
using Hikkaba.Shared.Services.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public class UsersRepository
{
    private readonly TimeProvider _timeProvider;
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly UserManager<ApplicationUser> _userMgr;
    private readonly IUserContext _userContext;

    public UsersRepository(
        TimeProvider timeProvider,
        ApplicationDbContext applicationDbContext,
        UserManager<ApplicationUser> userMgr,
        IUserContext userContext)
    {
        _timeProvider = timeProvider;
        _applicationDbContext = applicationDbContext;
        _userMgr = userMgr;
        _userContext = userContext;
    }

    public async Task<IReadOnlyList<ApplicationUserDetailsModel>> ListUsersAsync(bool includeDeleted)
    {
        Expression<Func<ApplicationUser, bool>> filterDeleted = includeDeleted
            ? x => true
            : x => !x.IsDeleted;

        return await _userMgr.Users
            .Where(filterDeleted)
            .Select(x => new ApplicationUserDetailsModel
            {
                Id = x.Id,
                IsDeleted = x.IsDeleted,
                AccessFailedCount = x.AccessFailedCount,
                EmailConfirmed = x.EmailConfirmed,
                LastLogin = x.LastLoginAt,
                LockoutEnabled = x.LockoutEnabled,
                LockoutEnd = x.LockoutEnd,
                Email = x.Email,
                UserName = x.UserName,
                PhoneNumber = x.PhoneNumber,
                PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                TwoFactorEnabled = x.TwoFactorEnabled
            })
            .ToListAsync();
    }

    public async Task<int> CreateAsync(ApplicationUserCreateRequestModel applicationUserCreateRequestModel)
    {
        var user = new ApplicationUser
        {
            UserName = applicationUserCreateRequestModel.UserName,
            Email = applicationUserCreateRequestModel.Email,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
        };
        var result = await _userMgr.CreateAsync(user, applicationUserCreateRequestModel.Password);
        if (result.Succeeded)
        {
            return user.Id;
        }
        else
        {
            throw new HikkabaDomainException($"User creation failed: {string.Join(',', result.Errors.Select(x => x.Description))}");
        }
    }

    public async Task SetUserDeletedAsync(int userId, bool isDeleted)
    {
        await _applicationDbContext.Users
            .TagWithCallSite()
            .Where(user => user.Id == userId)
            .ExecuteUpdateAsync(setProp =>
                setProp.SetProperty(user => user.IsDeleted, isDeleted));
    }
}
