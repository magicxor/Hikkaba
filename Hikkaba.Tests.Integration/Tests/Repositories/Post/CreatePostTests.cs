using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Constants;
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
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Original post", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var request = CreatePostRequest(
            appScope.ServiceScope,
            builder.LastThread.Id,
            "b",
            "New reply post");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreatePostAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.PostId, Is.GreaterThan(0));
        Assert.That(result.DeletedBlobContainerIds, Is.Empty);

        // Verify post was created in DB
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
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
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Original post", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var originalBumpTime = builder.LastThread.LastBumpAt;

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var request = CreatePostRequest(
            appScope.ServiceScope,
            builder.LastThread.Id,
            "b",
            "Sage post",
            isSageEnabled: true);

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        await repository.CreatePostAsync(request, emptyAttachments, cancellationToken);

        // Assert
        var updatedThread = await dbContext.Threads.FirstAsync(t => t.Id == builder.LastThread.Id, cancellationToken);
        Assert.That(updatedThread.LastBumpAt, Is.EqualTo(originalBumpTime));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreatePost_WhenNotSage_BumpsThread(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Original post", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var originalBumpTime = builder.LastThread.LastBumpAt;

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var request = CreatePostRequest(
            appScope.ServiceScope,
            builder.LastThread.Id,
            "b",
            "Normal reply");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        await repository.CreatePostAsync(request, emptyAttachments, cancellationToken);

        // Assert
        var updatedThread = await dbContext.Threads.FirstAsync(t => t.Id == builder.LastThread.Id, cancellationToken);
        Assert.That(updatedThread.LastBumpAt, Is.GreaterThanOrEqualTo(originalBumpTime));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreatePost_InCyclicThread_DeletesOldestPost(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random")
            .WithThread("Cyclic thread", isCyclic: true, bumpLimit: 3)
            .WithPost("Original post", isOriginalPost: true)
            .WithPost("Second post", "127.0.0.2", "Chrome")
            .WithPost("Third post", "127.0.0.3", "Safari");

        await builder.SaveAsync(cancellationToken);

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var originalPostId = builder.GetPost("Original post").Id;
        var secondPostId = builder.GetPost("Second post").Id;

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var request = CreatePostRequest(
            appScope.ServiceScope,
            builder.LastThread.Id,
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
        var originalPost = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == originalPostId, cancellationToken);
        Assert.That(originalPost, Is.Not.Null, "Original post should not be deleted");
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreatePost_InCyclicThread_DeletesOldestPostWithAllAttachmentsAndRelations(
        CancellationToken cancellationToken)
    {
        // Arrange
        // This test verifies that cascade deletion works correctly when a post has:
        // - ModifiedBy set
        // - All types of attachments (Audio, Document, Notice, Picture, Video)
        // - Replies to other posts (MentionedPosts)
        // - Replies from other posts (Replies)
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random")
            .WithThread("Cyclic thread", isCyclic: true, bumpLimit: 3)
            .WithPost("Original post", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var originalPostId = builder.LastPostId;

        // Create second post (to be deleted) with all attachments and ModifiedBy
        builder
            .WithPostThatMentionsPost("Second post with attachments", mentionedPostMessageText: "Original post", ipAddress: "127.0.0.2", userAgent: "Chrome")
            .WithModifiedBy(builder.Admin)
            .WithAudio()
            .WithDocument()
            .WithNotice("Admin notice on second post")
            .WithPicture()
            .WithVideo();

        await builder.SaveAsync(cancellationToken);

        var secondPostId = builder.LastPostId;

        // Create third post that replies to the second post
        builder.WithPostThatMentionsPost("Third post replying to second", mentionedPostMessageText: "Second post with attachments", ipAddress: "127.0.0.3", userAgent: "Safari");

        await builder.SaveAsync(cancellationToken);

        var thirdPostId = builder.LastPostId;

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Clear change tracker to simulate a fresh DbContext (as would happen in a real request)
        // This is important because the repository will load posts fresh from the database
        dbContext.ChangeTracker.Clear();

        // Verify attachments were created
        var audiosBefore = await dbContext.Audios.CountAsync(a => a.PostId == secondPostId, cancellationToken);
        var documentsBefore = await dbContext.Documents.CountAsync(d => d.PostId == secondPostId, cancellationToken);
        var noticesBefore = await dbContext.Notices.CountAsync(n => n.PostId == secondPostId, cancellationToken);
        var picturesBefore = await dbContext.Pictures.CountAsync(p => p.PostId == secondPostId, cancellationToken);
        var videosBefore = await dbContext.Videos.CountAsync(v => v.PostId == secondPostId, cancellationToken);

        Assert.That(audiosBefore, Is.EqualTo(1), "Audio attachment should exist before deletion");
        Assert.That(documentsBefore, Is.EqualTo(1), "Document attachment should exist before deletion");
        Assert.That(noticesBefore, Is.EqualTo(1), "Notice attachment should exist before deletion");
        Assert.That(picturesBefore, Is.EqualTo(1), "Picture attachment should exist before deletion");
        Assert.That(videosBefore, Is.EqualTo(1), "Video attachment should exist before deletion");

        // Verify PostToReply relations exist
        var repliesBeforeCount = await dbContext.PostsToReplies
            .CountAsync(ptr => ptr.PostId == secondPostId || ptr.ReplyId == secondPostId, cancellationToken);
        Assert.That(repliesBeforeCount, Is.EqualTo(2), "Second post should have 2 PostToReply relations (1 as reply, 1 as mentioned)");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var request = CreatePostRequest(
            appScope.ServiceScope,
            builder.LastThread.Id,
            "b",
            "Fourth post - should trigger deletion of second post",
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

        // Verify all attachments were cascade deleted
        var audiosAfter = await dbContext.Audios.CountAsync(a => a.PostId == secondPostId, cancellationToken);
        var documentsAfter = await dbContext.Documents.CountAsync(d => d.PostId == secondPostId, cancellationToken);
        var noticesAfter = await dbContext.Notices.CountAsync(n => n.PostId == secondPostId, cancellationToken);
        var picturesAfter = await dbContext.Pictures.CountAsync(p => p.PostId == secondPostId, cancellationToken);
        var videosAfter = await dbContext.Videos.CountAsync(v => v.PostId == secondPostId, cancellationToken);

        Assert.That(audiosAfter, Is.EqualTo(0), "Audio attachment should be cascade deleted");
        Assert.That(documentsAfter, Is.EqualTo(0), "Document attachment should be cascade deleted");
        Assert.That(noticesAfter, Is.EqualTo(0), "Notice attachment should be cascade deleted");
        Assert.That(picturesAfter, Is.EqualTo(0), "Picture attachment should be cascade deleted");
        Assert.That(videosAfter, Is.EqualTo(0), "Video attachment should be cascade deleted");

        // Verify PostToReply relations were cascade deleted
        var repliesAfterCount = await dbContext.PostsToReplies
            .CountAsync(ptr => ptr.PostId == secondPostId || ptr.ReplyId == secondPostId, cancellationToken);
        Assert.That(repliesAfterCount, Is.EqualTo(0), "PostToReply relations should be cascade deleted");

        // Verify original post still exists
        var originalPost = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == originalPostId, cancellationToken);
        Assert.That(originalPost, Is.Not.Null, "Original post should not be deleted");

        // Verify third post still exists (only its reply relation to deleted post should be removed)
        var thirdPost = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == thirdPostId, cancellationToken);
        Assert.That(thirdPost, Is.Not.Null, "Third post should not be deleted");

        // Verify admin user still exists (should not be cascade deleted)
        var admin = await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == Defaults.AdministratorUserName, cancellationToken);
        Assert.That(admin, Is.Not.Null, "Admin user should not be cascade deleted");
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreatePost_InCyclicThread_DeletesOldestPostWithCrossThreadReplies(
        CancellationToken cancellationToken)
    {
        // Arrange: Create a cyclic thread with posts, where one post is referenced by a post in another thread.
        // This tests that ClientCascade correctly deletes PostToReply records when the mentioned post
        // (PostId side) is deleted, while the reply post (in another thread) survives.
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random")
            .WithThread("Cyclic thread", bumpLimit: 3, isCyclic: true)
            .WithPost("OP post", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);
        var originalPostId = builder.LastPostId;

        // Add second post (this will be deleted when bump limit is exceeded)
        builder
            .WithPost("Second post - will be deleted", "127.0.0.2", "Chrome");

        await builder.SaveAsync(cancellationToken);
        var secondPostId = builder.LastPostId;

        // Add third post
        builder
            .WithPost("Third post", "127.0.0.3", "Safari");

        await builder.SaveAsync(cancellationToken);
        var thirdPostId = builder.LastPostId;

        // Save the cyclic thread reference before creating another thread
        var cyclicThread = builder.GetThread("Cyclic thread");

        // Create a post in ANOTHER thread that replies to secondPost (the one that will be deleted)
        // This creates a cross-thread reply where:
        // - secondPost (in cyclic thread) is the mentioned post (PostId)
        // - crossThreadPost (in another thread) is the reply (ReplyId)
        builder
            .WithThread("Other thread with cross-thread reply")
            .WithPostThatMentionsPost(
                "OP that mentions another post",
                mentionedPostMessageText: "Second post - will be deleted",
                mentionedThreadTitle: "Cyclic thread",
                isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.ChangeTracker.Clear();

        // Verify cross-thread PostToReply relation exists
        var crossThreadReplyBefore = await dbContext.PostsToReplies
            .CountAsync(ptr => ptr.PostId == secondPostId, cancellationToken);
        Assert.That(crossThreadReplyBefore, Is.EqualTo(1), "Cross-thread reply to second post should exist");

        // Verify there are 3 posts in the cyclic thread
        var postsInCyclicThread = await dbContext.Posts
            .CountAsync(p => p.ThreadId == cyclicThread.Id, cancellationToken);
        Assert.That(postsInCyclicThread, Is.EqualTo(3), "Cyclic thread should have 3 posts before adding new one");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var request = CreatePostRequest(
            appScope.ServiceScope,
            cyclicThread.Id,
            "b",
            "Fourth post - should trigger deletion of second post",
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

        // Verify cross-thread PostToReply relation was cascade deleted
        // This is the key assertion - when secondPost is deleted, the PostToReply record
        // should be deleted via Cascade on PostId (since secondPost is the mentioned post)
        var crossThreadReplyAfter = await dbContext.PostsToReplies
            .CountAsync(ptr => ptr.PostId == secondPostId, cancellationToken);
        Assert.That(crossThreadReplyAfter, Is.EqualTo(0), "Cross-thread PostToReply should be cascade deleted when mentioned post is deleted");

        // Verify other posts still exist
        var originalPost = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == originalPostId, cancellationToken);
        Assert.That(originalPost, Is.Not.Null, "Original post should not be deleted");

        var thirdPost = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == thirdPostId, cancellationToken);
        Assert.That(thirdPost, Is.Not.Null, "Third post should not be deleted");

        // Verify cross-thread reply post still exists (only the PostToReply relation should be removed)
        var crossThreadPosts = await dbContext.Posts
            .Where(p => p.Thread.Title == "Other thread with cross-thread reply")
            .ToListAsync(cancellationToken);
        Assert.That(crossThreadPosts, Has.Count.EqualTo(1), "Cross-thread reply post should still exist");
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreatePost_InCyclicThread_DeletesOldestPostWithCrossThreadMentions(
        CancellationToken cancellationToken)
    {
        // Arrange: Create a cyclic thread with posts, where one post mentions (replies to) a post in another thread.
        // This tests that ClientCascade correctly deletes PostToReply records when the reply post
        // (ReplyId side) is deleted, while the mentioned post (in another thread) survives.
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        // First, create another thread with a post that will be mentioned
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random")
            .WithThread("Other thread with mentioned post")
            .WithPost("Post that will be mentioned", "192.168.1.1", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);
        var mentionedPostId = builder.LastPostId;
        var mentionedThreadId = builder.LastThread.Id;

        // Now create the cyclic thread
        builder
            .WithThread("Cyclic thread", bumpLimit: 3, isCyclic: true)
            .WithPost("OP post", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);
        var originalPostId = builder.LastPostId;

        var cyclicThread = builder.GetThread("Cyclic thread");

        // Add second post that is a reply to the post in other thread (this will be deleted)
        builder
            .WithPostThatMentionsPost(
                "Second post - reply to other thread, will be deleted",
                mentionedPostMessageText: "Post that will be mentioned",
                mentionedThreadTitle: "Other thread with mentioned post",
                ipAddress: "127.0.0.50",
                userAgent: "CrossThreadReplyAgent");

        await builder.SaveAsync(cancellationToken);
        var secondPostId = builder.LastPostId;

        // Add third post
        builder
            .WithPost("Third post", "127.0.0.3", "Safari");

        await builder.SaveAsync(cancellationToken);
        var thirdPostId = builder.LastPostId;

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.ChangeTracker.Clear();

        // Verify cross-thread PostToReply relation exists (secondPost replies to mentionedPost)
        var crossThreadReplyBefore = await dbContext.PostsToReplies
            .CountAsync(ptr => ptr.ReplyId == secondPostId && ptr.PostId == mentionedPostId, cancellationToken);
        Assert.That(crossThreadReplyBefore, Is.EqualTo(1), "Cross-thread reply from second post should exist");

        // Verify there are 3 posts in the cyclic thread
        var postsInCyclicThread = await dbContext.Posts
            .CountAsync(p => p.ThreadId == cyclicThread.Id, cancellationToken);
        Assert.That(postsInCyclicThread, Is.EqualTo(3), "Cyclic thread should have 3 posts before adding new one");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var request = CreatePostRequest(
            appScope.ServiceScope,
            cyclicThread.Id,
            "b",
            "Fourth post - should trigger deletion of second post",
            isCyclic: true,
            bumpLimit: 3,
            postCount: 3);

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreatePostAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.DeletedBlobContainerIds, Has.Count.EqualTo(1));

        // Verify second post (oldest non-OP, the reply) was deleted
        var secondPost = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == secondPostId, cancellationToken);
        Assert.That(secondPost, Is.Null, "Second post (cross-thread reply) should be deleted in cyclic thread");

        // Verify cross-thread PostToReply relation was cascade deleted via ClientCascade on ReplyId
        var crossThreadReplyAfter = await dbContext.PostsToReplies
            .CountAsync(ptr => ptr.ReplyId == secondPostId, cancellationToken);
        Assert.That(crossThreadReplyAfter, Is.EqualTo(0), "Cross-thread PostToReply should be cascade deleted when reply post is deleted");

        // Verify other posts still exist
        var originalPost = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == originalPostId, cancellationToken);
        Assert.That(originalPost, Is.Not.Null, "Original post should not be deleted");

        var thirdPost = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == thirdPostId, cancellationToken);
        Assert.That(thirdPost, Is.Not.Null, "Third post should not be deleted");

        // Verify mentioned post in other thread still exists (only the PostToReply relation should be removed)
        var mentionedPost = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == mentionedPostId, cancellationToken);
        Assert.That(mentionedPost, Is.Not.Null, "Mentioned post in other thread should still exist");

        // Verify other thread still exists
        var otherThread = await dbContext.Threads.FirstOrDefaultAsync(t => t.Id == mentionedThreadId, cancellationToken);
        Assert.That(otherThread, Is.Not.Null, "Other thread should still exist");
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreatePost_WithMentionedPosts_CreatesReplies(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Original post", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var originalPostId = builder.LastPostId;

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var request = CreatePostRequest(
            appScope.ServiceScope,
            builder.LastThread.Id,
            "b",
            "Reply with mention",
            mentionedPosts: [originalPostId]);

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreatePostAsync(request, emptyAttachments, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
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
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Original post", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var request = CreatePostRequest(
            appScope.ServiceScope,
            builder.LastThread.Id,
            "b",
            "Post with client info",
            userAgent: "Mozilla/5.0 Chrome");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreatePostAsync(request, emptyAttachments, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
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
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Original post", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var testIp = "192.168.1.100";
        var request = CreatePostRequest(
            appScope.ServiceScope,
            builder.LastThread.Id,
            "b",
            "Post from specific IP",
            ipAddress: testIp);

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreatePostAsync(request, emptyAttachments, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var createdPost = await dbContext.Posts.FirstAsync(p => p.Id == result.PostId, cancellationToken);

        Assert.That(createdPost.UserIpAddress, Is.Not.Null);
        Assert.That(createdPost.UserIpAddress, Is.EqualTo(IPAddress.Parse(testIp).GetAddressBytes()));
    }
}
