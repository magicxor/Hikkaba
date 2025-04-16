using System.Globalization;
using System.Net;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Error;
using Hikkaba.Infrastructure.Models.Role;
using Hikkaba.Infrastructure.Models.User;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Extensions;
using Hikkaba.Shared.Constants;
using Hikkaba.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf.Types;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public sealed class UserRepository : IUserRepository
{
    private readonly ILogger<UserRepository> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly UserManager<ApplicationUser> _userMgr;

    public UserRepository(
        ILogger<UserRepository> logger,
        TimeProvider timeProvider,
        ApplicationDbContext applicationDbContext,
        UserManager<ApplicationUser> userMgr)
    {
        _logger = logger;
        _timeProvider = timeProvider;
        _applicationDbContext = applicationDbContext;
        _userMgr = userMgr;
    }

    public async Task<IReadOnlyList<UserDetailsModel>> ListUsersAsync(UserFilter filter, CancellationToken cancellationToken)
    {
        return await _applicationDbContext.Users
            .TagWithCallSite()
            .Where(x => filter.IncludeDeleted || !x.IsDeleted)
            .GroupJoin(
                _applicationDbContext.UserRoles
                    .Join(
                        _applicationDbContext.Roles,
                        role => role.RoleId,
                        role => role.Id,
                        (userRole, role) => new { UserRole = userRole, Role = role }),
                user => user.Id,
                userRole => userRole.UserRole.UserId,
                (user, userRoles) => new { User = user, Roles = userRoles.Select(ur => ur.Role) })
            .Select(gj => new UserDetailsModel
            {
                Id = gj.User.Id,
                IsDeleted = gj.User.IsDeleted,
                AccessFailedCount = gj.User.AccessFailedCount,
                EmailConfirmed = gj.User.EmailConfirmed,
                LastLogin = gj.User.LastLoginAt,
                LockoutEnabled = gj.User.LockoutEnabled,
                LockoutEnd = gj.User.LockoutEnd,
                Email = gj.User.Email,
                UserName = gj.User.UserName,
                PhoneNumber = gj.User.PhoneNumber,
                PhoneNumberConfirmed = gj.User.PhoneNumberConfirmed,
                TwoFactorEnabled = gj.User.TwoFactorEnabled,
                UserRoles = gj.Roles
                    .Select(role => new RoleModel
                    {
                        Id = role.Id,
                        Name = role.Name,
                        NormalizedName = role.NormalizedName,
                    })
                    .ToList(),
            })
            .ApplyOrderBy(filter, x => x.UserName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CategoryModeratorModel>> ListCategoryModerators(CategoryModeratorFilter filter, CancellationToken cancellationToken)
    {
        return await _applicationDbContext.UserRoles
            .TagWithCallSite()
            .Join(
                _applicationDbContext.Roles,
                role => role.RoleId,
                role => role.Id,
                (userRole, role) => new { UserRole = userRole, Role = role })
            .Join(
                _applicationDbContext.Users,
                joinResult => joinResult.UserRole.UserId,
                user => user.Id,
                (joinResult, user) => new { joinResult.UserRole, joinResult.Role, User = user })
            .Where(j => j.Role.Name == Defaults.ModeratorRoleName || j.Role.Name == Defaults.AdministratorRoleName)
            .Select(j => j.User)
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
            .GroupJoin(
                _applicationDbContext.UserRoles
                    .Join(
                        _applicationDbContext.Roles,
                        role => role.RoleId,
                        role => role.Id,
                        (userRole, role) => new { UserRole = userRole, Role = role }),
                user => user.Id,
                userRole => userRole.UserRole.UserId,
                (user, userRoles) => new { User = user, Roles = userRoles.Select(ur => ur.Role) })
            .Select(gj => new UserDetailsModel
            {
                Id = gj.User.Id,
                IsDeleted = gj.User.IsDeleted,
                AccessFailedCount = gj.User.AccessFailedCount,
                EmailConfirmed = gj.User.EmailConfirmed,
                LastLogin = gj.User.LastLoginAt,
                LockoutEnabled = gj.User.LockoutEnabled,
                LockoutEnd = gj.User.LockoutEnd,
                Email = gj.User.Email,
                UserName = gj.User.UserName,
                PhoneNumber = gj.User.PhoneNumber,
                PhoneNumberConfirmed = gj.User.PhoneNumberConfirmed,
                TwoFactorEnabled = gj.User.TwoFactorEnabled,
                UserRoles = gj.Roles
                    .Select(role => new RoleModel
                    {
                        Id = role.Id,
                        Name = role.Name,
                        NormalizedName = role.NormalizedName,
                    })
                    .ToList(),
            })
            .OrderBy(user => user.Id)
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

        if (result.Succeeded)
        {
            _logger.LogInformation(
                LogEventIds.UserCreated,
                "User with ID {UserId} created",
                user.Id);
        }
        else
        {
            _logger.LogWarning(
                LogEventIds.UserCreateError,
                "User creation failed: {Errors}",
                string.Join(", ", result.Errors.Select(e => $"{e.Code}:{e.Description}")));
        }

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
            _logger.LogWarning(
                LogEventIds.UserEditError,
                "User with ID {UserId} not found",
                requestModel.Id);
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

        var currentUserRoles = await _applicationDbContext.UserRoles
            .TagWithCallSite()
            .Where(ur => ur.UserId == user.Id)
            .ToListAsync(cancellationToken);

        _applicationDbContext.RemoveRange(currentUserRoles);

        var newUserRoles = await _applicationDbContext.Roles
            .TagWithCallSite()
            .Where(role => requestModel.UserRoleIds.Contains(role.Id))
            .Select(role => new IdentityUserRole<int>
            {
                UserId = requestModel.Id,
                RoleId = role.Id,
            })
            .ToListAsync(cancellationToken);

        _applicationDbContext.AddRange(newUserRoles);

        await _applicationDbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            LogEventIds.UserEdited,
            "User with ID {UserId} edited",
            requestModel.Id);

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
        _logger.LogInformation(
            LogEventIds.UserDeleted,
            "User with ID {UserId} marked as deleted",
            userId);
    }
}
