using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.ApplicationUser;
using Hikkaba.Infrastructure.Repositories.Contracts;

namespace Hikkaba.Application.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserDetailsModel>> ListUsersAsync(UserFilter filter, CancellationToken cancellationToken)
    {
        return await _userRepository.ListUsersAsync(filter, cancellationToken);
    }

    public async Task<IReadOnlyList<CategoryModeratorModel>> ListCategoryModerators(CategoryModeratorFilter filter, CancellationToken cancellationToken)
    {
        return await _userRepository.ListCategoryModerators(filter, cancellationToken);
    }

    public async Task<UserCreateResultModel> CreateUserAsync(UserCreateRequestModel requestModel, CancellationToken cancellationToken)
    {
        return await _userRepository.CreateUserAsync(requestModel, cancellationToken);
    }

    public async Task SetUserDeletedAsync(int userId, bool isDeleted, CancellationToken cancellationToken)
    {
        await _userRepository.SetUserDeletedAsync(userId, isDeleted, cancellationToken);
    }
}
