using Hikkaba.Infrastructure.Models;

namespace Hikkaba.Repositories.Contracts;

public interface IRolesRepository
{
    Task<IReadOnlyList<ApplicationRoleDto>> ListRolesAsync();
    Task<int> CreateAsync(string roleName);
    Task EditAsync(int roleId, string roleName);
    Task DeleteAsync(int roleId);
}
