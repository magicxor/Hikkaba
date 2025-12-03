using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Hikkaba.Tests.Integration.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Category;

internal sealed class EditCategoryTests : IntegrationTestBase
{
    #region Basic edit tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditCategory_WhenValidRequest_UpdatesCategorySuccessfully(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("anime", "Anime Discussion", defaultBumpLimit: 500);
        await builder.SaveAsync(cancellationToken);

        UserContextUtils.SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();
        var request = new CategoryEditRequestModel
        {
            Id = builder.LastCategory.Id,
            Alias = "animeedit",
            Name = "Anime Discussion Updated",
            IsHidden = true,
            DefaultBumpLimit = 750,
            ShowThreadLocalUserHash = true,
            ShowCountry = true,
            ShowOs = true,
            ShowBrowser = true,
            MaxThreadCount = 200,
        };

        // Act
        await repository.EditCategoryAsync(request, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedCategory = await dbContext.Categories.FirstAsync(c => c.Id == builder.LastCategory.Id, cancellationToken);
        Assert.That(updatedCategory.Alias, Is.EqualTo("animeedit"));
        Assert.That(updatedCategory.Name, Is.EqualTo("Anime Discussion Updated"));
        Assert.That(updatedCategory.IsHidden, Is.True);
        Assert.That(updatedCategory.DefaultBumpLimit, Is.EqualTo(750));
        Assert.That(updatedCategory.ShowThreadLocalUserHash, Is.True);
        Assert.That(updatedCategory.ShowCountry, Is.True);
        Assert.That(updatedCategory.ShowOs, Is.True);
        Assert.That(updatedCategory.ShowBrowser, Is.True);
        Assert.That(updatedCategory.MaxThreadCount, Is.EqualTo(200));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditCategory_WhenEdited_SetsModifiedByAndModifiedAt(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("anime", "Anime Discussion");
        await builder.SaveAsync(cancellationToken);

        UserContextUtils.SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var originalCreatedAt = builder.LastCategory.CreatedAt;

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();
        var request = new CategoryEditRequestModel
        {
            Id = builder.LastCategory.Id,
            Alias = "anime",
            Name = "Anime Discussion Updated",
            IsHidden = false,
            DefaultBumpLimit = 500,
            ShowThreadLocalUserHash = false,
            ShowCountry = false,
            ShowOs = false,
            ShowBrowser = false,
            MaxThreadCount = 100,
        };

        // Act
        await repository.EditCategoryAsync(request, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedCategory = await dbContext.Categories
            .Include(c => c.ModifiedBy)
            .FirstAsync(c => c.Id == builder.LastCategory.Id, cancellationToken);

        Assert.That(updatedCategory.ModifiedById, Is.EqualTo(builder.Admin.Id));
        Assert.That(updatedCategory.ModifiedBy, Is.Not.Null);
        Assert.That(updatedCategory.ModifiedBy!.UserName, Is.EqualTo("admin"));
        Assert.That(updatedCategory.ModifiedAt, Is.Not.Null);
        Assert.That(updatedCategory.ModifiedAt, Is.GreaterThanOrEqualTo(originalCreatedAt));
        Assert.That(updatedCategory.CreatedAt, Is.EqualTo(originalCreatedAt));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditCategory_WhenOnlyNameChanged_UpdatesOnlyName(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("anime", "Anime Discussion", defaultBumpLimit: 500);
        await builder.SaveAsync(cancellationToken);

        UserContextUtils.SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();
        var request = new CategoryEditRequestModel
        {
            Id = builder.LastCategory.Id,
            Alias = "anime",
            Name = "Anime & Manga",
            IsHidden = false,
            DefaultBumpLimit = 500,
            ShowThreadLocalUserHash = false,
            ShowCountry = false,
            ShowOs = false,
            ShowBrowser = false,
            MaxThreadCount = 100,
        };

        // Act
        await repository.EditCategoryAsync(request, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedCategory = await dbContext.Categories.FirstAsync(c => c.Id == builder.LastCategory.Id, cancellationToken);
        Assert.That(updatedCategory.Alias, Is.EqualTo("anime"));
        Assert.That(updatedCategory.Name, Is.EqualTo("Anime & Manga"));
        Assert.That(updatedCategory.DefaultBumpLimit, Is.EqualTo(500));
    }

    #endregion

    #region Display options tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditCategory_WhenToggleDisplayOptions_UpdatesCorrectly(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("test", "Test Category", showThreadLocalUserHash: false);
        await builder.SaveAsync(cancellationToken);

        UserContextUtils.SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // First update - enable all display options
        await repository.EditCategoryAsync(new CategoryEditRequestModel
        {
            Id = builder.LastCategory.Id,
            Alias = "test",
            Name = "Test Category",
            IsHidden = false,
            DefaultBumpLimit = 500,
            ShowThreadLocalUserHash = true,
            ShowCountry = true,
            ShowOs = true,
            ShowBrowser = true,
            MaxThreadCount = 100,
        }, cancellationToken);

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var afterFirstUpdate = await dbContext.Categories.FirstAsync(c => c.Id == builder.LastCategory.Id, cancellationToken);

        Assert.That(afterFirstUpdate.ShowThreadLocalUserHash, Is.True);
        Assert.That(afterFirstUpdate.ShowCountry, Is.True);
        Assert.That(afterFirstUpdate.ShowOs, Is.True);
        Assert.That(afterFirstUpdate.ShowBrowser, Is.True);

        // Second update - disable all display options
        await repository.EditCategoryAsync(new CategoryEditRequestModel
        {
            Id = builder.LastCategory.Id,
            Alias = "test",
            Name = "Test Category",
            IsHidden = false,
            DefaultBumpLimit = 500,
            ShowThreadLocalUserHash = false,
            ShowCountry = false,
            ShowOs = false,
            ShowBrowser = false,
            MaxThreadCount = 100,
        }, cancellationToken);

        dbContext.ChangeTracker.Clear();
        var afterSecondUpdate = await dbContext.Categories.FirstAsync(c => c.Id == builder.LastCategory.Id, cancellationToken);

        Assert.That(afterSecondUpdate.ShowThreadLocalUserHash, Is.False);
        Assert.That(afterSecondUpdate.ShowCountry, Is.False);
        Assert.That(afterSecondUpdate.ShowOs, Is.False);
        Assert.That(afterSecondUpdate.ShowBrowser, Is.False);
    }

    #endregion

    #region Visibility tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    public async Task EditCategory_WhenToggleHidden_UpdatesCorrectly(
        bool initialHidden,
        bool finalHidden,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("test", "Test Category", isHidden: initialHidden);
        await builder.SaveAsync(cancellationToken);

        UserContextUtils.SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        await repository.EditCategoryAsync(new CategoryEditRequestModel
        {
            Id = builder.LastCategory.Id,
            Alias = "test",
            Name = "Test Category",
            IsHidden = finalHidden,
            DefaultBumpLimit = 500,
            ShowThreadLocalUserHash = false,
            ShowCountry = false,
            ShowOs = false,
            ShowBrowser = false,
            MaxThreadCount = 100,
        }, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedCategory = await dbContext.Categories.FirstAsync(c => c.Id == builder.LastCategory.Id, cancellationToken);
        Assert.That(updatedCategory.IsHidden, Is.EqualTo(finalHidden));
    }

    #endregion
}
