using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Constants;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Post;

internal sealed class ListPostsTests : IntegrationTestBase
{
    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListPosts_WhenNoFilters_ReturnsNonDeletedNonHiddenPosts(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("First post", isOriginalPost: true)
            .WithPost("Second post", "127.0.0.2", "Chrome")
            .WithPost("Deleted post", "127.0.0.3", "Safari", isDeleted: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act
        var result = await repository.ListPostsAsync(new PostPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            IncludeHidden = false,
            IncludeDeleted = false,
            IncludeTotalCount = true,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Desc }],
        }, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(2));
        Assert.That(result.TotalItemCount, Is.EqualTo(2));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListPosts_WithIncludeDeleted_ReturnsAllPosts(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("First post", isOriginalPost: true)
            .WithPost("Deleted post", "127.0.0.2", "Chrome", isDeleted: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act
        var result = await repository.ListPostsAsync(new PostPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            IncludeHidden = false,
            IncludeDeleted = true,
            IncludeTotalCount = true,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Desc }],
        }, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(2));
        Assert.That(result.TotalItemCount, Is.EqualTo(2));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListPosts_WithIncludeHidden_ReturnsPostsFromHiddenCategories(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        // Create visible category with post and hidden category with post using the same builder
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random", isHidden: false)
            .WithThread("Visible thread")
            .WithPost("Visible post", isOriginalPost: true)
            .WithCategory("h", "Hidden", isHidden: true)
            .WithThread("Hidden thread")
            .WithPost("Hidden post", "127.0.0.2", "Chrome", isOriginalPost: true);
        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act - without hidden
        var resultWithoutHidden = await repository.ListPostsAsync(new PostPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            IncludeHidden = false,
            IncludeDeleted = false,
            IncludeTotalCount = true,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Desc }],
        }, cancellationToken);

        // Act - with hidden
        var resultWithHidden = await repository.ListPostsAsync(new PostPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            IncludeHidden = true,
            IncludeDeleted = false,
            IncludeTotalCount = true,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Desc }],
        }, cancellationToken);

        // Assert
        Assert.That(resultWithoutHidden.Data, Has.Count.EqualTo(1));
        Assert.That(resultWithoutHidden.Data[0].MessageHtml, Is.EqualTo("Visible post"));

        Assert.That(resultWithHidden.Data, Has.Count.EqualTo(2));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListPosts_WithPaging_ReturnsCorrectPage(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Post 1", isOriginalPost: true)
            .WithPost("Post 2", "127.0.0.2", "Chrome")
            .WithPost("Post 3", "127.0.0.3", "Safari")
            .WithPost("Post 4", "127.0.0.4", "Edge")
            .WithPost("Post 5", "127.0.0.5", "Opera");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act - page 1
        var page1 = await repository.ListPostsAsync(new PostPagingFilter
        {
            PageNumber = 1,
            PageSize = 2,
            IncludeHidden = false,
            IncludeDeleted = false,
            IncludeTotalCount = true,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Asc }],
        }, cancellationToken);

        // Act - page 2
        var page2 = await repository.ListPostsAsync(new PostPagingFilter
        {
            PageNumber = 2,
            PageSize = 2,
            IncludeHidden = false,
            IncludeDeleted = false,
            IncludeTotalCount = true,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Asc }],
        }, cancellationToken);

        // Assert
        Assert.That(page1.Data, Has.Count.EqualTo(2));
        Assert.That(page1.TotalItemCount, Is.EqualTo(5));
        Assert.That(page1.Data[0].MessageHtml, Is.EqualTo("Post 1"));
        Assert.That(page1.Data[1].MessageHtml, Is.EqualTo("Post 2"));

        Assert.That(page2.Data, Has.Count.EqualTo(2));
        Assert.That(page2.Data[0].MessageHtml, Is.EqualTo("Post 3"));
        Assert.That(page2.Data[1].MessageHtml, Is.EqualTo("Post 4"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListPosts_WithoutIncludeTotalCount_DoesNotReturnTotalCount(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("First post", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act
        var result = await repository.ListPostsAsync(new PostPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            IncludeHidden = false,
            IncludeDeleted = false,
            IncludeTotalCount = false,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Desc }],
        }, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(1));
        Assert.That(result.TotalItemCount, Is.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListPosts_WhenThreadIsDeleted_WithoutIncludeDeleted_ExcludesThreadPosts(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random")
            .WithThread("Deleted thread", isDeleted: true)
            .WithPost("Post in deleted thread", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act
        var result = await repository.ListPostsAsync(new PostPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            IncludeHidden = false,
            IncludeDeleted = false,
            IncludeTotalCount = true,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Desc }],
        }, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(0));
        Assert.That(result.TotalItemCount, Is.EqualTo(0));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListPosts_WhenCategoryIsDeleted_WithoutIncludeDeleted_ExcludesCategoryPosts(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Deleted category", isDeleted: true)
            .WithThread("Thread in deleted category")
            .WithPost("Post in deleted category", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act
        var result = await repository.ListPostsAsync(new PostPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            IncludeHidden = false,
            IncludeDeleted = false,
            IncludeTotalCount = true,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Desc }],
        }, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(0));
        Assert.That(result.TotalItemCount, Is.EqualTo(0));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListPosts_ReturnsPostsWithAttachmentsInfo(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Post with attachments", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act
        var result = await repository.ListPostsAsync(new PostPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            IncludeHidden = false,
            IncludeDeleted = false,
            IncludeTotalCount = true,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Desc }],
        }, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(1));
        var post = result.Data[0];

        // Verify attachment collections are present (even if empty)
        Assert.That(post.Audio, Is.Not.Null);
        Assert.That(post.Documents, Is.Not.Null);
        Assert.That(post.Notices, Is.Not.Null);
        Assert.That(post.Pictures, Is.Not.Null);
        Assert.That(post.Video, Is.Not.Null);
    }
}
