using System;
using System.Collections.Generic;
using System.Linq;
using Hikkaba.Data.Entities;
using Hikkaba.Shared.Constants;

namespace Hikkaba.Tests.Integration.Builders;

internal sealed partial class TestDataBuilder
{
    private readonly List<ApplicationRole> _roles = [];

    /// <summary>
    ///     Returns the last created role.
    /// </summary>
    public ApplicationRole LastRole =>
        _roles.LastOrDefault()
        ?? throw new InvalidOperationException("Role not created. Call WithRole() first.");

    /// <summary>
    ///     Creates a role with the specified name. If the role already exists, does nothing.
    /// </summary>
    public TestDataBuilder WithRole(string roleName)
    {
        // Check if role already exists
        if (_roles.Exists(r => r.Name == roleName))
        {
            return this;
        }

        var role = new ApplicationRole
        {
            Name = roleName,
            NormalizedName = roleName.ToUpperInvariant(),
            ConcurrencyStamp = _guidGenerator.GenerateSeededGuid().ToString(),
        };
        _roles.Add(role);
        _dbContext.Roles.Add(role);
        return this;
    }

    /// <summary>
    ///     Creates the default administrator role.
    /// </summary>
    public TestDataBuilder WithAdministratorRole()
    {
        return WithRole(Defaults.AdministratorRoleName);
    }

    /// <summary>
    ///     Creates the default moderator role.
    /// </summary>
    public TestDataBuilder WithModeratorRole()
    {
        return WithRole(Defaults.ModeratorRoleName);
    }

    /// <summary>
    ///     Gets a role by name.
    /// </summary>
    public ApplicationRole GetRole(string roleName)
    {
        return _roles.Find(r => r.Name == roleName)
               ?? throw new InvalidOperationException($"Role with name '{roleName}' not found.");
    }
}
