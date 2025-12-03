using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Category;

internal sealed class SetCategoryModeratorsTests : IntegrationTestBase
{
    #region Basic set moderators tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetCategoryModerators_WhenNoModeratorsExist_AddsModerators(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("anime", "Anime Discussion")
            .WithModerator("moderator1")
            .WithModerator("moderator2");
        await builder.SaveAsync(cancellationToken);

        var moderator1 = builder.GetModerator("moderator1");
        var moderator2 = builder.GetModerator("moderator2");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        await repository.SetCategoryModeratorsAsync("anime", [moderator1.Id, moderator2.Id], cancellationToken);

        // Assert
        var category = await dbContext.Categories
            .Include(c => c.Moderators)
            .ThenInclude(m => m.Moderator)
            .FirstAsync(c => c.Alias == "anime", cancellationToken);

        Assert.That(category.Moderators, Has.Count.EqualTo(2));
        Assert.That(category.Moderators.Select(m => m.Moderator.UserName), Contains.Item("moderator1"));
        Assert.That(category.Moderators.Select(m => m.Moderator.UserName), Contains.Item("moderator2"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetCategoryModerators_WhenModeratorsExist_ReplacesModerators(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("anime", "Anime Discussion")
            .WithModerator("moderator1")
            .WithModerator("moderator2")
            .WithModerator("moderator3")
            .WithCategoryModerators("anime", "moderator1", "moderator2");
        await builder.SaveAsync(cancellationToken);

        var moderator2 = builder.GetModerator("moderator2");
        var moderator3 = builder.GetModerator("moderator3");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act - Replace with moderators 2 and 3
        await repository.SetCategoryModeratorsAsync("anime", [moderator2.Id, moderator3.Id], cancellationToken);

        // Assert
        dbContext.ChangeTracker.Clear();
        var category = await dbContext.Categories
            .Include(c => c.Moderators)
            .ThenInclude(m => m.Moderator)
            .FirstAsync(c => c.Alias == "anime", cancellationToken);

        Assert.That(category.Moderators, Has.Count.EqualTo(2));
        Assert.That(category.Moderators.Select(m => m.Moderator.UserName), Does.Not.Contain("moderator1"));
        Assert.That(category.Moderators.Select(m => m.Moderator.UserName), Contains.Item("moderator2"));
        Assert.That(category.Moderators.Select(m => m.Moderator.UserName), Contains.Item("moderator3"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetCategoryModerators_WhenEmptyList_RemovesAllModerators(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("anime", "Anime Discussion")
            .WithModerator("moderator1")
            .WithModerator("moderator2")
            .WithCategoryModerators("anime", "moderator1", "moderator2");
        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act - Remove all moderators
        await repository.SetCategoryModeratorsAsync("anime", [], cancellationToken);

        // Assert
        dbContext.ChangeTracker.Clear();
        var category = await dbContext.Categories
            .Include(c => c.Moderators)
            .FirstAsync(c => c.Alias == "anime", cancellationToken);

        Assert.That(category.Moderators, Is.Empty);
    }

    #endregion

    #region Single moderator tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetCategoryModerators_WhenSingleModerator_AddsSingleModerator(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("anime", "Anime Discussion")
            .WithModerator("moderator1");
        await builder.SaveAsync(cancellationToken);

        var moderator = builder.GetModerator("moderator1");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        await repository.SetCategoryModeratorsAsync("anime", [moderator.Id], cancellationToken);

        // Assert
        var category = await dbContext.Categories
            .Include(c => c.Moderators)
            .ThenInclude(m => m.Moderator)
            .FirstAsync(c => c.Alias == "anime", cancellationToken);

        Assert.That(category.Moderators, Has.Count.EqualTo(1));
        Assert.That(category.Moderators.Single().Moderator.UserName, Is.EqualTo("moderator1"));
    }

    #endregion

    #region Multiple categories tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetCategoryModerators_WhenMultipleCategories_SetsOnlySpecifiedCategory(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("anime", "Anime Discussion")
            .WithCategory("random", "Random Board")
            .WithModerator("anime_mod")
            .WithModerator("random_mod");
        await builder.SaveAsync(cancellationToken);

        var animeMod = builder.GetModerator("anime_mod");
        var randomMod = builder.GetModerator("random_mod");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        await repository.SetCategoryModeratorsAsync("anime", [animeMod.Id], cancellationToken);
        await repository.SetCategoryModeratorsAsync("random", [randomMod.Id], cancellationToken);

        // Assert
        var animeCategory = await dbContext.Categories
            .Include(c => c.Moderators)
            .ThenInclude(m => m.Moderator)
            .FirstAsync(c => c.Alias == "anime", cancellationToken);

        var randomCategory = await dbContext.Categories
            .Include(c => c.Moderators)
            .ThenInclude(m => m.Moderator)
            .FirstAsync(c => c.Alias == "random", cancellationToken);

        Assert.That(animeCategory.Moderators, Has.Count.EqualTo(1));
        Assert.That(animeCategory.Moderators.Single().Moderator.UserName, Is.EqualTo("anime_mod"));

        Assert.That(randomCategory.Moderators, Has.Count.EqualTo(1));
        Assert.That(randomCategory.Moderators.Single().Moderator.UserName, Is.EqualTo("random_mod"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetCategoryModerators_WhenSameModeratorMultipleCategories_WorksCorrectly(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("anime", "Anime Discussion")
            .WithCategory("random", "Random Board")
            .WithModerator("shared_mod");
        await builder.SaveAsync(cancellationToken);

        var sharedModerator = builder.GetModerator("shared_mod");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act - Set same moderator for both categories
        await repository.SetCategoryModeratorsAsync("anime", [sharedModerator.Id], cancellationToken);
        await repository.SetCategoryModeratorsAsync("random", [sharedModerator.Id], cancellationToken);

        // Assert
        var animeCategory = await dbContext.Categories
            .Include(c => c.Moderators)
            .ThenInclude(m => m.Moderator)
            .FirstAsync(c => c.Alias == "anime", cancellationToken);

        var randomCategory = await dbContext.Categories
            .Include(c => c.Moderators)
            .ThenInclude(m => m.Moderator)
            .FirstAsync(c => c.Alias == "random", cancellationToken);

        Assert.That(animeCategory.Moderators, Has.Count.EqualTo(1));
        Assert.That(animeCategory.Moderators.Single().Moderator.UserName, Is.EqualTo("shared_mod"));

        Assert.That(randomCategory.Moderators, Has.Count.EqualTo(1));
        Assert.That(randomCategory.Moderators.Single().Moderator.UserName, Is.EqualTo("shared_mod"));

        // Verify the moderator exists only once in the Users table
        var moderatorCount = await dbContext.Users.CountAsync(u => u.UserName == "shared_mod", cancellationToken);
        Assert.That(moderatorCount, Is.EqualTo(1));

        // But there should be 2 CategoryToModerator records
        var categoryToModeratorCount = await dbContext.Set<CategoryToModerator>()
            .CountAsync(ctm => ctm.ModeratorId == sharedModerator.Id, cancellationToken);
        Assert.That(categoryToModeratorCount, Is.EqualTo(2));
    }

    #endregion

    #region Idempotency tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetCategoryModerators_WhenSetSameModeratorsAgain_RemainsUnchanged(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("anime", "Anime Discussion")
            .WithModerator("moderator1")
            .WithModerator("moderator2");
        await builder.SaveAsync(cancellationToken);

        var moderator1 = builder.GetModerator("moderator1");
        var moderator2 = builder.GetModerator("moderator2");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act - Set moderators twice
        await repository.SetCategoryModeratorsAsync("anime", [moderator1.Id, moderator2.Id], cancellationToken);
        await repository.SetCategoryModeratorsAsync("anime", [moderator1.Id, moderator2.Id], cancellationToken);

        // Assert
        dbContext.ChangeTracker.Clear();
        var category = await dbContext.Categories
            .Include(c => c.Moderators)
            .ThenInclude(m => m.Moderator)
            .FirstAsync(c => c.Alias == "anime", cancellationToken);

        Assert.That(category.Moderators, Has.Count.EqualTo(2));
        Assert.That(category.Moderators.Select(m => m.Moderator.UserName), Contains.Item("moderator1"));
        Assert.That(category.Moderators.Select(m => m.Moderator.UserName), Contains.Item("moderator2"));
    }

    #endregion
}
