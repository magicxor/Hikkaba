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

internal sealed class GetUserTests : IntegrationTestBase
{
    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetUser_WhenUserExists_ReturnsUser(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithUser("test_user", email: "test@example.com", isAdmin: true)
            .SaveAsync(cancellationToken);

        var userId = builder.GetUser("test_user").Id;
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.GetUserAsync(userId, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(userId));
        Assert.That(result.UserName, Is.EqualTo("test_user"));
        Assert.That(result.Email, Is.EqualTo("test@example.com"));
        Assert.That(result.UserRoles, Has.Count.EqualTo(1));
        Assert.That(result.UserRoles[0].Name, Is.EqualTo(Defaults.AdministratorRoleName));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetUser_WhenUserDoesNotExist_ReturnsNull(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.GetUserAsync(999999, cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetUser_WhenUserHasNoRoles_ReturnsUserWithEmptyRoles(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithUser("user_without_role")
            .SaveAsync(cancellationToken);

        var userId = builder.GetUser("user_without_role").Id;
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.GetUserAsync(userId, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.UserRoles, Is.Empty);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetUser_WhenUserHasMultipleRoles_ReturnsAllRoles(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithUser("super_user", isAdmin: true, isModerator: true)
            .SaveAsync(cancellationToken);

        var userId = builder.GetUser("super_user").Id;
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.GetUserAsync(userId, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.UserRoles, Has.Count.EqualTo(2));
        Assert.That(result.UserRoles.Select(r => r.Name), Contains.Item(Defaults.AdministratorRoleName));
        Assert.That(result.UserRoles.Select(r => r.Name), Contains.Item(Defaults.ModeratorRoleName));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetUser_ReturnsCorrectUserDetails(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);
        var utcNow = builder.TimeProvider.GetUtcNow().UtcDateTime;
        var lockoutEndDate = utcNow.AddDays(7);

        await builder
            .WithUser(
                "detailed_user",
                email: "detailed@example.com",
                emailConfirmed: true,
                lastLoginAt: utcNow.AddDays(-3),
                lockoutEnabled: true,
                lockoutEnd: lockoutEndDate)
            .SaveAsync(cancellationToken);

        var userId = builder.GetUser("detailed_user").Id;
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.GetUserAsync(userId, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(userId));
        Assert.That(result.UserName, Is.EqualTo("detailed_user"));
        Assert.That(result.Email, Is.EqualTo("detailed@example.com"));
        Assert.That(result.EmailConfirmed, Is.True);
        Assert.That(result.IsDeleted, Is.False);
        Assert.That(result.LastLogin, Is.EqualTo(utcNow.AddDays(-3)).Within(TimeSpan.FromSeconds(1)));
        Assert.That(result.LockoutEnabled, Is.True);
        Assert.That(result.LockoutEnd, Is.Not.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetUser_WhenUserIsDeleted_ReturnsUser(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithUser("deleted_user", isDeleted: true)
            .SaveAsync(cancellationToken);

        var userId = builder.GetUser("deleted_user").Id;
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.GetUserAsync(userId, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsDeleted, Is.True);
    }
}
