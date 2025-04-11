using System.Globalization;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models;
using Hikkaba.Infrastructure.Models.ApplicationRole;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public class RoleRepository : IRoleRepository
{
    private readonly RoleManager<ApplicationRole> _roleMgr;

    public RoleRepository(RoleManager<ApplicationRole> roleMgr)
    {
        _roleMgr = roleMgr;
    }

    public async Task<IReadOnlyList<ApplicationRoleModel>> ListRolesAsync(CancellationToken cancellationToken)
    {
        return await _roleMgr.Roles
            .Select(x => new ApplicationRoleModel
            {
                Id = x.Id,
                NormalizedName = x.NormalizedName ?? string.Empty,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CreateRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        var result = await _roleMgr.CreateAsync(new ApplicationRole
        {
            Name = roleName,
        });

        if (result.Succeeded)
        {
            var role = await _roleMgr.FindByNameAsync(roleName);
            var roleId = role?.Id;

            if (roleId != null)
            {
                return roleId.Value;
            }
            else
            {
                throw new HikkabaDomainException("Role not found after creation");
            }
        }
        else
        {
            throw new HikkabaDomainException($"Role creation failed: {string.Join(',', result.Errors.Select(x => x.Description))}");
        }
    }

    public async Task EditRoleAsync(int roleId, string roleName, CancellationToken cancellationToken)
    {
        var role = await _roleMgr.FindByIdAsync(roleId.ToString(CultureInfo.InvariantCulture));

        if (role == null)
        {
            throw new HikkabaDomainException("Role not found");
        }

        role.Name = roleName;
        await _roleMgr.UpdateAsync(role);
    }

    public async Task DeleteRoleAsync(int roleId, CancellationToken cancellationToken)
    {
        var role = await _roleMgr.FindByIdAsync(roleId.ToString(CultureInfo.InvariantCulture));

        if (role == null)
        {
            throw new HikkabaDomainException("Role not found");
        }

        await _roleMgr.DeleteAsync(role);
    }
}
