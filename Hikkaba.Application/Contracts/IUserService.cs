using Hikkaba.Infrastructure.Models.User;

namespace Hikkaba.Application.Contracts;

public interface IUserService
{
    Task<IReadOnlyList<UserDetailsModel>> ListUsersAsync(UserFilter filter, CancellationToken cancellationToken);
    Task<IReadOnlyList<CategoryModeratorModel>> ListCategoryModerators(CategoryModeratorFilter filter, CancellationToken cancellationToken);
    Task<UserDetailsModel?> GetUserAsync(int userId, CancellationToken cancellationToken);
    Task<UserCreateResultModel> CreateUserAsync(UserCreateRequestModel requestModel, CancellationToken cancellationToken);
    Task<UserEditResultModel> EditUserAsync(UserEditRequestModel requestModel, CancellationToken cancellationToken);
    Task SetUserDeletedAsync(int userId, bool isDeleted, CancellationToken cancellationToken);
}
