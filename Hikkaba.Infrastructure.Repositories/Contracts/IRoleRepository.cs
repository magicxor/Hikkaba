using Hikkaba.Infrastructure.Models;
using Hikkaba.Infrastructure.Models.ApplicationRole;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface IRoleRepository
{
    Task<IReadOnlyList<ApplicationRoleModel>> ListRolesAsync(CancellationToken cancellationToken);
    Task<int> CreateAsync(string roleName, CancellationToken cancellationToken);
    Task EditAsync(int roleId, string roleName, CancellationToken cancellationToken);
    Task DeleteAsync(int roleId, CancellationToken cancellationToken);
}
