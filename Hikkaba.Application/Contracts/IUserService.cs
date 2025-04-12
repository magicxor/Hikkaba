using Hikkaba.Infrastructure.Models.ApplicationUser;

namespace Hikkaba.Application.Contracts;

public interface IUserService
{
    Task<IReadOnlyList<UserDetailsModel>> ListUsersAsync(UserFilter filter, CancellationToken cancellationToken);
    Task<IReadOnlyList<CategoryModeratorModel>> ListCategoryModerators(CategoryModeratorFilter filter, CancellationToken cancellationToken);
    Task<UserCreateResultModel> CreateUserAsync(UserCreateRequestModel requestModel, CancellationToken cancellationToken);
    Task SetUserDeletedAsync(int userId, bool isDeleted, CancellationToken cancellationToken);
}
