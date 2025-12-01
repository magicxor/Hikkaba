using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Post;

internal sealed class ListThreadPostsTests : IntegrationTestBase
{
    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPosts_WhenThreadHasPosts_ReturnsAllNonDeletedPosts(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new PostTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("First post", "127.0.0.1", "Firefox", isOriginalPost: true)
            .WithPost("Second post", "127.0.0.2", "Chrome")
            .WithPost("Deleted post", "127.0.0.3", "Safari", isDeleted: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act
        var result = await repository.ListThreadPostsAsync(new ThreadPostsFilter
        {
            ThreadId = builder.Thread.Id,
            IncludeDeleted = false,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Asc }],
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].MessageHtml, Is.EqualTo("First post"));
        Assert.That(result[1].MessageHtml, Is.EqualTo("Second post"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPosts_WithIncludeDeleted_ReturnsAllPosts(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new PostTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("First post", "127.0.0.1", "Firefox", isOriginalPost: true)
            .WithPost("Deleted post", "127.0.0.2", "Chrome", isDeleted: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act
        var result = await repository.ListThreadPostsAsync(new ThreadPostsFilter
        {
            ThreadId = builder.Thread.Id,
            IncludeDeleted = true,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Asc }],
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPosts_WithPostIdFilter_ReturnsSinglePost(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new PostTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("First post", "127.0.0.1", "Firefox", isOriginalPost: true)
            .WithPost("Second post", "127.0.0.2", "Chrome");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IPostRepository>();
        var targetPostId = builder.LastPostId;

        // Act
        var result = await repository.ListThreadPostsAsync(new ThreadPostsFilter
        {
            ThreadId = builder.Thread.Id,
            PostId = targetPostId,
            IncludeDeleted = false,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Asc }],
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].MessageHtml, Is.EqualTo("Second post"));
        Assert.That(result[0].Id, Is.EqualTo(targetPostId));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPosts_WhenThreadIsDeleted_WithoutIncludeDeleted_ReturnsEmpty(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new PostTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Deleted thread", isDeleted: true)
            .WithPost("Post in deleted thread", "127.0.0.1", "Firefox", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act
        var result = await repository.ListThreadPostsAsync(new ThreadPostsFilter
        {
            ThreadId = builder.Thread.Id,
            IncludeDeleted = false,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Asc }],
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(0));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPosts_WhenCategoryIsDeleted_WithoutIncludeDeleted_ReturnsEmpty(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new PostTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Deleted category", isDeleted: true)
            .WithThread("Thread in deleted category")
            .WithPost("Post in deleted category", "127.0.0.1", "Firefox", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act
        var result = await repository.ListThreadPostsAsync(new ThreadPostsFilter
        {
            ThreadId = builder.Thread.Id,
            IncludeDeleted = false,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Asc }],
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(0));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPosts_OrderByDescending_ReturnsPostsInDescendingOrder(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new PostTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("First post", "127.0.0.1", "Firefox", isOriginalPost: true)
            .WithPost("Second post", "127.0.0.2", "Chrome")
            .WithPost("Third post", "127.0.0.3", "Safari");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act - order by Id descending (since CreatedAt may be the same for all posts in test)
        var result = await repository.ListThreadPostsAsync(new ThreadPostsFilter
        {
            ThreadId = builder.Thread.Id,
            IncludeDeleted = false,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.Id), Direction = OrderByDirection.Desc }],
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result[0].MessageHtml, Is.EqualTo("Third post"));
        Assert.That(result[2].MessageHtml, Is.EqualTo("First post"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPosts_ReturnsPostsWithReplies(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new PostTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Original post", "127.0.0.1", "Firefox", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        // Add a reply that mentions the original post
        builder.WithPostReplyingTo("Reply to original", "127.0.0.2", "Chrome", [builder.LastPostId]);
        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act
        var result = await repository.ListThreadPostsAsync(new ThreadPostsFilter
        {
            ThreadId = builder.Thread.Id,
            IncludeDeleted = false,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Asc }],
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        var originalPost = result.First(p => p.MessageHtml == "Original post");
        Assert.That(originalPost.Replies, Has.Count.EqualTo(1));
    }
}
