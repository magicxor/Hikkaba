using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Entities;
using Hikkaba.Shared.Constants;
using Microsoft.AspNetCore.Identity;

namespace Hikkaba.Tests.Integration.Builders;

internal sealed partial class TestDataBuilder
{
    private readonly List<ApplicationRole> _roles = [];
    private readonly List<(ApplicationUser User, ApplicationRole Role)> _pendingUserRoleAssignments = [];
    private readonly List<ApplicationUser> _users = [];

    public IReadOnlyList<ApplicationRole> Roles => _roles;
    public IReadOnlyList<ApplicationUser> Users => _users;

    /// <summary>
    ///     Returns the last created role.
    /// </summary>
    public ApplicationRole LastRole =>
        _roles.LastOrDefault()
        ?? throw new InvalidOperationException("Role not created. Call WithRole() first.");

    /// <summary>
    ///     Returns the last created user.
    /// </summary>
    public ApplicationUser LastUser =>
        _users.LastOrDefault()
        ?? throw new InvalidOperationException("User not created. Call WithUser() first.");

    /// <summary>
    ///     Creates a role with the specified name.
    /// </summary>
    public TestDataBuilder WithRole(string roleName)
    {
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

    /// <summary>
    ///     Creates a user with the specified username and email.
    /// </summary>
    public TestDataBuilder WithUser(
        string userName,
        string? email = null,
        bool isDeleted = false,
        bool emailConfirmed = true,
        DateTime? lastLoginAt = null,
        bool lockoutEnabled = false,
        DateTimeOffset? lockoutEnd = null)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            Email = email ?? $"{userName}@example.com",
            NormalizedEmail = (email ?? $"{userName}@example.com").ToUpperInvariant(),
            EmailConfirmed = emailConfirmed,
            IsDeleted = isDeleted,
            LastLoginAt = lastLoginAt,
            LockoutEnabled = lockoutEnabled,
            LockoutEnd = lockoutEnd,
            SecurityStamp = _guidGenerator.GenerateSeededGuid().ToString(),
            ConcurrencyStamp = _guidGenerator.GenerateSeededGuid().ToString(),
            CreatedAt = TimeProvider.GetUtcNow().UtcDateTime,
        };
        _users.Add(user);
        _dbContext.Users.Add(user);
        return this;
    }

    /// <summary>
    ///     Gets a user by username.
    /// </summary>
    public ApplicationUser GetUser(string userName)
    {
        return _users.Find(u => u.UserName == userName)
               ?? _moderators.Find(m => m.UserName == userName)
               ?? (_admin?.UserName == userName ? _admin : null)
               ?? throw new InvalidOperationException($"User with username '{userName}' not found.");
    }

    /// <summary>
    ///     Assigns a role to a user. The role assignment is deferred until SaveAsync is called.
    /// </summary>
    public TestDataBuilder WithUserRole(string userName, string roleName)
    {
        var user = GetUser(userName);
        var role = GetRole(roleName);

        _pendingUserRoleAssignments.Add((user, role));
        return this;
    }

    /// <summary>
    ///     Assigns a role to the last created user. The role assignment is deferred until SaveAsync is called.
    /// </summary>
    public TestDataBuilder WithRoleForLastUser(string roleName)
    {
        var user = LastUser;
        var role = GetRole(roleName);

        _pendingUserRoleAssignments.Add((user, role));
        return this;
    }

    /// <summary>
    ///     Creates a user and assigns a role in one call.
    /// </summary>
    public TestDataBuilder WithUserInRole(
        string userName,
        string roleName,
        string? email = null,
        bool isDeleted = false)
    {
        WithUser(userName, email, isDeleted);
        WithUserRole(userName, roleName);
        return this;
    }

    /// <summary>
    ///     Applies pending user role assignments after users and roles have been saved.
    /// </summary>
    private async Task ApplyPendingUserRoleAssignmentsAsync(CancellationToken cancellationToken)
    {
        if (_pendingUserRoleAssignments.Count == 0)
        {
            return;
        }

        foreach (var (user, role) in _pendingUserRoleAssignments)
        {
            _dbContext.UserRoles.Add(new IdentityUserRole<int>
            {
                UserId = user.Id,
                RoleId = role.Id,
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _pendingUserRoleAssignments.Clear();
    }
}
