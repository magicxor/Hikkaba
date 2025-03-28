using Hikkaba.Infrastructure.Models.ApplicationUser;

namespace Hikkaba.Services.Contracts;

public interface IApplicationUserService
{
    Task<ApplicationUserViewRm> GetAsync(int id);

    Task<IReadOnlyList<ApplicationUserViewRm>> ListAsync(ApplicationUserFilter filter);

    Task<int> CreateAsync(ApplicationUserViewRm viewRm);

    Task EditAsync(ApplicationUserViewRm viewRm);

    Task SetIsDeletedAsync(int id, bool newValue);
}
