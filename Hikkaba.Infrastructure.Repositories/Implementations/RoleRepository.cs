using System.Globalization;
using System.Net;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Error;
using Hikkaba.Infrastructure.Models.Role;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OneOf.Types;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public sealed class RoleRepository : IRoleRepository
{
    private readonly RoleManager<ApplicationRole> _roleMgr;

    public RoleRepository(RoleManager<ApplicationRole> roleMgr)
    {
        _roleMgr = roleMgr;
    }

    public async Task<IReadOnlyList<RoleModel>> ListRolesAsync(RoleFilter filter, CancellationToken cancellationToken)
    {
        return await _roleMgr.Roles
            .Select(x => new RoleModel
            {
                Id = x.Id,
                Name = x.Name ?? string.Empty,
                NormalizedName = x.NormalizedName ?? string.Empty,
            })
            .ApplyOrderBy(filter, x => x.NormalizedName)
            .ToListAsync(cancellationToken);
    }

    public async Task<RoleCreateResultModel> CreateRoleAsync(RoleCreateRequestModel requestModel, CancellationToken cancellationToken)
    {
        var role = new ApplicationRole
        {
            Name = requestModel.RoleName,
        };
        var result = await _roleMgr.CreateAsync(role);

        return result.Succeeded
            ? new RoleCreateResultSuccessModel { RoleId = role.Id }
            : new DomainError
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                ErrorMessage = $"Role creation failed: {result}",
            };
    }

    public async Task<RoleEditResultModel> EditRoleAsync(RoleEditRequestModel requestModel, CancellationToken cancellationToken)
    {
        var role = await _roleMgr.FindByIdAsync(requestModel.RoleId.ToString(CultureInfo.InvariantCulture));

        if (role == null)
        {
            return new DomainError
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                ErrorMessage = "Role not found",
            };
        }

        role.Name = requestModel.RoleName;
        var result = await _roleMgr.UpdateAsync(role);

        return result.Succeeded
            ? default(Success)
            : new DomainError
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                ErrorMessage = $"Role update failed: {result}",
            };
    }

    public async Task<RoleDeleteResultModel> DeleteRoleAsync(int roleId, CancellationToken cancellationToken)
    {
        var role = await _roleMgr.FindByIdAsync(roleId.ToString(CultureInfo.InvariantCulture));

        if (role == null)
        {
            return new DomainError
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                ErrorMessage = "Role not found",
            };
        }

        var result = await _roleMgr.DeleteAsync(role);

        return result.Succeeded
            ? default(Success)
            : new DomainError
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                ErrorMessage = $"Role deletion failed: {result}",
            };
    }
}
