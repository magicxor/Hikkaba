using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using Hikkaba.Infrastructure.Models.Error;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Constants;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Thread;

internal sealed class CreateThreadTests : IntegrationTestBase
{
    private static ThreadCreateExtendedRequestModel CreateThreadRequest(
        string categoryAlias,
        string threadTitle,
        string messageText,
        string ipAddress = "127.0.0.1",
        string userAgent = "TestAgent")
    {
        var ip = IPAddress.Parse(ipAddress);

        return new ThreadCreateExtendedRequestModel
        {
            BaseModel = new ThreadCreateRequestModel
            {
                CategoryAlias = categoryAlias,
                ThreadTitle = threadTitle,
                BlobContainerId = Guid.NewGuid(),
                MessageHtml = messageText,
                MessageText = messageText,
                UserIpAddress = ip.GetAddressBytes(),
                UserAgent = userAgent,
                ClientInfo = new ClientInfoModel
                {
                    CountryIsoCode = "US",
                    BrowserType = "Chrome",
                    OsType = "Windows",
                },
            },
            ThreadSalt = Guid.NewGuid(),
            ThreadLocalUserHash = new byte[32],
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
    public async Task CreateThread_WhenValidRequest_CreatesThreadAndPost(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("b", "New thread", "Hello world!");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<ThreadPostCreateSuccessResultModel>(), "Expected success result");
        var success = result.AsT0;
        Assert.That(success.ThreadId, Is.GreaterThan(0));
        Assert.That(success.PostId, Is.GreaterThan(0));
        Assert.That(success.DeletedBlobContainerIds, Is.Empty);

        // Verify thread was created
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var createdThread = await dbContext.Threads
            .Include(t => t.Posts)
            .FirstOrDefaultAsync(t => t.Id == success.ThreadId, cancellationToken);

        Assert.That(createdThread, Is.Not.Null);
        Assert.That(createdThread!.Title, Is.EqualTo("New thread"));
        Assert.That(createdThread.Posts, Has.Count.EqualTo(1));
        var opPost = createdThread.Posts.First();
        Assert.That(opPost.MessageText, Is.EqualTo("Hello world!"));
        Assert.That(opPost.IsOriginalPost, Is.True);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_WhenCategoryNotFound_ReturnsDomainError(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("nonexistent", "New thread", "Hello!");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<DomainError>(), "Expected error result");
        var error = result.AsT1;
        Assert.That(error.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_WhenCategoryIsDeleted_ReturnsDomainError(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random", isDeleted: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("b", "New thread", "Hello!");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<DomainError>(), "Expected error result");
        var error = result.AsT1;
        Assert.That(error.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_UsesCategoryDefaultBumpLimit(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random", defaultBumpLimit: 250);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("b", "New thread", "Hello!");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<ThreadPostCreateSuccessResultModel>(), "Expected success result");
        var success = result.AsT0;

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var thread = await dbContext.Threads.FirstAsync(t => t.Id == success.ThreadId, cancellationToken);
        Assert.That(thread.BumpLimit, Is.EqualTo(250));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_WhenCategoryDefaultBumpLimitIsZero_UsesDefaultBumpLimit(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random", defaultBumpLimit: 0);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("b", "New thread", "Hello!");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<ThreadPostCreateSuccessResultModel>(), "Expected success result");
        var success = result.AsT0;

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var thread = await dbContext.Threads.FirstAsync(t => t.Id == success.ThreadId, cancellationToken);
        Assert.That(thread.BumpLimit, Is.EqualTo(Defaults.DefaultBumpLimit));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_SetsCorrectClientInfo(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("b", "New thread", "Hello!", userAgent: "Mozilla/5.0 Firefox");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<ThreadPostCreateSuccessResultModel>(), "Expected success result");
        var success = result.AsT0;

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var post = await dbContext.Posts.FirstAsync(p => p.Id == success.PostId, cancellationToken);

        Assert.That(post.UserAgent, Is.EqualTo("Mozilla/5.0 Firefox"));
        Assert.That(post.CountryIsoCode, Is.EqualTo("US"));
        Assert.That(post.BrowserType, Is.EqualTo("Chrome"));
        Assert.That(post.OsType, Is.EqualTo("Windows"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_SetsLastBumpAtToCreatedAt(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("b", "New thread", "Hello!");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<ThreadPostCreateSuccessResultModel>(), "Expected success result");
        var success = result.AsT0;

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var thread = await dbContext.Threads.FirstAsync(t => t.Id == success.ThreadId, cancellationToken);

        Assert.That(thread.LastBumpAt, Is.EqualTo(thread.CreatedAt));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_WhenMaxThreadCountReached_DeletesOldestThread(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random");

        // Manually set MaxThreadCount to a small number for testing
        var category = builder.GetCategory("b");
        category.MaxThreadCount = 3;

        // Create 3 threads (at max capacity)
        builder
            .WithThreadAndOp("Oldest thread", createdAt: utcNow.AddDays(-3), lastBumpAt: utcNow.AddDays(-3))
            .WithThreadAndOp("Middle thread", createdAt: utcNow.AddDays(-2), lastBumpAt: utcNow.AddDays(-2))
            .WithThreadAndOp("Newest thread", createdAt: utcNow.AddDays(-1), lastBumpAt: utcNow.AddDays(-1));

        await builder.SaveAsync(cancellationToken);

        var oldestThread = builder.GetThread("Oldest thread");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("b", "Brand new thread", "Hello!");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<ThreadPostCreateSuccessResultModel>(), "Expected success result");
        var success = result.AsT0;
        Assert.That(success.DeletedBlobContainerIds, Has.Count.GreaterThan(0));

        // Verify oldest thread was deleted
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deletedThread = await dbContext.Threads.FirstOrDefaultAsync(t => t.Id == oldestThread.Id, cancellationToken);
        Assert.That(deletedThread, Is.Null, "Oldest thread should be deleted");

        // Verify new thread was created
        var newThread = await dbContext.Threads.FirstOrDefaultAsync(t => t.Id == success.ThreadId, cancellationToken);
        Assert.That(newThread, Is.Not.Null);
        Assert.That(newThread!.Title, Is.EqualTo("Brand new thread"));

        // Verify thread count is at max
        var threadCount = await dbContext.Threads.CountAsync(t => t.CategoryId == category.Id, cancellationToken);
        Assert.That(threadCount, Is.EqualTo(3));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_WhenMaxThreadCountReached_DeletesOldestThreadWithAllAttachmentsAndRelations(
        CancellationToken cancellationToken)
    {
        // Arrange
        // This test verifies that cascade deletion works correctly when a thread has:
        // - ModifiedBy set on the thread
        // - Posts with ModifiedBy set
        // - All types of attachments (Audio, Document, Notice, Picture, Video)
        // - PostToReply relations (replies between posts)
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random");

        // Manually set MaxThreadCount to a small number for testing
        var category = builder.GetCategory("b");
        category.MaxThreadCount = 3;

        // Create 3 enriched threads (at max capacity)
        // Each thread has ModifiedBy, posts with all attachment types, and PostToReply relations
        builder
            .WithEnrichedThreadAndPosts(
                "Oldest enriched thread",
                builder.Admin,
                createdAt: utcNow.AddDays(-3),
                lastBumpAt: utcNow.AddDays(-3))
            .WithEnrichedThreadAndPosts(
                "Middle enriched thread",
                builder.Admin,
                createdAt: utcNow.AddDays(-2),
                lastBumpAt: utcNow.AddDays(-2))
            .WithEnrichedThreadAndPosts(
                "Newest enriched thread",
                builder.Admin,
                createdAt: utcNow.AddDays(-1),
                lastBumpAt: utcNow.AddDays(-1));

        await builder.SaveAsync(cancellationToken);

        var oldestThread = builder.GetThread("Oldest enriched thread");
        var oldestThreadId = oldestThread.Id;

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Get post IDs from the oldest thread for verification
        var oldestThreadPostIds = await dbContext.Posts
            .Where(p => p.ThreadId == oldestThreadId)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        // Verify attachments exist before deletion
        var audiosBefore = await dbContext.Audios.CountAsync(a => oldestThreadPostIds.Contains(a.PostId), cancellationToken);
        var documentsBefore = await dbContext.Documents.CountAsync(d => oldestThreadPostIds.Contains(d.PostId), cancellationToken);
        var noticesBefore = await dbContext.Notices.CountAsync(n => oldestThreadPostIds.Contains(n.PostId), cancellationToken);
        var picturesBefore = await dbContext.Pictures.CountAsync(p => oldestThreadPostIds.Contains(p.PostId), cancellationToken);
        var videosBefore = await dbContext.Videos.CountAsync(v => oldestThreadPostIds.Contains(v.PostId), cancellationToken);

        Assert.That(audiosBefore, Is.GreaterThan(0), "Audio attachments should exist before deletion");
        Assert.That(documentsBefore, Is.GreaterThan(0), "Document attachments should exist before deletion");
        Assert.That(noticesBefore, Is.GreaterThan(0), "Notice attachments should exist before deletion");
        Assert.That(picturesBefore, Is.GreaterThan(0), "Picture attachments should exist before deletion");
        Assert.That(videosBefore, Is.GreaterThan(0), "Video attachments should exist before deletion");

        // Verify PostToReply relations exist
        var repliesBeforeCount = await dbContext.PostsToReplies
            .CountAsync(ptr => oldestThreadPostIds.Contains(ptr.PostId) || oldestThreadPostIds.Contains(ptr.ReplyId), cancellationToken);
        Assert.That(repliesBeforeCount, Is.GreaterThan(0), "PostToReply relations should exist before deletion");

        // Clear change tracker to simulate a fresh DbContext
        dbContext.ChangeTracker.Clear();

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("b", "Brand new thread", "Hello!");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<ThreadPostCreateSuccessResultModel>(), "Expected success result");
        var success = result.AsT0;
        Assert.That(success.DeletedBlobContainerIds, Has.Count.GreaterThan(0), "Should have deleted blob containers");

        // Verify oldest thread was deleted
        var deletedThread = await dbContext.Threads.FirstOrDefaultAsync(t => t.Id == oldestThreadId, cancellationToken);
        Assert.That(deletedThread, Is.Null, "Oldest thread should be deleted");

        // Verify all posts from the oldest thread were cascade deleted
        var deletedPosts = await dbContext.Posts.Where(p => p.ThreadId == oldestThreadId).ToListAsync(cancellationToken);
        Assert.That(deletedPosts, Is.Empty, "All posts in oldest thread should be cascade deleted");

        // Verify all attachments were cascade deleted
        var audiosAfter = await dbContext.Audios.CountAsync(a => oldestThreadPostIds.Contains(a.PostId), cancellationToken);
        var documentsAfter = await dbContext.Documents.CountAsync(d => oldestThreadPostIds.Contains(d.PostId), cancellationToken);
        var noticesAfter = await dbContext.Notices.CountAsync(n => oldestThreadPostIds.Contains(n.PostId), cancellationToken);
        var picturesAfter = await dbContext.Pictures.CountAsync(p => oldestThreadPostIds.Contains(p.PostId), cancellationToken);
        var videosAfter = await dbContext.Videos.CountAsync(v => oldestThreadPostIds.Contains(v.PostId), cancellationToken);

        Assert.That(audiosAfter, Is.Zero, "Audio attachments should be cascade deleted");
        Assert.That(documentsAfter, Is.Zero, "Document attachments should be cascade deleted");
        Assert.That(noticesAfter, Is.Zero, "Notice attachments should be cascade deleted");
        Assert.That(picturesAfter, Is.Zero, "Picture attachments should be cascade deleted");
        Assert.That(videosAfter, Is.Zero, "Video attachments should be cascade deleted");

        // Verify PostToReply relations were cascade deleted
        var repliesAfterCount = await dbContext.PostsToReplies
            .CountAsync(ptr => oldestThreadPostIds.Contains(ptr.PostId) || oldestThreadPostIds.Contains(ptr.ReplyId), cancellationToken);
        Assert.That(repliesAfterCount, Is.Zero, "PostToReply relations should be cascade deleted");

        // Verify new thread was created
        var newThread = await dbContext.Threads.FirstOrDefaultAsync(t => t.Id == success.ThreadId, cancellationToken);
        Assert.That(newThread, Is.Not.Null);
        Assert.That(newThread!.Title, Is.EqualTo("Brand new thread"));

        // Verify thread count is at max
        var threadCountAfter = await dbContext.Threads.CountAsync(t => t.CategoryId == category.Id, cancellationToken);
        Assert.That(threadCountAfter, Is.EqualTo(3));

        // Verify admin user still exists (should not be cascade deleted)
        var admin = await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == Defaults.AdministratorUserName, cancellationToken);
        Assert.That(admin, Is.Not.Null, "Admin user should not be cascade deleted");
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_WhenMaxThreadCountReached_DeletesOldestThreadWithCrossThreadReplies(
        CancellationToken cancellationToken)
    {
        // Arrange
        // This test verifies ClientCascade for PostToReply.ReplyId works correctly
        // when a thread with posts that REPLY TO posts in OTHER threads is deleted.
        // The cascade chain is:
        // - Thread (to be deleted) -> Post (Reply) -> PostToReply.ReplyId (ClientCascade)
        // Since the mentioned post is in a different thread, only ClientCascade can delete the PostToReply
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random");

        // Set MaxThreadCount to a small number for testing
        var category = builder.GetCategory("b");
        category.MaxThreadCount = 3;

        // Create 3 threads
        builder
            .WithThreadAndOp(
                "Oldest thread (to be deleted)",
                createdAt: utcNow.AddDays(-3),
                lastBumpAt: utcNow.AddDays(-3))
            .WithThreadAndOp(
                "Middle thread (survives)",
                createdAt: utcNow.AddDays(-2),
                lastBumpAt: utcNow.AddDays(-2))
            .WithThreadAndOp(
                "Newest thread (survives)",
                createdAt: utcNow.AddDays(-1),
                lastBumpAt: utcNow.AddDays(-1));

        await builder.SaveAsync(cancellationToken);

        // Now create a cross-thread reply:
        // A post in "Oldest thread" replies to OP in "Middle thread"
        // When "Oldest thread" is deleted, the PostToReply record must be deleted via ClientCascade
        builder
            .WithPostThatMentionsPost(
                "Cross-thread reply from oldest to middle",
                inThreadTitle: "Oldest thread (to be deleted)",
                mentionedThreadTitle: "Middle thread (survives)");

        await builder.SaveAsync(cancellationToken);

        var oldestThread = builder.GetThread("Oldest thread (to be deleted)");
        var middleThread = builder.GetThread("Middle thread (survives)");
        var oldestThreadId = oldestThread.Id;

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Get the reply post ID (the one that will be deleted)
        var replyPostId = await dbContext.Posts
            .Where(p => p.ThreadId == oldestThreadId && !p.IsOriginalPost)
            .Select(p => p.Id)
            .FirstAsync(cancellationToken);

        // Verify the cross-thread PostToReply exists
        var crossThreadReplyBefore = await dbContext.PostsToReplies
            .Include(postToReply => postToReply.Post)
            .FirstOrDefaultAsync(ptr => ptr.ReplyId == replyPostId, cancellationToken);
        Assert.That(crossThreadReplyBefore, Is.Not.Null, "Cross-thread PostToReply should exist before deletion");

        // Verify the mentioned post is in a different thread
        Assert.That(
            crossThreadReplyBefore.Post.ThreadId,
            Is.EqualTo(middleThread.Id),
            "Mentioned post should be in middle thread");

        // Clear change tracker to simulate a fresh DbContext
        dbContext.ChangeTracker.Clear();

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("b", "Brand new thread", "Hello!");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<ThreadPostCreateSuccessResultModel>(), "Expected success result");

        // Verify oldest thread was deleted
        var deletedThread = await dbContext.Threads.FirstOrDefaultAsync(t => t.Id == oldestThreadId, cancellationToken);
        Assert.That(deletedThread, Is.Null, "Oldest thread should be deleted");

        // Verify cross-thread PostToReply was deleted (via ClientCascade on ReplyId)
        var crossThreadReplyAfter = await dbContext.PostsToReplies
            .FirstOrDefaultAsync(ptr => ptr.ReplyId == replyPostId, cancellationToken);
        Assert.That(
            crossThreadReplyAfter,
            Is.Null,
            "Cross-thread PostToReply should be deleted via ClientCascade when reply post is deleted");

        // Verify middle thread still exists with its OP
        var middleThreadAfter = await dbContext.Threads
            .Include(t => t.Posts)
            .FirstOrDefaultAsync(t => t.Id == middleThread.Id, cancellationToken);
        Assert.That(middleThreadAfter, Is.Not.Null, "Middle thread should still exist");
        Assert.That(middleThreadAfter!.Posts, Has.Count.EqualTo(1), "Middle thread should still have its OP");
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_WhenMaxThreadCountReached_DeletesOldestThreadWithCrossThreadMentions(
        CancellationToken cancellationToken)
    {
        // Arrange: Test the opposite scenario - a post in another thread mentions a post in the oldest thread.
        // When the oldest thread is deleted, the PostToReply record should be deleted via Cascade on PostId.
        // The cascade chain is:
        // - Thread (to be deleted) -> Post (Mentioned) -> PostToReply.PostId (Cascade)
        // Since the reply post is in a different thread that survives, only PostId Cascade deletes the PostToReply
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random");

        // Set MaxThreadCount to a small number for testing
        var category = builder.GetCategory("b");
        category.MaxThreadCount = 3;

        // Create 3 threads
        builder
            .WithThreadAndOp(
                "Oldest thread (to be deleted, has mentioned post)",
                createdAt: utcNow.AddDays(-3),
                lastBumpAt: utcNow.AddDays(-3))
            .WithThreadAndOp(
                "Middle thread (survives, has reply post)",
                createdAt: utcNow.AddDays(-2),
                lastBumpAt: utcNow.AddDays(-2))
            .WithThreadAndOp(
                "Newest thread (survives)",
                createdAt: utcNow.AddDays(-1),
                lastBumpAt: utcNow.AddDays(-1));

        await builder.SaveAsync(cancellationToken);

        // Create a cross-thread mention:
        // A post in "Middle thread" replies to OP in "Oldest thread"
        // When "Oldest thread" is deleted, the PostToReply record must be deleted via Cascade on PostId
        builder
            .WithPostThatMentionsPost(
                "Cross-thread reply from middle to oldest",
                inThreadTitle: "Middle thread (survives, has reply post)",
                mentionedThreadTitle: "Oldest thread (to be deleted, has mentioned post)");

        await builder.SaveAsync(cancellationToken);

        var oldestThread = builder.GetThread("Oldest thread (to be deleted, has mentioned post)");
        var middleThread = builder.GetThread("Middle thread (survives, has reply post)");
        var oldestThreadId = oldestThread.Id;
        var oldestThreadOpId = oldestThread.Posts.First(p => p.IsOriginalPost).Id;

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Get the reply post ID (the one in middle thread that mentions oldest thread's OP)
        var replyPostId = await dbContext.Posts
            .Where(p => p.ThreadId == middleThread.Id && !p.IsOriginalPost)
            .Select(p => p.Id)
            .FirstAsync(cancellationToken);

        // Verify the cross-thread PostToReply exists
        var crossThreadReplyBefore = await dbContext.PostsToReplies
            .FirstOrDefaultAsync(ptr => ptr.PostId == oldestThreadOpId && ptr.ReplyId == replyPostId, cancellationToken);
        Assert.That(crossThreadReplyBefore, Is.Not.Null, "Cross-thread PostToReply should exist before deletion");

        // Clear change tracker to simulate a fresh DbContext
        dbContext.ChangeTracker.Clear();

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("b", "Brand new thread", "Hello!");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<ThreadPostCreateSuccessResultModel>(), "Expected success result");

        // Verify oldest thread was deleted
        var deletedThread = await dbContext.Threads.FirstOrDefaultAsync(t => t.Id == oldestThreadId, cancellationToken);
        Assert.That(deletedThread, Is.Null, "Oldest thread should be deleted");

        // Verify cross-thread PostToReply was deleted (via Cascade on PostId)
        var crossThreadReplyAfter = await dbContext.PostsToReplies
            .FirstOrDefaultAsync(ptr => ptr.PostId == oldestThreadOpId, cancellationToken);
        Assert.That(
            crossThreadReplyAfter,
            Is.Null,
            "Cross-thread PostToReply should be deleted via Cascade when mentioned post is deleted");

        // Verify middle thread still exists with its OP and reply post
        var middleThreadAfter = await dbContext.Threads
            .Include(t => t.Posts)
            .FirstOrDefaultAsync(t => t.Id == middleThread.Id, cancellationToken);
        Assert.That(middleThreadAfter, Is.Not.Null, "Middle thread should still exist");
        Assert.That(middleThreadAfter!.Posts, Has.Count.EqualTo(2), "Middle thread should still have its OP and reply post");

        // Verify the reply post still exists (only the PostToReply relation should be removed)
        var replyPostAfter = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == replyPostId, cancellationToken);
        Assert.That(replyPostAfter, Is.Not.Null, "Reply post in middle thread should still exist");
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_SetsCorrectIpAddress(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var testIp = "192.168.1.100";
        var request = CreateThreadRequest("b", "New thread", "Hello!", ipAddress: testIp);

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<ThreadPostCreateSuccessResultModel>(), "Expected success result");
        var success = result.AsT0;

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var post = await dbContext.Posts.FirstAsync(p => p.Id == success.PostId, cancellationToken);

        Assert.That(post.UserIpAddress, Is.EqualTo(IPAddress.Parse(testIp).GetAddressBytes()));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_NewThreadHasDefaultFlags(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("b", "Random");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("b", "New thread", "Hello!");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<ThreadPostCreateSuccessResultModel>(), "Expected success result");
        var success = result.AsT0;

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var thread = await dbContext.Threads.FirstAsync(t => t.Id == success.ThreadId, cancellationToken);

        Assert.That(thread.IsPinned, Is.False);
        Assert.That(thread.IsClosed, Is.False);
        Assert.That(thread.IsCyclic, Is.False);
        Assert.That(thread.IsDeleted, Is.False);
    }
}
