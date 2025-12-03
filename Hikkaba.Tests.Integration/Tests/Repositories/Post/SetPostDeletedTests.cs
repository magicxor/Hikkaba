using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Models;
using Hikkaba.Shared.Services.Contracts;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Post;

internal sealed class SetPostDeletedTests : IntegrationTestBase
{
    private static void SetupUserContext(IServiceScope scope, int adminId)
    {
        var userContext = scope.ServiceProvider.GetRequiredService<IUserContext>();
        userContext.SetUser(new CurrentUser
        {
            Id = adminId,
            UserName = "admin",
            Roles = ["Administrator"],
            ModeratedCategories = [],
        });
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetPostDeleted_WhenTrue_MarksPostAsDeleted(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Post to delete", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);
        SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var postId = builder.LastPostId;

        // Act
        await repository.SetPostDeletedAsync(postId, true, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deletedPost = await dbContext.Posts
            .IgnoreQueryFilters()
            .FirstAsync(p => p.Id == postId, cancellationToken);

        Assert.That(deletedPost.IsDeleted, Is.True);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetPostDeleted_WhenFalse_UndeletesPosts(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Deleted post", isOriginalPost: true, isDeleted: true);

        await builder.SaveAsync(cancellationToken);
        SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var postId = builder.LastPostId;

        // Act
        await repository.SetPostDeletedAsync(postId, false, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var restoredPost = await dbContext.Posts.FirstAsync(p => p.Id == postId, cancellationToken);

        Assert.That(restoredPost.IsDeleted, Is.False);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetPostDeleted_SetsModifiedAt(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Post to track modification", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);
        SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var postId = builder.LastPostId;

        var originalPost = await dbContext.Posts.AsNoTracking().FirstAsync(p => p.Id == postId, cancellationToken);
        Assert.That(originalPost.ModifiedAt, Is.Null);

        // Act
        await repository.SetPostDeletedAsync(postId, true, cancellationToken);

        // Assert
        var modifiedPost = await dbContext.Posts
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstAsync(p => p.Id == postId, cancellationToken);

        Assert.That(modifiedPost.ModifiedAt, Is.Not.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetPostDeleted_SetsModifiedById(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Post to track modifier", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);
        SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var postId = builder.LastPostId;

        // Act
        await repository.SetPostDeletedAsync(postId, true, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var modifiedPost = await dbContext.Posts
            .IgnoreQueryFilters()
            .FirstAsync(p => p.Id == postId, cancellationToken);

        Assert.That(modifiedPost.ModifiedById, Is.EqualTo(builder.Admin.Id));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetPostDeleted_WhenPostDoesNotExist_ThrowsException(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Existing post", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);
        SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await repository.SetPostDeletedAsync(999999, true, cancellationToken);
        });
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetPostDeleted_DeleteMultiplePosts_AllMarkedDeleted(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Post 1", isOriginalPost: true)
            .WithPost("Post 2", "127.0.0.2", "Chrome")
            .WithPost("Post 3", "127.0.0.3", "Safari");

        await builder.SaveAsync(cancellationToken);
        SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act - delete first two posts
        await repository.SetPostDeletedAsync(builder.Posts[0].Id, true, cancellationToken);
        await repository.SetPostDeletedAsync(builder.Posts[1].Id, true, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var allPosts = await dbContext.Posts
            .IgnoreQueryFilters()
            .Where(p => p.ThreadId == builder.LastThread.Id)
            .ToListAsync(cancellationToken);

        Assert.That(allPosts[0].IsDeleted, Is.True);
        Assert.That(allPosts[1].IsDeleted, Is.True);
        Assert.That(allPosts[2].IsDeleted, Is.False);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetPostDeleted_WhenAlreadyDeleted_RemainsDeleted(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Already deleted post", isOriginalPost: true, isDeleted: true);

        await builder.SaveAsync(cancellationToken);
        SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var postId = builder.LastPostId;

        // Act
        await repository.SetPostDeletedAsync(postId, true, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var post = await dbContext.Posts
            .IgnoreQueryFilters()
            .FirstAsync(p => p.Id == postId, cancellationToken);

        Assert.That(post.IsDeleted, Is.True);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetPostDeleted_DoesNotAffectOtherPostFields(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Post content to preserve", "192.168.1.100", "TestBrowser", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);
        SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var postId = builder.LastPostId;
        var originalPost = await dbContext.Posts.AsNoTracking().FirstAsync(p => p.Id == postId, cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act
        await repository.SetPostDeletedAsync(postId, true, cancellationToken);

        // Assert
        var modifiedPost = await dbContext.Posts
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstAsync(p => p.Id == postId, cancellationToken);

        Assert.That(modifiedPost.MessageText, Is.EqualTo(originalPost.MessageText));
        Assert.That(modifiedPost.MessageHtml, Is.EqualTo(originalPost.MessageHtml));
        Assert.That(modifiedPost.UserIpAddress, Is.EqualTo(originalPost.UserIpAddress));
        Assert.That(modifiedPost.UserAgent, Is.EqualTo(originalPost.UserAgent));
        Assert.That(modifiedPost.ThreadId, Is.EqualTo(originalPost.ThreadId));
        Assert.That(modifiedPost.BlobContainerId, Is.EqualTo(originalPost.BlobContainerId));
    }
}
