using Hikkaba.Infrastructure.Models.ApplicationUser;

namespace Hikkaba.Application.Contracts;

public interface IApplicationUserService
{
    Task<ApplicationUserDetailsModel> GetUserAsync(int id);

    Task<IReadOnlyList<ApplicationUserDetailsModel>> ListUsersAsync(ApplicationUserFilter filter);

    Task<int> CreateUserAsync(ApplicationUserDetailsModel detailsModel);

    Task EditUserAsync(ApplicationUserDetailsModel detailsModel);

    Task SetUserDeletedAsync(int id, bool newValue);
}
