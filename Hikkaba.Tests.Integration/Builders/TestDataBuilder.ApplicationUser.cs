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
    private readonly List<ApplicationUser> _users = [];
    private readonly List<(ApplicationUser User, ApplicationRole Role)> _pendingUserRoleAssignments = [];

    /// <summary>
    ///     Returns the default admin user (created via WithUser(Defaults.AdministratorUserName, isAdmin: true)).
    /// </summary>
    public ApplicationUser Admin => GetUser(Defaults.AdministratorUserName);

    /// <summary>
    ///     Returns the last created user.
    /// </summary>
    public ApplicationUser LastUser =>
        _users.LastOrDefault()
        ?? throw new InvalidOperationException("User not created. Call WithUser() first.");

    /// <summary>
    ///     Creates a user with the specified parameters.
    /// </summary>
    /// <param name="userName">The username for the user.</param>
    /// <param name="email">The email address. If null, defaults to "{userName}@example.com".</param>
    /// <param name="isDeleted">Whether the user is marked as deleted.</param>
    /// <param name="emailConfirmed">Whether the email is confirmed.</param>
    /// <param name="lastLoginAt">The last login timestamp.</param>
    /// <param name="lockoutEnabled">Whether lockout is enabled for the user.</param>
    /// <param name="lockoutEnd">The lockout end date.</param>
    /// <param name="isAdmin">Whether to assign the administrator role to the user.</param>
    /// <param name="isModerator">Whether to assign the moderator role to the user.</param>
    public TestDataBuilder WithUser(
        string userName,
        string? email = null,
        bool isDeleted = false,
        bool emailConfirmed = true,
        DateTime? lastLoginAt = null,
        bool lockoutEnabled = false,
        DateTimeOffset? lockoutEnd = null,
        bool isAdmin = false,
        bool isModerator = false)
    {
        var emailValue = email ?? $"{userName}@example.com";
        var user = new ApplicationUser
        {
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            Email = emailValue,
            NormalizedEmail = emailValue.ToUpperInvariant(),
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

        if (isAdmin)
        {
            WithAdministratorRole();
            WithUserRole(userName, Defaults.AdministratorRoleName);
        }

        if (isModerator)
        {
            WithModeratorRole();
            WithUserRole(userName, Defaults.ModeratorRoleName);
        }

        return this;
    }

    /// <summary>
    ///     Gets a user by username.
    /// </summary>
    public ApplicationUser GetUser(string userName)
    {
        return _users.Find(u => u.UserName == userName)
               ?? throw new InvalidOperationException($"User with username '{userName}' not found.");
    }

    /// <summary>
    ///     Assigns a role to a user. The role assignment is deferred until SaveAsync is called.
    ///     If the assignment already exists, does nothing.
    /// </summary>
    public TestDataBuilder WithUserRole(string userName, string roleName)
    {
        var user = GetUser(userName);
        var role = GetRole(roleName);

        // Check if assignment already exists
        if (_pendingUserRoleAssignments.Exists(x => x.User == user && x.Role == role))
        {
            return this;
        }

        _pendingUserRoleAssignments.Add((user, role));
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

    private void EnsureAdminExists()
    {
        var adminExists = _users.Exists(u => u.UserName == Defaults.AdministratorUserName);
        if (!adminExists)
        {
            throw new InvalidOperationException("Admin must be created first. Call WithUser(\"admin\", isAdmin: true).");
        }
    }
}
