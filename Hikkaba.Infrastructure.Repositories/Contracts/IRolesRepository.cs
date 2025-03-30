using Hikkaba.Infrastructure.Models;
using Hikkaba.Infrastructure.Models.ApplicationRole;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface IRolesRepository
{
    Task<IReadOnlyList<ApplicationRoleModel>> ListRolesAsync();
    Task<int> CreateAsync(string roleName);
    Task EditAsync(int roleId, string roleName);
    Task DeleteAsync(int roleId);
}
