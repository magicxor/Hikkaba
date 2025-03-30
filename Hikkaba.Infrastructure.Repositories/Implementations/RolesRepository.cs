using System.Globalization;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models;
using Hikkaba.Infrastructure.Models.ApplicationRole;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public class RolesRepository : IRolesRepository
{
    private readonly RoleManager<ApplicationRole> _roleMgr;

    public RolesRepository(RoleManager<ApplicationRole> roleMgr)
    {
        _roleMgr = roleMgr;
    }

    public async Task<IReadOnlyList<ApplicationRoleModel>> ListRolesAsync()
    {
        return await _roleMgr.Roles
            .Select(x => new ApplicationRoleModel
            {
                Id = x.Id,
                NormalizedName = x.NormalizedName ?? string.Empty,
            })
            .ToListAsync();
    }

    public async Task<int> CreateAsync(string roleName)
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

    public async Task EditAsync(int roleId, string roleName)
    {
        var role = await _roleMgr.FindByIdAsync(roleId.ToString(CultureInfo.InvariantCulture));

        if (role == null)
        {
            throw new HikkabaDomainException("Role not found");
        }

        role.Name = roleName;
        await _roleMgr.UpdateAsync(role);
    }

    public async Task DeleteAsync(int roleId)
    {
        var role = await _roleMgr.FindByIdAsync(roleId.ToString(CultureInfo.InvariantCulture));

        if (role == null)
        {
            throw new HikkabaDomainException("Role not found");
        }

        await _roleMgr.DeleteAsync(role);
    }
}
