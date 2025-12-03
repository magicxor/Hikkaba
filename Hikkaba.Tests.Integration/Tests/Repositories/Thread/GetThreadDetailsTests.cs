using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Thread;

internal sealed class GetThreadDetailsTests : IntegrationTestBase
{
    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetThreadDetails_WhenThreadExists_ReturnsThreadWithPosts(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("OP post", isOriginalPost: true)
            .WithPost("Reply 1")
            .WithPost("Reply 2");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var thread = builder.GetThread("Test thread");

        // Act
        var result = await repository.GetThreadDetailsAsync(thread.Id, includeDeleted: false, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(thread.Id));
        Assert.That(result.Title, Is.EqualTo("Test thread"));
        Assert.That(result.PostCount, Is.EqualTo(3));
        Assert.That(result.Posts, Has.Count.EqualTo(3));
        Assert.That(result.Posts[0].MessageHtml, Is.EqualTo("OP post"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetThreadDetails_WhenThreadDoesNotExist_ReturnsNull(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();

        // Act
        var result = await repository.GetThreadDetailsAsync(999999, includeDeleted: false, cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetThreadDetails_ReturnsCorrectThreadFields(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random", showThreadLocalUserHash: true)
            .WithThread("Pinned cyclic thread", isPinned: true, isClosed: true, isCyclic: true, bumpLimit: 100)
            .WithPost("OP post", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var thread = builder.GetThread("Pinned cyclic thread");

        // Act
        var result = await repository.GetThreadDetailsAsync(thread.Id, includeDeleted: false, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsPinned, Is.True);
        Assert.That(result.IsClosed, Is.True);
        Assert.That(result.IsCyclic, Is.True);
        Assert.That(result.BumpLimit, Is.EqualTo(100));
        Assert.That(result.ShowThreadLocalUserHash, Is.True);
        Assert.That(result.CategoryAlias, Is.EqualTo("b"));
        Assert.That(result.CategoryName, Is.EqualTo("Random"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetThreadDetails_PostsOrderedByCreatedAt(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("First", isOriginalPost: true, createdAtOffset: TimeSpan.FromSeconds(1))
            .WithPost("Second", createdAtOffset: TimeSpan.FromSeconds(2))
            .WithPost("Third", createdAtOffset: TimeSpan.FromSeconds(3));

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var thread = builder.GetThread("Test thread");

        // Act
        var result = await repository.GetThreadDetailsAsync(thread.Id, includeDeleted: false, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Posts[0].MessageHtml, Is.EqualTo("First"));
        Assert.That(result.Posts[1].MessageHtml, Is.EqualTo("Second"));
        Assert.That(result.Posts[2].MessageHtml, Is.EqualTo("Third"));
        Assert.That(result.Posts, Is.Ordered.By(nameof(PostDetailsModel.CreatedAt)).Ascending);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetThreadDetails_PostsHaveCorrectIndex(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Post 1", isOriginalPost: true, createdAtOffset: TimeSpan.FromSeconds(1))
            .WithPost("Post 2", createdAtOffset: TimeSpan.FromSeconds(2))
            .WithPost("Post 3", createdAtOffset: TimeSpan.FromSeconds(3));

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var thread = builder.GetThread("Test thread");

        // Act
        var result = await repository.GetThreadDetailsAsync(thread.Id, includeDeleted: false, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Posts[0].Index, Is.EqualTo(1));
        Assert.That(result.Posts[1].Index, Is.EqualTo(2));
        Assert.That(result.Posts[2].Index, Is.EqualTo(3));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetThreadDetails_WhenDeletedPostsExist_WithoutIncludeDeleted_ExcludesThem(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("OP post", isOriginalPost: true, createdAtOffset: TimeSpan.FromSeconds(1))
            .WithPost("Normal post", createdAtOffset: TimeSpan.FromSeconds(2))
            .WithPost("Deleted post", isDeleted: true, createdAtOffset: TimeSpan.FromSeconds(3));

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var thread = builder.GetThread("Test thread");

        // Act
        var result = await repository.GetThreadDetailsAsync(thread.Id, includeDeleted: false, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Posts, Has.Count.EqualTo(2));
        Assert.That(result.Posts.Any(p => p.MessageHtml == "Deleted post"), Is.False);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetThreadDetails_WhenDeletedPostsExist_WithIncludeDeleted_IncludesThem(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("OP post", isOriginalPost: true, createdAtOffset: TimeSpan.FromSeconds(1))
            .WithPost("Normal post", createdAtOffset: TimeSpan.FromSeconds(2))
            .WithPost("Deleted post", isDeleted: true, createdAtOffset: TimeSpan.FromSeconds(3));

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var thread = builder.GetThread("Test thread");

        // Act
        var result = await repository.GetThreadDetailsAsync(thread.Id, includeDeleted: true, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Posts, Has.Count.EqualTo(3));
        Assert.That(result.Posts.Any(p => p.MessageHtml == "Deleted post"), Is.True);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetThreadDetails_WhenThreadIsDeleted_WithoutIncludeDeleted_ReturnsNull(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThreadAndOp("Deleted thread", isDeleted: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var thread = builder.GetThread("Deleted thread");

        // Act
        var result = await repository.GetThreadDetailsAsync(thread.Id, includeDeleted: false, cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetThreadDetails_WhenThreadIsDeleted_WithIncludeDeleted_ReturnsThread(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThreadAndOp("Deleted thread", isDeleted: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var thread = builder.GetThread("Deleted thread");

        // Act
        var result = await repository.GetThreadDetailsAsync(thread.Id, includeDeleted: true, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsDeleted, Is.True);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetThreadDetails_WhenCategoryIsDeleted_WithoutIncludeDeleted_ReturnsNull(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random", isDeleted: true)
            .WithThreadAndOp("Thread in deleted category");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var thread = builder.GetThread("Thread in deleted category");

        // Act
        var result = await repository.GetThreadDetailsAsync(thread.Id, includeDeleted: false, cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetThreadDetails_WhenThreadHasNoPosts_ReturnsNull(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Empty thread"); // No posts

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var thread = builder.GetThread("Empty thread");

        // Act
        var result = await repository.GetThreadDetailsAsync(thread.Id, includeDeleted: false, cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetThreadDetails_PostsIncludeReplies(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("OP post", isOriginalPost: true, createdAtOffset: TimeSpan.FromSeconds(1));

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var thread = builder.GetThread("Test thread");

        // Act
        var result = await repository.GetThreadDetailsAsync(thread.Id, includeDeleted: false, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Posts[0].Replies, Is.Not.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetThreadDetails_PostsIncludeAttachmentsCollections(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("OP post", isOriginalPost: true)
            .WithPostWithPicture("Post with picture", "image.jpg", createdAtOffset: TimeSpan.FromSeconds(1))
            .WithPostWithAudio("Post with audio", "audio.mp3", createdAtOffset: TimeSpan.FromSeconds(2));

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var thread = builder.GetThread("Test thread");

        // Act
        var result = await repository.GetThreadDetailsAsync(thread.Id, includeDeleted: false, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Posts, Has.Count.EqualTo(3));

        var postWithPicture = result.Posts.First(p => p.MessageHtml == "Post with picture");
        var postWithAudio = result.Posts.First(p => p.MessageHtml == "Post with audio");

        Assert.That(postWithPicture.Pictures, Has.Count.EqualTo(1));
        Assert.That(postWithAudio.Audio, Has.Count.EqualTo(1));
    }
}
