using Hikkaba.Infrastructure.Models.Role;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface IRoleRepository
{
    Task<IReadOnlyList<RoleModel>> ListRolesAsync(RoleFilter filter, CancellationToken cancellationToken);
    Task<RoleCreateResultModel> CreateRoleAsync(RoleCreateRequestModel requestModel, CancellationToken cancellationToken);
    Task<RoleEditResultModel> EditRoleAsync(RoleEditRequestModel requestModel, CancellationToken cancellationToken);
    Task<RoleDeleteResultModel> DeleteRoleAsync(int roleId, CancellationToken cancellationToken);
}
