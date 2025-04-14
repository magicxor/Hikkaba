using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Role;
using Hikkaba.Infrastructure.Repositories.Contracts;

namespace Hikkaba.Application.Implementations;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;

    public RoleService(
        IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<IReadOnlyList<RoleModel>> ListRolesAsync(RoleFilter filter, CancellationToken cancellationToken)
    {
        return await _roleRepository.ListRolesAsync(filter, cancellationToken);
    }
}
