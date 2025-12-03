using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Constants;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Hikkaba.Tests.Integration.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Category;

internal sealed class SetCategoryDeletedTests : IntegrationTestBase
{
    #region Delete tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetCategoryDeleted_WhenSetToTrue_DeletesCategory(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("anime", "Anime Discussion", isDeleted: false);
        await builder.SaveAsync(cancellationToken);

        UserContextUtils.SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        await repository.SetCategoryDeletedAsync("anime", isDeleted: true, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deletedCategory = await dbContext.Categories
            .IgnoreQueryFilters()
            .FirstAsync(c => c.Alias == "anime", cancellationToken);
        Assert.That(deletedCategory.IsDeleted, Is.True);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetCategoryDeleted_WhenSetToFalse_RestoresCategory(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("anime", "Anime Discussion", isDeleted: true);
        await builder.SaveAsync(cancellationToken);

        UserContextUtils.SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        await repository.SetCategoryDeletedAsync("anime", isDeleted: false, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var restoredCategory = await dbContext.Categories.FirstAsync(c => c.Alias == "anime", cancellationToken);
        Assert.That(restoredCategory.IsDeleted, Is.False);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetCategoryDeleted_WhenDeleted_SetsModifiedByAndModifiedAt(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("anime", "Anime Discussion", isDeleted: false);
        await builder.SaveAsync(cancellationToken);

        UserContextUtils.SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var originalCreatedAt = builder.LastCategory.CreatedAt;

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        await repository.SetCategoryDeletedAsync("anime", isDeleted: true, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deletedCategory = await dbContext.Categories
            .IgnoreQueryFilters()
            .Include(c => c.ModifiedBy)
            .FirstAsync(c => c.Alias == "anime", cancellationToken);

        Assert.That(deletedCategory.ModifiedById, Is.EqualTo(builder.Admin.Id));
        Assert.That(deletedCategory.ModifiedBy, Is.Not.Null);
        Assert.That(deletedCategory.ModifiedBy!.UserName, Is.EqualTo(Defaults.AdministratorUserName));
        Assert.That(deletedCategory.ModifiedAt, Is.Not.Null);
        Assert.That(deletedCategory.ModifiedAt, Is.GreaterThanOrEqualTo(originalCreatedAt));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetCategoryDeleted_WhenRestored_UpdatesModifiedByAndModifiedAt(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("anime", "Anime Discussion", isDeleted: true);
        await builder.SaveAsync(cancellationToken);

        UserContextUtils.SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var originalCreatedAt = builder.LastCategory.CreatedAt;

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        await repository.SetCategoryDeletedAsync("anime", isDeleted: false, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var restoredCategory = await dbContext.Categories
            .Include(c => c.ModifiedBy)
            .FirstAsync(c => c.Alias == "anime", cancellationToken);

        Assert.That(restoredCategory.ModifiedById, Is.EqualTo(builder.Admin.Id));
        Assert.That(restoredCategory.ModifiedBy, Is.Not.Null);
        Assert.That(restoredCategory.ModifiedAt, Is.Not.Null);
        Assert.That(restoredCategory.ModifiedAt, Is.GreaterThanOrEqualTo(originalCreatedAt));
    }

    #endregion

    #region Multiple categories tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetCategoryDeleted_WhenMultipleCategoriesExist_DeletesOnlySpecifiedCategory(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("anime", "Anime Discussion", isDeleted: false)
            .WithCategory("random", "Random Board", isDeleted: false)
            .WithCategory("tech", "Technology", isDeleted: false);
        await builder.SaveAsync(cancellationToken);

        UserContextUtils.SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        await repository.SetCategoryDeletedAsync("random", isDeleted: true, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var animeCategory = await dbContext.Categories.FirstAsync(c => c.Alias == "anime", cancellationToken);
        var randomCategory = await dbContext.Categories
            .IgnoreQueryFilters()
            .FirstAsync(c => c.Alias == "random", cancellationToken);
        var techCategory = await dbContext.Categories.FirstAsync(c => c.Alias == "tech", cancellationToken);

        Assert.That(animeCategory.IsDeleted, Is.False);
        Assert.That(randomCategory.IsDeleted, Is.True);
        Assert.That(techCategory.IsDeleted, Is.False);
    }

    #endregion

    #region Idempotency tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetCategoryDeleted_WhenAlreadyDeleted_RemainsDeleted(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("anime", "Anime Discussion", isDeleted: true);
        await builder.SaveAsync(cancellationToken);

        UserContextUtils.SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        await repository.SetCategoryDeletedAsync("anime", isDeleted: true, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = await dbContext.Categories
            .IgnoreQueryFilters()
            .FirstAsync(c => c.Alias == "anime", cancellationToken);
        Assert.That(category.IsDeleted, Is.True);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetCategoryDeleted_WhenAlreadyNotDeleted_RemainsNotDeleted(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("anime", "Anime Discussion", isDeleted: false);
        await builder.SaveAsync(cancellationToken);

        UserContextUtils.SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        await repository.SetCategoryDeletedAsync("anime", isDeleted: false, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = await dbContext.Categories.FirstAsync(c => c.Alias == "anime", cancellationToken);
        Assert.That(category.IsDeleted, Is.False);
    }

    #endregion
}
