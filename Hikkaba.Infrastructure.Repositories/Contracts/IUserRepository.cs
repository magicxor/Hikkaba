using Hikkaba.Infrastructure.Models.ApplicationUser;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface IUserRepository
{
    Task<IReadOnlyList<UserDetailsModel>> ListUsersAsync(UserFilter filter, CancellationToken cancellationToken);
    Task<UserCreateResultModel> CreateUserAsync(UserCreateRequestModel requestModel, CancellationToken cancellationToken);
    Task SetUserDeletedAsync(int userId, bool isDeleted, CancellationToken cancellationToken);
}
