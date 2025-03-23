using System.Linq.Expressions;
using Hikkaba.Common.Exceptions;
using Hikkaba.Common.Services.Contracts;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.ApplicationUser;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Repositories.Implementations;

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

    public async Task<IReadOnlyList<ApplicationUserDto>> ListUsersAsync(bool includeDeleted)
    {
        Expression<Func<ApplicationUser, bool>> filterDeleted = includeDeleted
            ? x => true
            : x => !x.IsDeleted;

        return await _userMgr.Users
            .Where(filterDeleted)
            .Select(x => new ApplicationUserDto
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

    public async Task<int> CreateAsync(ApplicationUserCreateRm applicationUserCreateRm)
    {
        var user = new ApplicationUser
        {
            UserName = applicationUserCreateRm.UserName,
            Email = applicationUserCreateRm.Email,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
        };
        var result = await _userMgr.CreateAsync(user, applicationUserCreateRm.Password);
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
            .Where(user => user.Id == userId)
            .ExecuteUpdateAsync(setProp =>
                setProp.SetProperty(user => user.IsDeleted, isDeleted));
    }
}
