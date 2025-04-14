using Hikkaba.Infrastructure.Models.Role;

namespace Hikkaba.Application.Contracts;

public interface IRoleService
{
    Task<IReadOnlyList<RoleModel>> ListRolesAsync(RoleFilter filter, CancellationToken cancellationToken);
}
