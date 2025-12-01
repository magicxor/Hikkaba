using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Post;

internal sealed class CreatePostTests : IntegrationTestBase
{
    private static PostCreateExtendedRequestModel CreatePostRequest(
        IServiceScope scope,
        long threadId,
        string categoryAlias,
        string messageText,
        string ipAddress = "127.0.0.1",
        string userAgent = "TestAgent",
        bool isSageEnabled = false,
        bool isCyclic = false,
        int bumpLimit = 500,
        int postCount = 0,
        IReadOnlyList<long>? mentionedPosts = null)
    {
        var hashService = scope.ServiceProvider.GetRequiredService<IHashService>();
        var ip = IPAddress.Parse(ipAddress);

        return new PostCreateExtendedRequestModel
        {
            BaseModel = new PostCreateRequestModel
            {
                BlobContainerId = Guid.NewGuid(),
                IsSageEnabled = isSageEnabled,
                MessageHtml = messageText,
                MessageText = messageText,
                UserIpAddress = ip.GetAddressBytes(),
                UserAgent = userAgent,
                CategoryAlias = categoryAlias,
                ThreadId = threadId,
                MentionedPosts = mentionedPosts ?? [],
                ClientInfo = new ClientInfoModel
                {
                    CountryIsoCode = "US",
                    BrowserType = "Chrome",
                    OsType = "Windows",
                },
            },
            ThreadLocalUserHash = hashService.GetHashBytes(Guid.NewGuid(), ip.GetAddressBytes()),
            IsCyclic = isCyclic,
            BumpLimit = bumpLimit,
            PostCount = postCount,
            ClientInfo = new ClientInfoModel
            {
                CountryIsoCode = "US",
                BrowserType = "Chrome",
                OsType = "Windows",
            },
        };
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreatePost_WhenValidRequest_CreatesPostSuccessfully(
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

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IPostRepository>();
        var request = CreatePostRequest(
            appScope.Scope,
            builder.Thread.Id,
            "b",
            "New reply post");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreatePostAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.PostId, Is.GreaterThan(0));
        Assert.That(result.DeletedBlobContainerIds, Is.Empty);

        // Verify post was created in DB
        var dbContext = appScope.Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var createdPost = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == result.PostId, cancellationToken);
        Assert.That(createdPost, Is.Not.Null);
        Assert.That(createdPost!.MessageText, Is.EqualTo("New reply post"));
        Assert.That(createdPost.IsOriginalPost, Is.False);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreatePost_WhenSageEnabled_DoesNotBumpThread(
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

        var dbContext = appScope.Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var originalBumpTime = builder.Thread.LastBumpAt;

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IPostRepository>();
        var request = CreatePostRequest(
            appScope.Scope,
            builder.Thread.Id,
            "b",
            "Sage post",
            isSageEnabled: true);

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        await repository.CreatePostAsync(request, emptyAttachments, cancellationToken);

        // Assert
        var updatedThread = await dbContext.Threads.FirstAsync(t => t.Id == builder.Thread.Id, cancellationToken);
        Assert.That(updatedThread.LastBumpAt, Is.EqualTo(originalBumpTime));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreatePost_WhenNotSage_BumpsThread(
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

        var dbContext = appScope.Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var originalBumpTime = builder.Thread.LastBumpAt;

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IPostRepository>();
        var request = CreatePostRequest(
            appScope.Scope,
            builder.Thread.Id,
            "b",
            "Normal reply");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        await repository.CreatePostAsync(request, emptyAttachments, cancellationToken);

        // Assert
        var updatedThread = await dbContext.Threads.FirstAsync(t => t.Id == builder.Thread.Id, cancellationToken);
        Assert.That(updatedThread.LastBumpAt, Is.GreaterThanOrEqualTo(originalBumpTime));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreatePost_InCyclicThread_DeletesOldestPost(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new PostTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Cyclic thread", isCyclic: true, bumpLimit: 3)
            .WithPost("Original post", "127.0.0.1", "Firefox", isOriginalPost: true)
            .WithPost("Second post", "127.0.0.2", "Chrome")
            .WithPost("Third post", "127.0.0.3", "Safari");

        await builder.SaveAsync(cancellationToken);

        var dbContext = appScope.Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var secondPostId = builder.Posts[1].Id;

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IPostRepository>();
        var request = CreatePostRequest(
            appScope.Scope,
            builder.Thread.Id,
            "b",
            "Fourth post - should trigger deletion",
            isCyclic: true,
            bumpLimit: 3,
            postCount: 3);

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreatePostAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.DeletedBlobContainerIds, Has.Count.EqualTo(1));

        // Verify second post (oldest non-OP) was deleted
        var secondPost = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == secondPostId, cancellationToken);
        Assert.That(secondPost, Is.Null, "Second post should be deleted in cyclic thread");

        // Verify original post still exists
        var originalPost = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == builder.Posts[0].Id, cancellationToken);
        Assert.That(originalPost, Is.Not.Null, "Original post should not be deleted");
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreatePost_WithMentionedPosts_CreatesReplies(
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

        var originalPostId = builder.LastPostId;

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IPostRepository>();
        var request = CreatePostRequest(
            appScope.Scope,
            builder.Thread.Id,
            "b",
            "Reply with mention",
            mentionedPosts: [originalPostId]);

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreatePostAsync(request, emptyAttachments, cancellationToken);

        // Assert
        var dbContext = appScope.Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var reply = await dbContext.PostsToReplies
            .FirstOrDefaultAsync(r => r.ReplyId == result.PostId, cancellationToken);

        Assert.That(reply, Is.Not.Null);
        Assert.That(reply!.PostId, Is.EqualTo(originalPostId));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreatePost_SetsCorrectClientInfo(
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

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IPostRepository>();
        var request = CreatePostRequest(
            appScope.Scope,
            builder.Thread.Id,
            "b",
            "Post with client info",
            userAgent: "Mozilla/5.0 Chrome");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreatePostAsync(request, emptyAttachments, cancellationToken);

        // Assert
        var dbContext = appScope.Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var createdPost = await dbContext.Posts.FirstAsync(p => p.Id == result.PostId, cancellationToken);

        Assert.That(createdPost.UserAgent, Is.EqualTo("Mozilla/5.0 Chrome"));
        Assert.That(createdPost.CountryIsoCode, Is.EqualTo("US"));
        Assert.That(createdPost.BrowserType, Is.EqualTo("Chrome"));
        Assert.That(createdPost.OsType, Is.EqualTo("Windows"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreatePost_SetsCorrectIpAddress(
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

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IPostRepository>();
        var testIp = "192.168.1.100";
        var request = CreatePostRequest(
            appScope.Scope,
            builder.Thread.Id,
            "b",
            "Post from specific IP",
            ipAddress: testIp);

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreatePostAsync(request, emptyAttachments, cancellationToken);

        // Assert
        var dbContext = appScope.Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var createdPost = await dbContext.Posts.FirstAsync(p => p.Id == result.PostId, cancellationToken);

        Assert.That(createdPost.UserIpAddress, Is.Not.Null);
        Assert.That(createdPost.UserIpAddress, Is.EqualTo(IPAddress.Parse(testIp).GetAddressBytes()));
    }
}
