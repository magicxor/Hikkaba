using System.Globalization;
using System.Net;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Error;
using Hikkaba.Infrastructure.Models.User;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OneOf.Types;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public sealed class UserRepository : IUserRepository
{
    private readonly TimeProvider _timeProvider;
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly UserManager<ApplicationUser> _userMgr;

    public UserRepository(
        TimeProvider timeProvider,
        ApplicationDbContext applicationDbContext,
        UserManager<ApplicationUser> userMgr)
    {
        _timeProvider = timeProvider;
        _applicationDbContext = applicationDbContext;
        _userMgr = userMgr;
    }

    public async Task<IReadOnlyList<UserDetailsModel>> ListUsersAsync(UserFilter filter, CancellationToken cancellationToken)
    {
        var query = _applicationDbContext.Users
            .TagWithCallSite()
            .AsQueryable();

        return await query
            .Where(x => filter.IncludeDeleted || !x.IsDeleted)
            .Select(x => new UserDetailsModel
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
                TwoFactorEnabled = x.TwoFactorEnabled,
            })
            .ApplyOrderBy(filter, x => x.UserName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CategoryModeratorModel>> ListCategoryModerators(CategoryModeratorFilter filter, CancellationToken cancellationToken)
    {
        return await _applicationDbContext.Users
            .TagWithCallSite()
            .Where(user => filter.IncludeDeleted || !user.IsDeleted)
            .Select(user => new CategoryModeratorModel
            {
                Id = user.Id,
                LastLogin = user.LastLoginAt,
                Email = user.Email,
                UserName = user.UserName,
                IsCategoryModerator = user.ModerationCategories
                    .Any(mc => mc.Category.Alias == filter.CategoryAlias),
            })
            .ApplyOrderBy(filter, x => x.UserName)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDetailsModel?> GetUserAsync(int userId, CancellationToken cancellationToken)
    {
        return await _applicationDbContext.Users
            .TagWithCallSite()
            .Where(x => x.Id == userId)
            .Select(x => new UserDetailsModel
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
                TwoFactorEnabled = x.TwoFactorEnabled,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserCreateResultModel> CreateUserAsync(UserCreateRequestModel requestModel, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = requestModel.UserName,
            Email = requestModel.Email,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
        };
        var result = await _userMgr.CreateAsync(user, requestModel.Password);

        return result.Succeeded
            ? new UserCreateResultSuccessModel { UserId = user.Id }
            : new DomainError
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                ErrorMessage = $"User creation failed: {result}",
            };
    }

    public async Task<UserEditResultModel> EditUserAsync(UserEditRequestModel requestModel, CancellationToken cancellationToken)
    {
        var user = await _userMgr.FindByIdAsync(requestModel.Id.ToString(CultureInfo.InvariantCulture));

        if (user is null)
        {
            return new DomainError
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                ErrorMessage = $"User with ID {requestModel.Id} not found.",
            };
        }

        await _userMgr.SetUserNameAsync(user, requestModel.UserName);
        await _userMgr.SetEmailAsync(user, requestModel.Email);
        await _userMgr.SetLockoutEndDateAsync(user, requestModel.LockoutEndDate);
        await _userMgr.SetTwoFactorEnabledAsync(user, requestModel.TwoFactorEnabled);

        return default(Success);
    }

    public async Task SetUserDeletedAsync(int userId, bool isDeleted, CancellationToken cancellationToken)
    {
        await _applicationDbContext.Users
            .TagWithCallSite()
            .Where(user => user.Id == userId)
            .ExecuteUpdateAsync(setProp =>
                setProp.SetProperty(user => user.IsDeleted, isDeleted),
                cancellationToken);
    }
}
