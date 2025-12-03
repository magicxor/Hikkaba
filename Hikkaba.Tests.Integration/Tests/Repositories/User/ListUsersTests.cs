using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Models.User;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Constants;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.User;

internal sealed class ListUsersTests : IntegrationTestBase
{
    #region IncludeDeleted filter tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase(true, 3)]
    [TestCase(false, 2)]
    public async Task ListUsers_WhenIncludeDeleted_ReturnsExpectedCount(
        bool includeDeleted,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithAdministratorRole()
            .WithUser("active_user1")
            .WithUser("active_user2")
            .WithUser("deleted_user", isDeleted: true)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.ListUsersAsync(new UserFilter
        {
            IncludeDeleted = includeDeleted,
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(expectedCount));
    }

    #endregion

    #region User roles tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListUsers_WhenUserHasRoles_ReturnsUserWithRoles(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithUser("admin_user", isAdmin: true)
            .WithUser("mod_user", isModerator: true)
            .WithUser("user_without_role")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.ListUsersAsync(new UserFilter
        {
            IncludeDeleted = false,
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));

        var adminUser = result.Single(u => u.UserName == "admin_user");
        Assert.That(adminUser.UserRoles, Has.Count.EqualTo(1));
        Assert.That(adminUser.UserRoles[0].Name, Is.EqualTo(Defaults.AdministratorRoleName));

        var modUser = result.Single(u => u.UserName == "mod_user");
        Assert.That(modUser.UserRoles, Has.Count.EqualTo(1));
        Assert.That(modUser.UserRoles[0].Name, Is.EqualTo(Defaults.ModeratorRoleName));

        var userWithoutRole = result.Single(u => u.UserName == "user_without_role");
        Assert.That(userWithoutRole.UserRoles, Is.Empty);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListUsers_WhenUserHasMultipleRoles_ReturnsAllRoles(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithUser("super_user", isAdmin: true, isModerator: true)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.ListUsersAsync(new UserFilter
        {
            IncludeDeleted = false,
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var user = result[0];
        Assert.That(user.UserRoles, Has.Count.EqualTo(2));
        Assert.That(user.UserRoles.Select(r => r.Name), Contains.Item(Defaults.AdministratorRoleName));
        Assert.That(user.UserRoles.Select(r => r.Name), Contains.Item(Defaults.ModeratorRoleName));
    }

    #endregion

    #region User details tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListUsers_ReturnsCorrectUserDetails(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);
        var utcNow = builder.TimeProvider.GetUtcNow().UtcDateTime;

        await builder
            .WithUser(
                "test_user",
                email: "test@example.com",
                emailConfirmed: true,
                lastLoginAt: utcNow.AddDays(-1),
                lockoutEnabled: true,
                lockoutEnd: utcNow.AddDays(1))
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.ListUsersAsync(new UserFilter
        {
            IncludeDeleted = false,
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var user = result[0];
        Assert.That(user.UserName, Is.EqualTo("test_user"));
        Assert.That(user.Email, Is.EqualTo("test@example.com"));
        Assert.That(user.EmailConfirmed, Is.True);
        Assert.That(user.LastLogin, Is.EqualTo(utcNow.AddDays(-1)).Within(TimeSpan.FromSeconds(1)));
        Assert.That(user.LockoutEnabled, Is.True);
        Assert.That(user.LockoutEnd, Is.Not.Null);
        Assert.That(user.IsDeleted, Is.False);
    }

    #endregion

    #region Empty result tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListUsers_WhenNoUsers_ReturnsEmptyList(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.ListUsersAsync(new UserFilter
        {
            IncludeDeleted = false,
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListUsers_WhenAllUsersDeleted_AndIncludeDeletedFalse_ReturnsEmptyList(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithUser("deleted_user1", isDeleted: true)
            .WithUser("deleted_user2", isDeleted: true)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.ListUsersAsync(new UserFilter
        {
            IncludeDeleted = false,
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.Empty);
    }

    #endregion
}
