using System.Net;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.ApplicationUser;
using Hikkaba.Infrastructure.Models.Error;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
        return await _userMgr.Users
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
