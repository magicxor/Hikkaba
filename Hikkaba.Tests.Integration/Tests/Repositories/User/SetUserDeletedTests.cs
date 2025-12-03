using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.User;

internal sealed class SetUserDeletedTests : IntegrationTestBase
{
    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetUserDeleted_WhenSettingToDeleted_MarksUserAsDeleted(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithUser("test_user")
            .SaveAsync(cancellationToken);

        var userId = builder.GetUser("test_user").Id;
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        await repository.SetUserDeletedAsync(userId, isDeleted: true, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.ChangeTracker.Clear(); // Clear tracked entities to get fresh data
        var user = await dbContext.Users.FirstAsync(u => u.Id == userId, cancellationToken);
        Assert.That(user.IsDeleted, Is.True);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetUserDeleted_WhenRestoringUser_MarksUserAsNotDeleted(
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
        await repository.SetUserDeletedAsync(userId, isDeleted: false, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.ChangeTracker.Clear(); // Clear tracked entities to get fresh data
        var user = await dbContext.Users.FirstAsync(u => u.Id == userId, cancellationToken);
        Assert.That(user.IsDeleted, Is.False);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetUserDeleted_WhenAlreadyDeleted_KeepsUserDeleted(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithUser("already_deleted_user", isDeleted: true)
            .SaveAsync(cancellationToken);

        var userId = builder.GetUser("already_deleted_user").Id;
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        await repository.SetUserDeletedAsync(userId, isDeleted: true, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.ChangeTracker.Clear(); // Clear tracked entities to get fresh data
        var user = await dbContext.Users.FirstAsync(u => u.Id == userId, cancellationToken);
        Assert.That(user.IsDeleted, Is.True);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetUserDeleted_DoesNotAffectOtherUsers(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithUser("user_to_delete")
            .WithUser("other_user")
            .SaveAsync(cancellationToken);

        var userToDeleteId = builder.GetUser("user_to_delete").Id;
        var otherUserId = builder.GetUser("other_user").Id;
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        await repository.SetUserDeletedAsync(userToDeleteId, isDeleted: true, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.ChangeTracker.Clear(); // Clear tracked entities to get fresh data
        var deletedUser = await dbContext.Users.FirstAsync(u => u.Id == userToDeleteId, cancellationToken);
        var otherUser = await dbContext.Users.FirstAsync(u => u.Id == otherUserId, cancellationToken);

        Assert.That(deletedUser.IsDeleted, Is.True);
        Assert.That(otherUser.IsDeleted, Is.False);
    }
}
