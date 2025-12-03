using System;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Category;

internal sealed class GetCategoryTests : IntegrationTestBase
{
    #region Basic get tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetCategory_WhenCategoryExists_ReturnsCategory(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("anime", "Anime Discussion")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        var result = await repository.GetCategoryAsync("anime", includeDeleted: false, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Alias, Is.EqualTo("anime"));
        Assert.That(result.Name, Is.EqualTo("Anime Discussion"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetCategory_WhenCategoryDoesNotExist_ReturnsNull(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("anime", "Anime Discussion")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        var result = await repository.GetCategoryAsync("nonexistent", includeDeleted: false, cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    #endregion

    #region IncludeDeleted tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetCategory_WhenDeletedAndIncludeDeletedFalse_ReturnsNull(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("anime", "Anime Discussion", isDeleted: true)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        var result = await repository.GetCategoryAsync("anime", includeDeleted: false, cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetCategory_WhenDeletedAndIncludeDeletedTrue_ReturnsCategory(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("anime", "Anime Discussion", isDeleted: true)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        var result = await repository.GetCategoryAsync("anime", includeDeleted: true, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Alias, Is.EqualTo("anime"));
        Assert.That(result.IsDeleted, Is.True);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetCategory_WhenNotDeletedAndIncludeDeletedTrue_ReturnsCategory(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("anime", "Anime Discussion", isDeleted: false)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        var result = await repository.GetCategoryAsync("anime", includeDeleted: true, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsDeleted, Is.False);
    }

    #endregion

    #region Category properties tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetCategory_WhenCategoryExists_ReturnsAllProperties(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory(
                "test",
                "Test Category",
                isDeleted: false,
                isHidden: true,
                defaultBumpLimit: 750,
                showThreadLocalUserHash: true)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        var result = await repository.GetCategoryAsync("test", includeDeleted: false, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Alias, Is.EqualTo("test"));
        Assert.That(result.Name, Is.EqualTo("Test Category"));
        Assert.That(result.IsDeleted, Is.False);
        Assert.That(result.IsHidden, Is.True);
        Assert.That(result.DefaultBumpLimit, Is.EqualTo(750));
        Assert.That(result.ShowThreadLocalUserHash, Is.True);
        Assert.That(result.CreatedBy, Is.Not.Null);
        Assert.That(result.CreatedBy.UserName, Is.EqualTo("admin"));
        Assert.That(result.CreatedAt, Is.Not.EqualTo(default(DateTime)));
        Assert.That(result.ModifiedBy, Is.Null);
        Assert.That(result.ModifiedAt, Is.Null);
    }

    #endregion

    #region Multiple categories tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetCategory_WhenMultipleCategoriesExist_ReturnsCorrectCategory(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("anime", "Anime Discussion")
            .WithCategory("random", "Random Board")
            .WithCategory("tech", "Technology")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        // Act
        var result = await repository.GetCategoryAsync("random", includeDeleted: false, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Alias, Is.EqualTo("random"));
        Assert.That(result.Name, Is.EqualTo("Random Board"));
    }

    #endregion
}
