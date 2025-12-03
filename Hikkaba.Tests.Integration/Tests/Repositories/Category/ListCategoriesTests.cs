using System;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Constants;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Hikkaba.Tests.Integration.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Category;

internal sealed class ListCategoriesTests : IntegrationTestBase
{
    #region Basic listing tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListCategories_WhenNoCategories_ReturnsEmptyList(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        var result = await repository.ListCategoriesAsync(new CategoryFilter(), cancellationToken);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListCategories_WhenCategoriesExist_ReturnsAllCategories(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("a", "Anime")
            .WithCategory("b", "Random")
            .WithCategory("c", "Creativity")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        var result = await repository.ListCategoriesAsync(new CategoryFilter(), cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
    }

    #endregion

    #region IncludeHidden filter tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase(true, 2)]
    [TestCase(false, 1)]
    public async Task ListCategories_WhenIncludeHidden_ReturnsExpectedCount(
        bool includeHidden,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("a", "Anime", isHidden: false)
            .WithCategory("b", "Random", isHidden: true)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        var result = await repository.ListCategoriesAsync(new CategoryFilter
        {
            IncludeHidden = includeHidden,
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(expectedCount));
    }

    #endregion

    #region IncludeDeleted filter tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase(true, 2)]
    [TestCase(false, 1)]
    public async Task ListCategories_WhenIncludeDeleted_ReturnsExpectedCount(
        bool includeDeleted,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("a", "Anime", isDeleted: false)
            .WithCategory("b", "Random", isDeleted: true)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        var result = await repository.ListCategoriesAsync(new CategoryFilter
        {
            IncludeDeleted = includeDeleted,
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(expectedCount));
    }

    #endregion

    #region Combined filters tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase(true, true, 4)]
    [TestCase(true, false, 2)]
    [TestCase(false, true, 2)]
    [TestCase(false, false, 1)]
    public async Task ListCategories_WhenCombiningFilters_ReturnsExpectedCount(
        bool includeHidden,
        bool includeDeleted,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("a", "Visible Active", isHidden: false, isDeleted: false)
            .WithCategory("b", "Hidden Active", isHidden: true, isDeleted: false)
            .WithCategory("c", "Visible Deleted", isHidden: false, isDeleted: true)
            .WithCategory("d", "Hidden Deleted", isHidden: true, isDeleted: true)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        var result = await repository.ListCategoriesAsync(new CategoryFilter
        {
            IncludeHidden = includeHidden,
            IncludeDeleted = includeDeleted,
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(expectedCount));
    }

    #endregion

    #region Ordering tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase(nameof(CategoryDetailsModel.Name), OrderByDirection.Asc)]
    [TestCase(nameof(CategoryDetailsModel.Name), OrderByDirection.Desc)]
    [TestCase(nameof(CategoryDetailsModel.Alias), OrderByDirection.Asc)]
    [TestCase(nameof(CategoryDetailsModel.Alias), OrderByDirection.Desc)]
    [TestCase(nameof(CategoryDetailsModel.Id), OrderByDirection.Asc)]
    [TestCase(nameof(CategoryDetailsModel.Id), OrderByDirection.Desc)]
    [TestCase(nameof(CategoryDetailsModel.CreatedAt), OrderByDirection.Asc)]
    [TestCase(nameof(CategoryDetailsModel.CreatedAt), OrderByDirection.Desc)]
    [TestCase(nameof(CategoryDetailsModel.DefaultBumpLimit), OrderByDirection.Asc)]
    [TestCase(nameof(CategoryDetailsModel.DefaultBumpLimit), OrderByDirection.Desc)]
    [TestCase(nameof(CategoryDetailsModel.MaxThreadCount), OrderByDirection.Asc)]
    [TestCase(nameof(CategoryDetailsModel.MaxThreadCount), OrderByDirection.Desc)]
    public async Task ListCategories_WhenOrderByField_ReturnsOrderedResults(
        string fieldName,
        OrderByDirection direction,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("c", "Creativity", defaultBumpLimit: 300)
            .WithCategory("a", "Anime", defaultBumpLimit: 100)
            .WithCategory("b", "Random", defaultBumpLimit: 200)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        var result = await repository.ListCategoriesAsync(new CategoryFilter
        {
            OrderBy = [new OrderByItem { Field = fieldName, Direction = direction }],
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result, Is.OrderedBy(fieldName, direction));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListCategories_WhenOrderByMultipleFields_ReturnsCorrectlyOrderedResults(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("a", "Anime", defaultBumpLimit: 100)
            .WithCategory("b", "Random", defaultBumpLimit: 100)
            .WithCategory("c", "Creativity", defaultBumpLimit: 200)
            .WithCategory("d", "Discussion", defaultBumpLimit: 200)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        var result = await repository.ListCategoriesAsync(new CategoryFilter
        {
            OrderBy =
            [
                new OrderByItem { Field = nameof(CategoryDetailsModel.DefaultBumpLimit), Direction = OrderByDirection.Desc },
                new OrderByItem { Field = nameof(CategoryDetailsModel.Name), Direction = OrderByDirection.Asc },
            ],
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(4));
        Assert.That(
            result,
            Is.Ordered
                .Descending.By(nameof(CategoryDetailsModel.DefaultBumpLimit))
                .Then.Ascending.By(nameof(CategoryDetailsModel.Name)));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListCategories_WhenNoOrderBySpecified_ReturnsOrderedByName(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("c", "Creativity")
            .WithCategory("a", "Anime")
            .WithCategory("b", "Random")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        var result = await repository.ListCategoriesAsync(new CategoryFilter(), cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result, Is.Ordered.Ascending.By(nameof(CategoryDetailsModel.Name)));
    }

    #endregion

    #region Category properties tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListCategories_WhenCategoryHasAllProperties_ReturnsCorrectModel(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory(
                "test",
                "Test Category",
                isDeleted: false,
                isHidden: false,
                defaultBumpLimit: 750,
                showThreadLocalUserHash: true)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        var result = await repository.ListCategoriesAsync(new CategoryFilter(), cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var category = result[0];
        Assert.That(category.Alias, Is.EqualTo("test"));
        Assert.That(category.Name, Is.EqualTo("Test Category"));
        Assert.That(category.IsDeleted, Is.False);
        Assert.That(category.IsHidden, Is.False);
        Assert.That(category.DefaultBumpLimit, Is.EqualTo(750));
        Assert.That(category.ShowThreadLocalUserHash, Is.True);
        Assert.That(category.CreatedBy, Is.Not.Null);
        Assert.That(category.CreatedBy.UserName, Is.EqualTo(Defaults.AdministratorUserName));
        Assert.That(category.CreatedAt, Is.Not.EqualTo(default(DateTime)));
    }

    #endregion
}
