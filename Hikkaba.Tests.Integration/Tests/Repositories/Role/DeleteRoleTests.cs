using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Error;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Constants;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OneOf.Types;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Role;

internal sealed class DeleteRoleTests : IntegrationTestBase
{
    #region Basic delete tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task DeleteRole_WhenValidRoleId_DeletesRoleSuccessfully(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithRole("RoleToDelete");
        await builder.SaveAsync(cancellationToken);

        var role = builder.GetRole("RoleToDelete");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Act
        var result = await repository.DeleteRoleAsync(role.Id, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<Success>(), "Expected successful role deletion");

        // Verify role was deleted from DB
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deletedRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == role.Id, cancellationToken);
        Assert.That(deletedRole, Is.Null, "Role should be deleted from database");
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task DeleteRole_WhenDeleted_RoleNoLongerInList(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithRole("Role1")
            .WithRole("Role2")
            .WithRole("Role3");
        await builder.SaveAsync(cancellationToken);

        var role2 = builder.GetRole("Role2");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Verify role exists before deletion
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var rolesBefore = await dbContext.Roles.CountAsync(cancellationToken);
        Assert.That(rolesBefore, Is.EqualTo(3));

        // Act
        var result = await repository.DeleteRoleAsync(role2.Id, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<Success>(), "Expected successful role deletion");

        var rolesAfter = await dbContext.Roles.CountAsync(cancellationToken);
        Assert.That(rolesAfter, Is.EqualTo(2));

        var remainingRoles = await dbContext.Roles.Select(r => r.Name).ToListAsync(cancellationToken);
        Assert.That(remainingRoles, Does.Contain("Role1"));
        Assert.That(remainingRoles, Does.Not.Contain("Role2"));
        Assert.That(remainingRoles, Does.Contain("Role3"));
    }

    #endregion

    #region Not found tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task DeleteRole_WhenRoleNotFound_ReturnsNotFoundError(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Act
        var result = await repository.DeleteRoleAsync(999999, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<DomainError>(), "Expected error result for non-existent role");
        var error = result.AsT1;
        Assert.That(error.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
        Assert.That(error.ErrorMessage, Is.EqualTo("Role not found."));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task DeleteRole_WhenZeroRoleId_ReturnsNotFoundError(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Act
        var result = await repository.DeleteRoleAsync(0, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<DomainError>());
        var error = result.AsT1;
        Assert.That(error.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task DeleteRole_WhenNegativeRoleId_ReturnsNotFoundError(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Act
        var result = await repository.DeleteRoleAsync(-1, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<DomainError>());
        var error = result.AsT1;
        Assert.That(error.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
    }

    #endregion

    #region Double delete tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task DeleteRole_WhenDeletedTwice_SecondDeleteReturnsNotFound(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithRole("RoleToDelete");
        await builder.SaveAsync(cancellationToken);

        var role = builder.GetRole("RoleToDelete");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Act - First delete
        var result1 = await repository.DeleteRoleAsync(role.Id, cancellationToken);
        Assert.That(result1.IsT0, Is.True, "First delete should succeed");

        // Act - Second delete
        var result2 = await repository.DeleteRoleAsync(role.Id, cancellationToken);

        // Assert
        Assert.That(result2.IsT1, Is.True, "Second delete should fail");
        var error = result2.AsT1;
        Assert.That(error.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
    }

    #endregion

    #region Delete with user associations tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task DeleteRole_WhenRoleHasUsers_DeletesRoleAndUserRoleAssociations(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithRole("CustomRole")
            .WithUser("testuser");
        await builder.SaveAsync(cancellationToken);

        var role = builder.GetRole("CustomRole");

        // Manually add user to role using the builder's pattern
        builder.WithUserRole("testuser", "CustomRole");
        await builder.SaveAsync(cancellationToken);

        var user = builder.GetUser("testuser");

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Verify user-role association exists
        var userRoleBefore = await dbContext.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id, cancellationToken);
        Assert.That(userRoleBefore, Is.Not.Null, "User-role association should exist before deletion");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Act
        var result = await repository.DeleteRoleAsync(role.Id, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<Success>(), "Expected successful role deletion");

        // Verify role is deleted
        var deletedRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == role.Id, cancellationToken);
        Assert.That(deletedRole, Is.Null, "Role should be deleted");

        // Verify user-role association is deleted (cascade delete)
        var userRoleAfter = await dbContext.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id, cancellationToken);
        Assert.That(userRoleAfter, Is.Null, "User-role association should be deleted");

        // Verify user still exists
        var userAfter = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);
        Assert.That(userAfter, Is.Not.Null, "User should still exist after role deletion");
    }

    #endregion

    #region Delete default roles tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task DeleteRole_WhenDeletingAdministratorRole_DeletesSuccessfully(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithAdministratorRole();
        await builder.SaveAsync(cancellationToken);

        var adminRole = builder.GetRole(Defaults.AdministratorRoleName);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Act
        var result = await repository.DeleteRoleAsync(adminRole.Id, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<Success>(), "Expected successful role deletion");

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deletedRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == adminRole.Id, cancellationToken);
        Assert.That(deletedRole, Is.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task DeleteRole_WhenDeletingModeratorRole_DeletesSuccessfully(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithModeratorRole();
        await builder.SaveAsync(cancellationToken);

        var moderatorRole = builder.GetRole(Defaults.ModeratorRoleName);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Act
        var result = await repository.DeleteRoleAsync(moderatorRole.Id, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<Success>(), "Expected successful role deletion");

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deletedRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == moderatorRole.Id, cancellationToken);
        Assert.That(deletedRole, Is.Null);
    }

    #endregion

    #region Multiple roles deletion tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task DeleteRole_WhenDeletingMultipleRoles_AllAreDeleted(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithRole("Role1")
            .WithRole("Role2")
            .WithRole("Role3");
        await builder.SaveAsync(cancellationToken);

        var role1 = builder.GetRole("Role1");
        var role2 = builder.GetRole("Role2");
        var role3 = builder.GetRole("Role3");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Act
        var result1 = await repository.DeleteRoleAsync(role1.Id, cancellationToken);
        var result2 = await repository.DeleteRoleAsync(role2.Id, cancellationToken);
        var result3 = await repository.DeleteRoleAsync(role3.Id, cancellationToken);

        // Assert
        Assert.That(result1.IsT0, Is.True);
        Assert.That(result2.IsT0, Is.True);
        Assert.That(result3.IsT0, Is.True);

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var remainingRoles = await dbContext.Roles.CountAsync(cancellationToken);
        Assert.That(remainingRoles, Is.EqualTo(0), "All roles should be deleted");
    }

    #endregion
}
