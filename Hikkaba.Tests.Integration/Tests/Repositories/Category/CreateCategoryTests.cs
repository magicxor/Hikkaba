using System;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Constants;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Hikkaba.Tests.Integration.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Category;

internal sealed class CreateCategoryTests : IntegrationTestBase
{
    #region Basic create tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateCategory_WhenValidRequest_CreatesCategorySuccessfully(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true);
        await builder.SaveAsync(cancellationToken);

        UserContextUtils.SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();
        var request = new CategoryCreateRequestModel
        {
            Alias = "anime",
            Name = "Anime Discussion",
            IsHidden = false,
            DefaultBumpLimit = 500,
            ShowThreadLocalUserHash = false,
            ShowCountry = true,
            ShowOs = false,
            ShowBrowser = false,
            MaxThreadCount = 100,
        };

        // Act
        var categoryId = await repository.CreateCategoryAsync(request, cancellationToken);

        // Assert
        Assert.That(categoryId, Is.GreaterThan(0));

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var createdCategory = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);
        Assert.That(createdCategory, Is.Not.Null);
        Assert.That(createdCategory!.Alias, Is.EqualTo("anime"));
        Assert.That(createdCategory.Name, Is.EqualTo("Anime Discussion"));
        Assert.That(createdCategory.IsHidden, Is.False);
        Assert.That(createdCategory.DefaultBumpLimit, Is.EqualTo(500));
        Assert.That(createdCategory.ShowThreadLocalUserHash, Is.False);
        Assert.That(createdCategory.ShowCountry, Is.True);
        Assert.That(createdCategory.ShowOs, Is.False);
        Assert.That(createdCategory.ShowBrowser, Is.False);
        Assert.That(createdCategory.MaxThreadCount, Is.EqualTo(100));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateCategory_WhenCreated_SetsCreatedByAndCreatedAt(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true);
        await builder.SaveAsync(cancellationToken);

        UserContextUtils.SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();
        var request = new CategoryCreateRequestModel
        {
            Alias = "test",
            Name = "Test Category",
            IsHidden = false,
            DefaultBumpLimit = 500,
            ShowThreadLocalUserHash = false,
            ShowCountry = false,
            ShowOs = false,
            ShowBrowser = false,
            MaxThreadCount = 100,
        };

        // Act
        var categoryId = await repository.CreateCategoryAsync(request, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var createdCategory = await dbContext.Categories
            .Include(c => c.CreatedBy)
            .FirstAsync(c => c.Id == categoryId, cancellationToken);

        Assert.That(createdCategory.CreatedById, Is.EqualTo(builder.Admin.Id));
        Assert.That(createdCategory.CreatedBy.UserName, Is.EqualTo(Defaults.AdministratorUserName));
        Assert.That(createdCategory.CreatedAt, Is.Not.EqualTo(default(DateTime)));
        Assert.That(createdCategory.ModifiedById, Is.Null);
        Assert.That(createdCategory.ModifiedAt, Is.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateCategory_WhenHidden_SetsIsHiddenTrue(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true);
        await builder.SaveAsync(cancellationToken);

        UserContextUtils.SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();
        var request = new CategoryCreateRequestModel
        {
            Alias = "hidden",
            Name = "Hidden Category",
            IsHidden = true,
            DefaultBumpLimit = 500,
            ShowThreadLocalUserHash = false,
            ShowCountry = false,
            ShowOs = false,
            ShowBrowser = false,
            MaxThreadCount = 100,
        };

        // Act
        var categoryId = await repository.CreateCategoryAsync(request, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var createdCategory = await dbContext.Categories.FirstAsync(c => c.Id == categoryId, cancellationToken);
        Assert.That(createdCategory.IsHidden, Is.True);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateCategory_WithAllDisplayOptions_SetsAllOptionsCorrectly(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true);
        await builder.SaveAsync(cancellationToken);

        UserContextUtils.SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();
        var request = new CategoryCreateRequestModel
        {
            Alias = "full",
            Name = "Full Options Category",
            IsHidden = false,
            DefaultBumpLimit = 750,
            ShowThreadLocalUserHash = true,
            ShowCountry = true,
            ShowOs = true,
            ShowBrowser = true,
            MaxThreadCount = 200,
        };

        // Act
        var categoryId = await repository.CreateCategoryAsync(request, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var createdCategory = await dbContext.Categories.FirstAsync(c => c.Id == categoryId, cancellationToken);
        Assert.That(createdCategory.DefaultBumpLimit, Is.EqualTo(750));
        Assert.That(createdCategory.ShowThreadLocalUserHash, Is.True);
        Assert.That(createdCategory.ShowCountry, Is.True);
        Assert.That(createdCategory.ShowOs, Is.True);
        Assert.That(createdCategory.ShowBrowser, Is.True);
        Assert.That(createdCategory.MaxThreadCount, Is.EqualTo(200));
    }

    #endregion

    #region Multiple categories tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateCategory_WhenMultipleCategoriesCreated_AllHaveUniqueIds(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true);
        await builder.SaveAsync(cancellationToken);

        UserContextUtils.SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        var categoryId1 = await repository.CreateCategoryAsync(new CategoryCreateRequestModel
        {
            Alias = "cat1",
            Name = "Category 1",
            IsHidden = false,
            DefaultBumpLimit = 500,
            ShowThreadLocalUserHash = false,
            ShowCountry = false,
            ShowOs = false,
            ShowBrowser = false,
            MaxThreadCount = 100,
        }, cancellationToken);

        var categoryId2 = await repository.CreateCategoryAsync(new CategoryCreateRequestModel
        {
            Alias = "cat2",
            Name = "Category 2",
            IsHidden = false,
            DefaultBumpLimit = 500,
            ShowThreadLocalUserHash = false,
            ShowCountry = false,
            ShowOs = false,
            ShowBrowser = false,
            MaxThreadCount = 100,
        }, cancellationToken);

        var categoryId3 = await repository.CreateCategoryAsync(new CategoryCreateRequestModel
        {
            Alias = "cat3",
            Name = "Category 3",
            IsHidden = false,
            DefaultBumpLimit = 500,
            ShowThreadLocalUserHash = false,
            ShowCountry = false,
            ShowOs = false,
            ShowBrowser = false,
            MaxThreadCount = 100,
        }, cancellationToken);

        // Assert
        Assert.That(categoryId1, Is.Not.EqualTo(categoryId2));
        Assert.That(categoryId2, Is.Not.EqualTo(categoryId3));
        Assert.That(categoryId1, Is.Not.EqualTo(categoryId3));
    }

    #endregion
}
