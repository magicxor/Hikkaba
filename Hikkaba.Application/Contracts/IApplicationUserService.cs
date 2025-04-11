using Hikkaba.Infrastructure.Models.ApplicationUser;

namespace Hikkaba.Application.Contracts;

public interface IApplicationUserService
{
    Task<UserDetailsModel> GetUserAsync(int userId);

    Task<IReadOnlyList<UserDetailsModel>> ListUsersAsync(UserFilter filter);

    Task<int> CreateUserAsync(UserDetailsModel detailsModel);

    Task EditUserAsync(UserDetailsModel detailsModel);

    Task SetUserDeletedAsync(int id, bool newValue);
}
