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
            .WithDefaultAdmin()
            .WithModeratorRole()
            .WithCategory("a", "Anime")
            .WithModerator("mod1")
            .WithUserRole("mod1", Defaults.ModeratorRoleName)
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
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].UserName, Is.EqualTo("mod1"));
        Assert.That(result[0].IsCategoryModerator, Is.True);
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
            .WithDefaultAdmin()
            .WithModeratorRole()
            .WithCategory("a", "Anime")
            .WithCategory("b", "Random")
            .WithModerator("mod1")
            .WithUserRole("mod1", Defaults.ModeratorRoleName)
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
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].UserName, Is.EqualTo("mod1"));
        Assert.That(result[0].IsCategoryModerator, Is.False);
    }

    #endregion

    #region IncludeDeleted tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase(true, 2)]
    [TestCase(false, 1)]
    public async Task ListCategoryModerators_WhenIncludeDeleted_ReturnsExpectedCount(
        bool includeDeleted,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithDefaultAdmin()
            .WithModeratorRole()
            .WithCategory("a", "Anime")
            .WithModerator("active_mod")
            .WithUserRole("active_mod", Defaults.ModeratorRoleName)
            .WithCategoryModerator("a", "active_mod")
            .SaveAsync(cancellationToken);

        // Add deleted moderator via WithUser since WithModerator doesn't support isDeleted
        await builder
            .WithUser("deleted_mod", isDeleted: true)
            .WithUserRole("deleted_mod", Defaults.ModeratorRoleName)
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
            .WithDefaultAdmin()
            .WithModeratorRole()
            .WithCategory("a", "Anime")
            .WithModerator("mod1")
            .WithUserRole("mod1", Defaults.ModeratorRoleName)
            .WithCategoryModerator("a", "mod1")
            .WithModerator("mod2")
            .WithUserRole("mod2", Defaults.ModeratorRoleName)
            .WithCategoryModerator("a", "mod2")
            .WithModerator("mod3")
            .WithUserRole("mod3", Defaults.ModeratorRoleName)
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
        Assert.That(result, Has.Count.EqualTo(3));

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
            .WithDefaultAdmin()
            .WithAdministratorRole()
            .WithModeratorRole()
            .WithCategory("a", "Anime")
            .WithUser("admin_user")
            .WithUserRole("admin_user", Defaults.AdministratorRoleName)
            .WithModerator("mod_user")
            .WithUserRole("mod_user", Defaults.ModeratorRoleName)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.ListCategoryModerators(new CategoryModeratorFilter
        {
            IncludeDeleted = false,
            CategoryAlias = "a",
        }, cancellationToken);

        // Assert
        // Includes: admin_user (administrator), mod_user (moderator) = 2 users
        // Note: WithDefaultAdmin() doesn't add user to any role, so default admin is not included
        Assert.That(result, Has.Count.EqualTo(2));
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
            .WithDefaultAdmin()
            .WithModeratorRole()
            .WithCategory("a", "Anime")
            .WithUser("detailed_mod", email: "mod@example.com", lastLoginAt: utcNow.AddDays(-1))
            .WithUserRole("detailed_mod", Defaults.ModeratorRoleName)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await repository.ListCategoryModerators(new CategoryModeratorFilter
        {
            IncludeDeleted = false,
            CategoryAlias = "a",
        }, cancellationToken);

        // Assert
        // Should have at least 2: admin and detailed_mod
        Assert.That(result, Has.Count.GreaterThanOrEqualTo(1));
        var mod = result.Single(m => m.UserName == "detailed_mod");
        Assert.That(mod.Email, Is.EqualTo("mod@example.com"));
        Assert.That(mod.LastLogin, Is.EqualTo(utcNow.AddDays(-1)).Within(TimeSpan.FromSeconds(1)));
    }

    #endregion

    #region Empty result tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListCategoryModerators_WhenNoModeratorsExist_ReturnsEmptyList(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithDefaultAdmin()
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
        Assert.That(result, Is.Empty);
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
            .WithDefaultAdmin()
            .WithModeratorRole()
            .WithCategory("a", "Anime")
            .WithCategory("b", "Random")
            .WithModerator("multi_mod")
            .WithUserRole("multi_mod", Defaults.ModeratorRoleName)
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
        Assert.That(resultA, Has.Count.EqualTo(1));
        Assert.That(resultA[0].IsCategoryModerator, Is.True);

        Assert.That(resultB, Has.Count.EqualTo(1));
        Assert.That(resultB[0].IsCategoryModerator, Is.True);
    }

    #endregion
}
