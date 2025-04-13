using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.User;
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

    public async Task<UserDetailsModel?> GetUserAsync(int userId, CancellationToken cancellationToken)
    {
        return await _userRepository.GetUserAsync(userId, cancellationToken);
    }

    public async Task<UserCreateResultModel> CreateUserAsync(UserCreateRequestModel requestModel, CancellationToken cancellationToken)
    {
        return await _userRepository.CreateUserAsync(requestModel, cancellationToken);
    }

    public async Task<UserEditResultModel> EditUserAsync(UserEditRequestModel requestModel, CancellationToken cancellationToken)
    {
        return await _userRepository.EditUserAsync(requestModel, cancellationToken);
    }

    public async Task SetUserDeletedAsync(int userId, bool isDeleted, CancellationToken cancellationToken)
    {
        await _userRepository.SetUserDeletedAsync(userId, isDeleted, cancellationToken);
    }
}
