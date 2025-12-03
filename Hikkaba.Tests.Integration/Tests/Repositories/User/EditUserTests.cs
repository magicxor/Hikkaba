using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Error;
using Hikkaba.Infrastructure.Models.User;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Constants;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OneOf.Types;

namespace Hikkaba.Tests.Integration.Tests.Repositories.User;

internal sealed class EditUserTests : IntegrationTestBase
{
    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditUser_WhenValidRequest_UpdatesUserSuccessfully(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithAdministratorRole()
            .WithUser("test_user", email: "old@example.com")
            .SaveAsync(cancellationToken);

        var userId = builder.GetUser("test_user").Id;
        var adminRoleId = builder.GetRole(Defaults.AdministratorRoleName).Id;
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        var request = new UserEditRequestModel
        {
            Id = userId,
            Email = "new@example.com",
            UserName = "updated_user",
            LockoutEndDate = null,
            TwoFactorEnabled = true,
            UserRoleIds = [adminRoleId],
        };

        // Act
        var result = await repository.EditUserAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<Success>(), "Expected success result");

        // Verify user was updated in DB
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedUser = await dbContext.Users.FirstAsync(u => u.Id == userId, cancellationToken);
        Assert.That(updatedUser.UserName, Is.EqualTo("updated_user"));
        Assert.That(updatedUser.Email, Is.EqualTo("new@example.com"));
        Assert.That(updatedUser.TwoFactorEnabled, Is.True);

        // Verify role was assigned
        var userRoles = await dbContext.UserRoles.Where(ur => ur.UserId == userId).ToListAsync(cancellationToken);
        Assert.That(userRoles, Has.Count.EqualTo(1));
        Assert.That(userRoles[0].RoleId, Is.EqualTo(adminRoleId));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditUser_WhenUserNotFound_ReturnsError(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        var request = new UserEditRequestModel
        {
            Id = 999999, // Non-existent user
            Email = "new@example.com",
            UserName = "updated_user",
            LockoutEndDate = null,
            TwoFactorEnabled = false,
            UserRoleIds = [],
        };

        // Act
        var result = await repository.EditUserAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<DomainError>(), "Expected error result");
        var error = result.AsT1;
        Assert.That(error.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditUser_WhenChangingRoles_UpdatesRolesCorrectly(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithModeratorRole()
            .WithUser("test_user", isAdmin: true)
            .SaveAsync(cancellationToken);

        var userId = builder.GetUser("test_user").Id;
        var moderatorRoleId = builder.GetRole(Defaults.ModeratorRoleName).Id;
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        var request = new UserEditRequestModel
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "test_user",
            LockoutEndDate = null,
            TwoFactorEnabled = false,
            UserRoleIds = [moderatorRoleId], // Change from admin to moderator
        };

        // Act
        var result = await repository.EditUserAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<Success>(), "Expected success result");

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userRoles = await dbContext.UserRoles.Where(ur => ur.UserId == userId).ToListAsync(cancellationToken);
        Assert.That(userRoles, Has.Count.EqualTo(1));
        Assert.That(userRoles[0].RoleId, Is.EqualTo(moderatorRoleId));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditUser_WhenRemovingAllRoles_RemovesAllRoles(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithUser("test_user", isAdmin: true)
            .SaveAsync(cancellationToken);

        var userId = builder.GetUser("test_user").Id;
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        var request = new UserEditRequestModel
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "test_user",
            LockoutEndDate = null,
            TwoFactorEnabled = false,
            UserRoleIds = [], // Remove all roles
        };

        // Act
        var result = await repository.EditUserAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<Success>(), "Expected success result");

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userRoles = await dbContext.UserRoles.Where(ur => ur.UserId == userId).ToListAsync(cancellationToken);
        Assert.That(userRoles, Is.Empty);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditUser_WhenSettingLockoutEndDate_SetsLockoutCorrectly(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);
        var utcNow = builder.TimeProvider.GetUtcNow().UtcDateTime;
        var lockoutEndDate = utcNow.AddDays(7);

        await builder
            .WithUser("test_user", lockoutEnabled: true) // LockoutEnabled must be true for SetLockoutEndDateAsync to work
            .SaveAsync(cancellationToken);

        var userId = builder.GetUser("test_user").Id;
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        var request = new UserEditRequestModel
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "test_user",
            LockoutEndDate = lockoutEndDate,
            TwoFactorEnabled = false,
            UserRoleIds = [],
        };

        // Act
        var result = await repository.EditUserAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<Success>(), "Expected success result");

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.ChangeTracker.Clear(); // Clear cached entities to get fresh data from the database
        var updatedUser = await dbContext.Users.FirstAsync(u => u.Id == userId, cancellationToken);
        Assert.That(updatedUser.LockoutEnd, Is.Not.Null);
        Assert.That(updatedUser.LockoutEnd!.Value.UtcDateTime, Is.EqualTo(lockoutEndDate).Within(TimeSpan.FromSeconds(1)));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditUser_WhenAssigningMultipleRoles_AssignsAllRoles(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithAdministratorRole()
            .WithModeratorRole()
            .WithUser("test_user")
            .SaveAsync(cancellationToken);

        var userId = builder.GetUser("test_user").Id;
        var adminRoleId = builder.GetRole(Defaults.AdministratorRoleName).Id;
        var moderatorRoleId = builder.GetRole(Defaults.ModeratorRoleName).Id;
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        var request = new UserEditRequestModel
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "test_user",
            LockoutEndDate = null,
            TwoFactorEnabled = false,
            UserRoleIds = [adminRoleId, moderatorRoleId],
        };

        // Act
        var result = await repository.EditUserAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<Success>(), "Expected success result");

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userRoles = await dbContext.UserRoles.Where(ur => ur.UserId == userId).ToListAsync(cancellationToken);
        Assert.That(userRoles, Has.Count.EqualTo(2));
        Assert.That(userRoles.Select(ur => ur.RoleId), Contains.Item(adminRoleId));
        Assert.That(userRoles.Select(ur => ur.RoleId), Contains.Item(moderatorRoleId));
    }
}
