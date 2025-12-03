using System;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Error;
using Hikkaba.Infrastructure.Models.User;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.User;

internal sealed class CreateUserTests : IntegrationTestBase
{
    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateUser_WhenValidRequest_CreatesUserSuccessfully(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        var request = new UserCreateRequestModel
        {
            Email = "newuser@example.com",
            UserName = "new_user",
            Password = "StrongPassword123!",
            UserRoleIds = [],
        };

        // Act
        var result = await repository.CreateUserAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<UserCreateResultSuccessModel>(), "Expected success result");
        var successResult = result.AsT0;
        Assert.That(successResult.UserId, Is.GreaterThan(0));

        // Verify user was created in DB
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var createdUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == successResult.UserId, cancellationToken);
        Assert.That(createdUser, Is.Not.Null);
        Assert.That(createdUser!.UserName, Is.EqualTo("new_user"));
        Assert.That(createdUser.Email, Is.EqualTo("newuser@example.com"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateUser_WhenDuplicateUserName_ReturnsError(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithUser("existing_user")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        var request = new UserCreateRequestModel
        {
            Email = "another@example.com",
            UserName = "existing_user", // Duplicate username
            Password = "StrongPassword123!",
            UserRoleIds = [],
        };

        // Act
        var result = await repository.CreateUserAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<DomainError>(), "Expected error result");
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateUser_WhenWeakPassword_ReturnsError(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        var request = new UserCreateRequestModel
        {
            Email = "newuser@example.com",
            UserName = "new_user",
            Password = "weak", // Weak password
            UserRoleIds = [],
        };

        // Act
        var result = await repository.CreateUserAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<DomainError>(), "Expected error result for weak password");
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateUser_SetsCreatedAtTimestamp(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var request = new UserCreateRequestModel
        {
            Email = "newuser@example.com",
            UserName = "new_user",
            Password = "StrongPassword123!",
            UserRoleIds = [],
        };

        // Act
        var result = await repository.CreateUserAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<UserCreateResultSuccessModel>(), "Expected success result");
        var successResult = result.AsT0;

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var createdUser = await dbContext.Users.FirstAsync(u => u.Id == successResult.UserId, cancellationToken);
        Assert.That(createdUser.CreatedAt, Is.EqualTo(utcNow).Within(TimeSpan.FromSeconds(5)));
    }
}
