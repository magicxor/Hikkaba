using Hikkaba.Infrastructure.Models.ApplicationUser;

namespace Hikkaba.Application.Contracts;

public interface IApplicationUserService
{
    Task<ApplicationUserDetailsModel> GetAsync(int id);

    Task<IReadOnlyList<ApplicationUserDetailsModel>> ListAsync(ApplicationUserFilter filter);

    Task<int> CreateAsync(ApplicationUserDetailsModel detailsModel);

    Task EditAsync(ApplicationUserDetailsModel detailsModel);

    Task SetIsDeletedAsync(int id, bool newValue);
}
