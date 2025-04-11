using Hikkaba.Infrastructure.Models;
using Hikkaba.Infrastructure.Models.ApplicationRole;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface IRoleRepository
{
    Task<IReadOnlyList<ApplicationRoleModel>> ListRolesAsync(CancellationToken cancellationToken);
    Task<int> CreateRoleAsync(string roleName, CancellationToken cancellationToken);
    Task EditRoleAsync(int roleId, string roleName, CancellationToken cancellationToken);
    Task DeleteRoleAsync(int roleId, CancellationToken cancellationToken);
}
