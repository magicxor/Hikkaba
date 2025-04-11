using Hikkaba.Infrastructure.Models;
using Hikkaba.Infrastructure.Models.ApplicationRole;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface IRoleRepository
{
    Task<IReadOnlyList<ApplicationRoleModel>> ListRolesAsync(CancellationToken cancellationToken);
    Task<RoleCreateResultModel> CreateRoleAsync(RoleCreateRequestModel requestModel, CancellationToken cancellationToken);
    Task<RoleEditResultModel> EditRoleAsync(RoleEditRequestModel requestModel, CancellationToken cancellationToken);
    Task<RoleDeleteResultModel> DeleteRoleAsync(int roleId, CancellationToken cancellationToken);
}
