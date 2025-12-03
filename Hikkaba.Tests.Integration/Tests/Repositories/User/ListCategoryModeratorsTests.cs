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

internal sealed class ListCategoryModeratorsTests : IntegrationTestBase
{
    #region Basic tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListCategoryModerators_WhenCategoryHasModerators_ReturnsModeratorsWithFlag(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("a", "Anime")
            .WithUser("mod1", isModerator: true)
            .WithCategoryModerator("a", "mod1")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.ListCategoryModerators(new CategoryModeratorFilter
        {
            IncludeDeleted = false,
            CategoryAlias = "a",
        }, cancellationToken);

        // Assert
        // Includes: admin (administrator via isAdmin: true), mod1 (moderator) = 2 users
        Assert.That(result, Has.Count.EqualTo(2));

        var admin = result.Single(m => m.UserName == Defaults.AdministratorUserName);
        var mod1 = result.Single(m => m.UserName == "mod1");

        Assert.That(admin.IsCategoryModerator, Is.False);
        Assert.That(mod1.IsCategoryModerator, Is.True);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListCategoryModerators_WhenModeratorNotInCategory_ReturnsWithFlagFalse(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("a", "Anime")
            .WithCategory("b", "Random")
            .WithUser("mod1", isModerator: true)
            .WithCategoryModerator("a", "mod1") // mod1 is moderator of category 'a'
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.ListCategoryModerators(new CategoryModeratorFilter
        {
            IncludeDeleted = false,
            CategoryAlias = "b", // Asking about category 'b'
        }, cancellationToken);

        // Assert
        // Includes: admin (administrator), mod1 (moderator in category 'a') = 2 users
        Assert.That(result, Has.Count.EqualTo(2));

        var admin = result.Single(m => m.UserName == Defaults.AdministratorUserName);
        var mod1 = result.Single(m => m.UserName == "mod1");

        Assert.That(admin.IsCategoryModerator, Is.False);
        Assert.That(mod1.IsCategoryModerator, Is.False); // mod1 is not moderator of category 'b'
    }

    #endregion

    #region IncludeDeleted tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase(true, 3)] // admin + active_mod + deleted_mod
    [TestCase(false, 2)] // admin + active_mod
    public async Task ListCategoryModerators_WhenIncludeDeleted_ReturnsExpectedCount(
        bool includeDeleted,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("a", "Anime")
            .WithUser("active_mod", isModerator: true)
            .WithCategoryModerator("a", "active_mod")
            .WithUser("deleted_mod", isDeleted: true, isModerator: true)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.ListCategoryModerators(new CategoryModeratorFilter
        {
            IncludeDeleted = includeDeleted,
            CategoryAlias = "a",
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(expectedCount));
    }

    #endregion

    #region Multiple moderators tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListCategoryModerators_WhenMultipleModerators_ReturnsAllWithCorrectFlags(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("a", "Anime")
            .WithUser("mod1", isModerator: true)
            .WithCategoryModerator("a", "mod1")
            .WithUser("mod2", isModerator: true)
            .WithCategoryModerator("a", "mod2")
            .WithUser("mod3", isModerator: true)
            // mod3 is not assigned to category 'a'
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.ListCategoryModerators(new CategoryModeratorFilter
        {
            IncludeDeleted = false,
            CategoryAlias = "a",
        }, cancellationToken);

        // Assert
        // Includes: admin (administrator), mod1, mod2, mod3 = 4 users
        Assert.That(result, Has.Count.EqualTo(4));

        var admin = result.Single(m => m.UserName == Defaults.AdministratorUserName);
        Assert.That(admin.IsCategoryModerator, Is.False); // admin is not a category moderator

        var mod1 = result.Single(m => m.UserName == "mod1");
        Assert.That(mod1.IsCategoryModerator, Is.True);

        var mod2 = result.Single(m => m.UserName == "mod2");
        Assert.That(mod2.IsCategoryModerator, Is.True);

        var mod3 = result.Single(m => m.UserName == "mod3");
        Assert.That(mod3.IsCategoryModerator, Is.False);
    }

    #endregion

    #region Administrator tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListCategoryModerators_IncludesAdministrators(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("a", "Anime")
            .WithUser("admin_user", isAdmin: true)
            .WithUser("mod_user", isModerator: true)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.ListCategoryModerators(new CategoryModeratorFilter
        {
            IncludeDeleted = false,
            CategoryAlias = "a",
        }, cancellationToken);

        // Assert
        // Includes: admin (via isAdmin: true), admin_user (administrator), mod_user (moderator) = 3 users
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result.Select(m => m.UserName), Contains.Item(Defaults.AdministratorUserName));
        Assert.That(result.Select(m => m.UserName), Contains.Item("admin_user"));
        Assert.That(result.Select(m => m.UserName), Contains.Item("mod_user"));
    }

    #endregion

    #region User details tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListCategoryModerators_ReturnsCorrectUserDetails(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);
        var utcNow = builder.TimeProvider.GetUtcNow().UtcDateTime;

        await builder
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("a", "Anime")
            .WithUser("detailed_mod", email: "mod@example.com", lastLoginAt: utcNow.AddDays(-1), isModerator: true)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.ListCategoryModerators(new CategoryModeratorFilter
        {
            IncludeDeleted = false,
            CategoryAlias = "a",
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2)); // admin + detailed_mod

        var mod = result.Single(m => m.UserName == "detailed_mod");

        Assert.That(mod.Email, Is.EqualTo("mod@example.com"));
        Assert.That(mod.LastLogin, Is.EqualTo(utcNow.AddDays(-1)).Within(TimeSpan.FromSeconds(1)));
    }

    #endregion

    #region Empty result tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListCategoryModerators_WhenNoModeratorsExist_ReturnsOnlyAdmin(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("a", "Anime")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.ListCategoryModerators(new CategoryModeratorFilter
        {
            IncludeDeleted = false,
            CategoryAlias = "a",
        }, cancellationToken);

        // Assert
        // Admin has administrator role (via isAdmin: true), so they appear in the list
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].UserName, Is.EqualTo(Defaults.AdministratorUserName));
    }

    #endregion

    #region Cross-category tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListCategoryModerators_WhenModeratorInMultipleCategories_ReturnsCorrectFlagPerCategory(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("a", "Anime")
            .WithCategory("b", "Random")
            .WithUser("multi_mod", isModerator: true)
            .WithCategoryModerator("a", "multi_mod")
            .WithCategoryModerator("b", "multi_mod")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var resultA = await repository.ListCategoryModerators(new CategoryModeratorFilter
        {
            IncludeDeleted = false,
            CategoryAlias = "a",
        }, cancellationToken);

        var resultB = await repository.ListCategoryModerators(new CategoryModeratorFilter
        {
            IncludeDeleted = false,
            CategoryAlias = "b",
        }, cancellationToken);

        // Assert
        // Includes admin (administrator) + multi_mod = 2 users per category
        Assert.That(resultA, Has.Count.EqualTo(2));
        var multiModA = resultA.Single(m => m.UserName == "multi_mod");
        Assert.That(multiModA.IsCategoryModerator, Is.True);

        Assert.That(resultB, Has.Count.EqualTo(2));
        var multiModB = resultB.Single(m => m.UserName == "multi_mod");
        Assert.That(multiModB.IsCategoryModerator, Is.True);
    }

    #endregion
}
