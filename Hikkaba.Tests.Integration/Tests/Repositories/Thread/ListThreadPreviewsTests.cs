using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Hikkaba.Tests.Integration.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Thread;

internal sealed class ListThreadPreviewsTests : IntegrationTestBase
{
    private const int PageSize = 5;

    private static readonly OrderByItem[] DefaultOrderBy =
    [
        new() { Field = nameof(ThreadPreviewModel.IsPinned), Direction = OrderByDirection.Desc },
        new() { Field = nameof(ThreadPreviewModel.LastBumpAt), Direction = OrderByDirection.Desc },
        new() { Field = nameof(ThreadPreviewModel.Id), Direction = OrderByDirection.Desc },
    ];

    private static async Task<TestDataBuilder> CreateBaseBuilderAsync(
        IAppScope appScope,
        Action<TestDataBuilder> configure,
        CancellationToken cancellationToken)
    {
        using var seedScope = appScope.ServiceScope.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var builder = new TestDataBuilder(seedScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random");

        configure(builder);
        await builder.SaveAsync(cancellationToken);
        return builder;
    }

    private static async Task<PagedResult<ThreadPreviewModel>> QueryPageAsync(
        IAppScope appScope,
        CancellationToken cancellationToken,
        int pageNumber = 1,
        bool includeDeleted = false)
    {
        using var queryScope = appScope.ServiceScope.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var repository = queryScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        return await repository.ListThreadPreviewsAsync(new ThreadPreviewFilter
        {
            PageNumber = pageNumber,
            PageSize = PageSize,
            OrderBy = DefaultOrderBy,
            CategoryAlias = "b",
            IncludeDeleted = includeDeleted,
        }, cancellationToken);
    }

    #region 1. Pagination: correct thread count on page

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase(3, 3, Description = "3 threads => all 3 shown")]
    [TestCase(5, 5, Description = "5 threads => all 5 shown")]
    [TestCase(6, 5, Description = "6 threads => only first 5 shown")]
    [TestCase(50, 5, Description = "50 threads (10 pages) => only first 5 shown")]
    public async Task ListThreadPreviews_WhenPageSizeIs5_ReturnsAtMost5Threads(
        int totalThreadCount,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                for (var i = 0; i < totalThreadCount; i++)
                {
                    var createdAt = builder.TimeProvider.GetUtcNow().UtcDateTime.AddSeconds(i);
                    builder.WithThreadAndOp($"thread {i}", createdAt: createdAt, lastBumpAt: createdAt);
                }
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(expectedCount));
        Assert.That(result.TotalItemCount, Is.EqualTo(totalThreadCount));
    }

    #endregion

    #region 2. Sorting: threads sorted by LastBumpAt descending

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_Always_ReturnsSortedByLastBumpAtDescending(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                builder
                    .WithThreadAndOp("old thread", createdAt: utcNow.AddDays(-10), lastBumpAt: utcNow.AddDays(-10))
                    .WithThreadAndOp("newest thread", createdAt: utcNow, lastBumpAt: utcNow)
                    .WithThreadAndOp("middle thread", createdAt: utcNow.AddDays(-5), lastBumpAt: utcNow.AddDays(-5));
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(3));
        Assert.That(result.Data[0].Title, Is.EqualTo("newest thread"));
        Assert.That(result.Data[1].Title, Is.EqualTo("middle thread"));
        Assert.That(result.Data[2].Title, Is.EqualTo("old thread"));

        Assert.That(result.Data, Is.Ordered.By(nameof(ThreadPreviewModel.LastBumpAt)).Descending);
    }

    #endregion

    #region 3. Bump limit: posts after bump limit don't affect sorting

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenBumpLimitReached_IgnoresPostsAfterLimitForSorting(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        const int bumpLimit = 3;
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                // Thread 1: has old posts within bump limit, but very fresh posts after limit
                builder
                    .WithThread("thread at bump limit", bumpLimit: bumpLimit, createdAt: utcNow.AddDays(-10), lastBumpAt: utcNow.AddDays(-10))
                    .AddPostsToThread("thread at bump limit", utcNow.AddDays(-10), bumpLimit) // posts within limit (old)
                    .AddPostsToThread("thread at bump limit", utcNow, 5) // posts after limit (very fresh, should be ignored)
                    .UpdateThreadLastBumpAt("thread at bump limit");

                // Thread 2: has fresh posts within bump limit
                builder
                    .WithThread("fresh thread", bumpLimit: bumpLimit, createdAt: utcNow.AddDays(-1), lastBumpAt: utcNow.AddDays(-1))
                    .AddPostsToThread("fresh thread", utcNow.AddDays(-1), bumpLimit)
                    .UpdateThreadLastBumpAt("fresh thread");
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert: "fresh thread" should be first because its last bump (within limit) is newer
        Assert.That(result.Data, Has.Count.EqualTo(2));
        Assert.That(result.Data[0].Title, Is.EqualTo("fresh thread"));
        Assert.That(result.Data[1].Title, Is.EqualTo("thread at bump limit"));
    }

    #endregion

    #region 4. Deleted threads: excluded and don't affect page size

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenIncludeDeletedIsFalse_ExcludesDeletedThreads(CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                // Fresh deleted thread - should be excluded even though it's newest
                builder.WithThreadAndOp("deleted fresh thread", isDeleted: true, createdAt: utcNow, lastBumpAt: utcNow);

                // 5 normal threads
                for (var i = 0; i < 5; i++)
                {
                    var createdAt = utcNow.AddDays(-i - 1);
                    builder.WithThreadAndOp($"normal thread {i}", createdAt: createdAt, lastBumpAt: createdAt);
                }
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert: page is full (5 threads), deleted thread is not present
        Assert.That(result.Data, Has.Count.EqualTo(PageSize));
        Assert.That(result.TotalItemCount, Is.EqualTo(5));
        Assert.That(result.Data.Any(t => t.Title == "deleted fresh thread"), Is.False);
        Assert.That(result.Data, Is.All.Matches<ThreadPreviewModel>(t => !t.IsDeleted));
    }

    #endregion

    #region 5. Pinned threads: always first, not repeated on other pages

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenThreadIsPinned_ReturnsItFirstOnFirstPage(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                // Old pinned thread
                builder.WithThreadAndOp("pinned thread", isPinned: true, createdAt: utcNow.AddYears(-1), lastBumpAt: utcNow.AddYears(-1));

                // Fresh normal threads
                for (var i = 0; i < 5; i++)
                {
                    var createdAt = utcNow.AddDays(-i);
                    builder.WithThreadAndOp($"fresh thread {i}", createdAt: createdAt, lastBumpAt: createdAt);
                }
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert: pinned thread is first despite being oldest
        Assert.That(result.Data, Has.Count.EqualTo(PageSize));
        Assert.That(result.Data[0].Title, Is.EqualTo("pinned thread"));
        Assert.That(result.Data[0].IsPinned, Is.True);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenRequestingSecondPage_DoesNotRepeatPinnedThread(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                // Pinned thread
                builder.WithThreadAndOp("pinned thread", isPinned: true, createdAt: utcNow, lastBumpAt: utcNow);

                // 10 normal threads to fill 2 pages
                for (var i = 0; i < 10; i++)
                {
                    var createdAt = utcNow.AddDays(-i - 1);
                    builder.WithThreadAndOp($"thread {i}", createdAt: createdAt, lastBumpAt: createdAt);
                }
            },
            cancellationToken);

        // Act
        var page2 = await QueryPageAsync(appScope, cancellationToken, pageNumber: 2);

        // Assert: pinned thread is not on second page
        Assert.That(page2.Data, Has.Count.EqualTo(PageSize));
        Assert.That(page2.Data.Any(t => t.Title == "pinned thread"), Is.False);
        Assert.That(page2.Data, Is.All.Matches<ThreadPreviewModel>(t => !t.IsPinned));
    }

    #endregion

    #region 6. Result fields: all fields match expected values

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenQuerying12ThreadsPage2_ReturnsCorrectPagingMetadata(CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                // Create 12 threads to have 3 pages (12/5 = 2.4, ceil = 3)
                for (var i = 0; i < 12; i++)
                {
                    var createdAt = utcNow.AddSeconds(i);
                    builder.WithThreadAndOp($"thread {i}", createdAt: createdAt, lastBumpAt: createdAt);
                }
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken, pageNumber: 2);

        // Assert
        Assert.That(result.PageNumber, Is.EqualTo(2));
        Assert.That(result.PageSize, Is.EqualTo(PageSize));
        Assert.That(result.TotalItemCount, Is.EqualTo(12));
        Assert.That(result.TotalPageCount, Is.EqualTo(3));
        Assert.That(result.SkippedItemCount, Is.EqualTo(PageSize)); // (pageNumber - 1) * pageSize = 5
        Assert.That(result.Data, Has.Count.EqualTo(PageSize));
    }

    #endregion

    #region 7. Thread preview: shows OP + latest posts, correct post count

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase(1, 1, new[] { "OP post" }, Description = "1 post (OP only) => shows 1 post")]
    [TestCase(2, 2, new[] { "OP post", "post 2" }, Description = "2 posts => shows all 2 posts")]
    [TestCase(3, 3, new[] { "OP post", "post 2", "post 3" }, Description = "3 posts => shows all 3 posts")]
    [TestCase(4, 4, new[] { "OP post", "post 2", "post 3", "post 4" }, Description = "4 posts (OP + 3) => shows all 4 posts")]
    [TestCase(5, 4, new[] { "OP post", "post 3", "post 4", "post 5" }, Description = "5 posts => shows OP + 3 latest (skips post 2)")]
    [TestCase(6, 4, new[] { "OP post", "post 4", "post 5", "post 6" }, Description = "6 posts => shows OP + 3 latest (skips posts 2-3)")]
    [TestCase(10, 4, new[] { "OP post", "post 8", "post 9", "post 10" }, Description = "10 posts => shows OP + 3 latest (skips posts 2-7)")]
    public async Task ListThreadPreviews_ReturnsOpPlusLatestPosts(
        int totalPostCount,
        int expectedPostsInPreview,
        string[] expectedPostMessages,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                builder.WithThread("test thread");

                for (var i = 1; i <= totalPostCount; i++)
                {
                    var message = i == 1 ? "OP post" : $"post {i}";
                    builder.WithPost(message, isOriginalPost: i == 1, createdAtOffset: TimeSpan.FromSeconds(i));
                }
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert
        var thread = result.Data[0];
        Assert.That(thread.PostCount, Is.EqualTo(totalPostCount));
        Assert.That(thread.Posts, Has.Count.EqualTo(expectedPostsInPreview));

        // Verify exact posts returned in order
        for (var i = 0; i < expectedPostMessages.Length; i++)
        {
            Assert.That(thread.Posts[i].MessageHtml, Is.EqualTo(expectedPostMessages[i]));
        }

        // Posts are ordered by CreatedAt ascending
        Assert.That(thread.Posts, Is.Ordered.By(nameof(PostDetailsModel.CreatedAt)).Ascending);
    }

    #endregion

    #region 8. Thread local user hash: correctly calculated

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_Always_ReturnsPostsWithCorrectThreadLocalUserHash(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = await CreateBaseBuilderAsync(
            appScope,
            b =>
            {
                b.WithThread("test thread")
                    .WithPost("test post", isOriginalPost: true);
            },
            cancellationToken);

        var thread = builder.GetThread("test thread");
        var userIp = IPAddress.Parse("127.0.0.1").GetAddressBytes();
        var expectedHash = builder.HashService.GetHashBytes(thread.Salt, userIp);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert
        var post = result.Data[0].Posts[0];
        Assert.That(post.ThreadLocalUserHash, Is.EqualTo(expectedHash));
        Assert.That(post.ThreadLocalUserHash.Length, Is.EqualTo(32)); // Blake3 hash length
    }

    #endregion

    #region 9. Category filter: only threads from requested category

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenCategoryAliasSpecified_ReturnsOnlyThreadsFromThatCategory(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("a", "Anime")
            .WithThread("anime thread", createdAt: utcNow, lastBumpAt: utcNow)
            .WithPost("anime OP", isOriginalPost: true)
            .WithCategory("b", "Random")
            .WithThread("random thread 1", createdAt: utcNow.AddDays(-1), lastBumpAt: utcNow.AddDays(-1))
            .WithPost("random OP 1", isOriginalPost: true)
            .WithThread("random thread 2", createdAt: utcNow.AddDays(-2), lastBumpAt: utcNow.AddDays(-2))
            .WithPost("random OP 2", isOriginalPost: true);
        await builder.SaveAsync(cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(2));
        Assert.That(result.TotalItemCount, Is.EqualTo(2));
        Assert.That(result.Data, Is.All.Matches<ThreadPreviewModel>(t => t.CategoryAlias == "b"));
    }

    #endregion

    #region 10. Deleted posts: excluded from preview

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenPostIsDeleted_ExcludesItFromPreview(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                builder
                    .WithThread("test thread")
                    .WithPost("OP post", isOriginalPost: true, createdAtOffset: TimeSpan.FromSeconds(1))
                    .WithPost("normal post", createdAtOffset: TimeSpan.FromSeconds(2))
                    .WithPost("deleted post", isDeleted: true, createdAtOffset: TimeSpan.FromSeconds(3));
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert
        var thread = result.Data[0];
        Assert.That(thread.Posts.Any(p => p.MessageHtml == "deleted post"), Is.False);
        Assert.That(thread.Posts, Is.All.Matches<PostDetailsModel>(p => !p.IsDeleted));
    }

    #endregion

    #region 11. Sage posts: don't affect thread's LastBumpAt

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenPostHasSageEnabled_DoesNotAffectThreadSorting(CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                // Thread with only sage posts - should use original lastBumpAt
                builder
                    .WithThread("all sage thread", createdAt: utcNow.AddDays(-10), lastBumpAt: utcNow.AddDays(-10))
                    .WithPost("OP sage", isOriginalPost: true, createdAtOffset: TimeSpan.Zero)
                    .WithPost("sage 1", isSageEnabled: true, createdAtOffset: TimeSpan.FromDays(5))
                    .WithPost("sage 2", isSageEnabled: true, createdAtOffset: TimeSpan.FromDays(6));

                // Thread with normal bump
                builder
                    .WithThreadAndOp("normal thread", createdAt: utcNow.AddDays(-5), lastBumpAt: utcNow.AddDays(-5));
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert: "normal thread" should be first because its lastBumpAt is newer
        Assert.That(result.Data[0].Title, Is.EqualTo("normal thread"));
        Assert.That(result.Data[1].Title, Is.EqualTo("all sage thread"));
    }

    #endregion

    #region 12. Empty page: returns empty when page number exceeds total

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenPageNumberExceedsTotal_ReturnsEmptyData(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await CreateBaseBuilderAsync(
            appScope,
            builder => builder.WithThreadAndOp("single thread"),
            cancellationToken);

        // Act - request page 10 when only 1 thread exists
        var result = await QueryPageAsync(appScope, cancellationToken, pageNumber: 10);

        // Assert
        Assert.That(result.Data, Is.Empty);
        Assert.That(result.TotalItemCount, Is.EqualTo(1));
        Assert.That(result.TotalPageCount, Is.EqualTo(1));
    }

    #endregion

    #region 13. Attachments: posts with audio and pictures are included

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenPostsHaveAttachments_IncludesThemInPreview(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                builder
                    .WithThread("test thread")
                    .WithPost("OP", isOriginalPost: true, createdAtOffset: TimeSpan.FromSeconds(1))
                    .WithPostWithAudio("audio post", "audio.mp3", createdAtOffset: TimeSpan.FromSeconds(2))
                    .WithPostWithPicture("picture post", "image.jpg", createdAtOffset: TimeSpan.FromSeconds(3));
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert
        var thread = result.Data[0];
        Assert.That(thread.PostCount, Is.EqualTo(3));
        Assert.That(thread.Posts.Any(p => p.MessageHtml == "audio post" && p.Audio.Count(audio => audio.FileExtension == "mp3") == 1), Is.True);
        Assert.That(thread.Posts.Any(p => p.MessageHtml == "picture post" && p.Pictures.Count(pic => pic.FileExtension == "jpg") == 1), Is.True);
    }

    #endregion

    #region 14. Empty threads: threads without posts are excluded

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenThreadHasNoPosts_ExcludesItFromResults(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                // Thread without any posts (empty)
                builder.WithThread("empty thread", createdAt: utcNow, lastBumpAt: utcNow);

                // Thread with posts
                builder.WithThreadAndOp("normal thread", createdAt: utcNow.AddDays(-1), lastBumpAt: utcNow.AddDays(-1));
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert: empty thread should not appear in results
        Assert.That(result.Data, Has.Count.EqualTo(1));
        Assert.That(result.TotalItemCount, Is.EqualTo(1));
        Assert.That(result.Data[0].Title, Is.EqualTo("normal thread"));
        Assert.That(result.Data.Any(t => t.Title == "empty thread"), Is.False);
    }

    #endregion

    #region 15. Threads with only deleted posts: excluded from results

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenThreadHasOnlyDeletedPosts_ExcludesItFromResults(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                // Thread with only deleted posts
                builder
                    .WithThread("all deleted posts thread", createdAt: utcNow, lastBumpAt: utcNow)
                    .WithPost("deleted OP", isOriginalPost: true, isDeleted: true, createdAtOffset: TimeSpan.FromSeconds(1))
                    .WithPost("deleted post 2", isDeleted: true, createdAtOffset: TimeSpan.FromSeconds(2))
                    .WithPost("deleted post 3", isDeleted: true, createdAtOffset: TimeSpan.FromSeconds(3));

                // Thread with normal posts
                builder.WithThreadAndOp("normal thread", createdAt: utcNow.AddDays(-1), lastBumpAt: utcNow.AddDays(-1));
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert: thread with only deleted posts should not appear in results
        Assert.That(result.Data, Has.Count.EqualTo(1));
        Assert.That(result.TotalItemCount, Is.EqualTo(1));
        Assert.That(result.Data[0].Title, Is.EqualTo("normal thread"));
        Assert.That(result.Data.Any(t => t.Title == "all deleted posts thread"), Is.False);
    }

    #endregion

    #region 16. IncludeDeleted: deleted threads appear in chronological order

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenIncludeDeletedIsTrue_ReturnsDeletedThreadsInChronologicalOrder(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                // Deleted thread with newest lastBumpAt
                builder.WithThreadAndOp("deleted newest thread", isDeleted: true, createdAt: utcNow, lastBumpAt: utcNow);

                // Normal thread with middle lastBumpAt
                builder.WithThreadAndOp("normal middle thread", createdAt: utcNow.AddDays(-5), lastBumpAt: utcNow.AddDays(-5));

                // Deleted thread with oldest lastBumpAt
                builder.WithThreadAndOp("deleted oldest thread", isDeleted: true, createdAt: utcNow.AddDays(-10), lastBumpAt: utcNow.AddDays(-10));
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken, includeDeleted: true);

        // Assert: all 3 threads present, sorted by lastBumpAt descending
        Assert.That(result.Data, Has.Count.EqualTo(3));
        Assert.That(result.TotalItemCount, Is.EqualTo(3));
        Assert.That(result.Data[0].Title, Is.EqualTo("deleted newest thread"));
        Assert.That(result.Data[0].IsDeleted, Is.True);
        Assert.That(result.Data[1].Title, Is.EqualTo("normal middle thread"));
        Assert.That(result.Data[1].IsDeleted, Is.False);
        Assert.That(result.Data[2].Title, Is.EqualTo("deleted oldest thread"));
        Assert.That(result.Data[2].IsDeleted, Is.True);

        Assert.That(result.Data, Is.Ordered.By(nameof(ThreadPreviewModel.LastBumpAt)).Descending);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenIncludeDeletedIsTrue_DeletedThreadsAppearOnCorrectPages(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                // Create 7 threads: 3 deleted interleaved with 4 normal
                // Page 1 should have: thread 0 (deleted), thread 1, thread 2 (deleted), thread 3, thread 4 (deleted)
                // Page 2 should have: thread 5, thread 6
                for (var i = 0; i < 7; i++)
                {
                    var createdAt = utcNow.AddDays(-i);
                    var isDeleted = i % 2 == 0 && i < 5; // threads 0, 2, 4 are deleted
                    builder.WithThreadAndOp($"thread {i}", isDeleted: isDeleted, createdAt: createdAt, lastBumpAt: createdAt);
                }
            },
            cancellationToken);

        // Act
        var page1 = await QueryPageAsync(appScope, cancellationToken, includeDeleted: true);
        var page2 = await QueryPageAsync(appScope, cancellationToken, pageNumber: 2, includeDeleted: true);

        // Assert page 1
        Assert.That(page1.Data, Has.Count.EqualTo(PageSize));
        Assert.That(page1.TotalItemCount, Is.EqualTo(7));
        Assert.That(page1.Data[0].Title, Is.EqualTo("thread 0"));
        Assert.That(page1.Data[0].IsDeleted, Is.True);
        Assert.That(page1.Data[1].Title, Is.EqualTo("thread 1"));
        Assert.That(page1.Data[1].IsDeleted, Is.False);
        Assert.That(page1.Data[2].Title, Is.EqualTo("thread 2"));
        Assert.That(page1.Data[2].IsDeleted, Is.True);
        Assert.That(page1.Data[3].Title, Is.EqualTo("thread 3"));
        Assert.That(page1.Data[3].IsDeleted, Is.False);
        Assert.That(page1.Data[4].Title, Is.EqualTo("thread 4"));
        Assert.That(page1.Data[4].IsDeleted, Is.True);

        // Assert page 2
        Assert.That(page2.Data, Has.Count.EqualTo(2));
        Assert.That(page2.Data[0].Title, Is.EqualTo("thread 5"));
        Assert.That(page2.Data[0].IsDeleted, Is.False);
        Assert.That(page2.Data[1].Title, Is.EqualTo("thread 6"));
        Assert.That(page2.Data[1].IsDeleted, Is.False);
    }

    #endregion

    #region 17. IncludeDeleted: deleted posts appear in thread preview

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenIncludeDeletedIsTrue_ReturnsDeletedPostsInPreview(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                builder
                    .WithThread("test thread")
                    .WithPost("OP post", isOriginalPost: true, createdAtOffset: TimeSpan.FromSeconds(1))
                    .WithPost("normal post", createdAtOffset: TimeSpan.FromSeconds(2))
                    .WithPost("deleted post", isDeleted: true, createdAtOffset: TimeSpan.FromSeconds(3))
                    .WithPost("another normal post", createdAtOffset: TimeSpan.FromSeconds(4));
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken, includeDeleted: true);

        // Assert: deleted post is present and in correct chronological position
        var thread = result.Data[0];
        Assert.That(thread.Posts.Any(p => p.MessageHtml == "deleted post"), Is.True);
        Assert.That(thread.Posts.Single(p => p.MessageHtml == "deleted post").IsDeleted, Is.True);

        // Posts are ordered by CreatedAt ascending
        Assert.That(thread.Posts, Is.Ordered.By(nameof(PostDetailsModel.CreatedAt)).Ascending);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenIncludeDeletedIsTrue_DeletedPostsDoNotAffectThreadSorting(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                // Thread with old normal post and very fresh deleted post
                // The deleted post should NOT bump the thread
                builder
                    .WithThread("thread with deleted bump", createdAt: utcNow.AddDays(-10), lastBumpAt: utcNow.AddDays(-10))
                    .WithPost("old OP", isOriginalPost: true, createdAtOffset: TimeSpan.Zero)
                    .WithPost("very fresh deleted post", isDeleted: true, createdAtOffset: TimeSpan.FromDays(10));

                // Thread with recent normal post
                builder.WithThreadAndOp("fresh normal thread", createdAt: utcNow.AddDays(-5), lastBumpAt: utcNow.AddDays(-5));
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken, includeDeleted: true);

        // Assert: fresh normal thread should be first, deleted post doesn't bump thread
        Assert.That(result.Data, Has.Count.EqualTo(2));
        Assert.That(result.Data[0].Title, Is.EqualTo("fresh normal thread"));
        Assert.That(result.Data[1].Title, Is.EqualTo("thread with deleted bump"));

        // Verify the deleted post is present in preview
        var threadWithDeletedPost = result.Data.Single(t => t.Title == "thread with deleted bump");
        Assert.That(threadWithDeletedPost.Posts.Any(p => p.MessageHtml == "very fresh deleted post" && p.IsDeleted), Is.True);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenIncludeDeletedIsTrue_ThreadWithOnlyDeletedPostsAppears(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                // Thread with only deleted posts
                builder
                    .WithThread("all deleted posts thread", createdAt: utcNow, lastBumpAt: utcNow)
                    .WithPost("deleted OP", isOriginalPost: true, isDeleted: true, createdAtOffset: TimeSpan.FromSeconds(1))
                    .WithPost("deleted post 2", isDeleted: true, createdAtOffset: TimeSpan.FromSeconds(2));

                // Normal thread
                builder.WithThreadAndOp("normal thread", createdAt: utcNow.AddDays(-1), lastBumpAt: utcNow.AddDays(-1));
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken, includeDeleted: true);

        // Assert: thread with only deleted posts appears
        Assert.That(result.Data, Has.Count.EqualTo(2));
        Assert.That(result.TotalItemCount, Is.EqualTo(2));
        Assert.That(result.Data.Any(t => t.Title == "all deleted posts thread"), Is.True);

        var threadWithDeletedPosts = result.Data.Single(t => t.Title == "all deleted posts thread");
        Assert.That(threadWithDeletedPosts.Posts, Has.Count.EqualTo(2));
        Assert.That(threadWithDeletedPosts.Posts, Is.All.Matches<PostDetailsModel>(p => p.IsDeleted));
    }

    #endregion

    #region 18. IsClosed: closed threads are returned with correct flag

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenThreadIsClosed_ReturnsIsClosedTrue(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                builder.WithThreadAndOp("closed thread", isClosed: true);
                builder.WithThreadAndOp("open thread", isClosed: false);
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(2));
        var closedThread = result.Data.Single(t => t.Title == "closed thread");
        var openThread = result.Data.Single(t => t.Title == "open thread");
        Assert.That(closedThread.IsClosed, Is.True);
        Assert.That(openThread.IsClosed, Is.False);
    }

    #endregion

    #region 19. IsCyclic: cyclic threads are returned with correct flag

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenThreadIsCyclic_ReturnsIsCyclicTrue(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                builder.WithThreadAndOp("cyclic thread", isCyclic: true);
                builder.WithThreadAndOp("normal thread", isCyclic: false);
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(2));
        var cyclicThread = result.Data.Single(t => t.Title == "cyclic thread");
        var normalThread = result.Data.Single(t => t.Title == "normal thread");
        Assert.That(cyclicThread.IsCyclic, Is.True);
        Assert.That(normalThread.IsCyclic, Is.False);
    }

    #endregion

    #region 20. Multiple pinned threads: sorted by LastBumpAt among themselves

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenMultiplePinnedThreads_SortedByLastBumpAtAmongPinned(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                // Old pinned thread
                builder.WithThreadAndOp("old pinned", isPinned: true, createdAt: utcNow.AddDays(-10), lastBumpAt: utcNow.AddDays(-10));
                // Fresh pinned thread
                builder.WithThreadAndOp("fresh pinned", isPinned: true, createdAt: utcNow, lastBumpAt: utcNow);
                // Middle pinned thread
                builder.WithThreadAndOp("middle pinned", isPinned: true, createdAt: utcNow.AddDays(-5), lastBumpAt: utcNow.AddDays(-5));
                // Normal thread (very fresh but should be after all pinned)
                builder.WithThreadAndOp("fresh normal", isPinned: false, createdAt: utcNow.AddHours(1), lastBumpAt: utcNow.AddHours(1));
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert: pinned threads first (sorted by LastBumpAt desc), then normal threads
        Assert.That(result.Data, Has.Count.EqualTo(4));
        Assert.That(result.Data[0].Title, Is.EqualTo("fresh pinned"));
        Assert.That(result.Data[0].IsPinned, Is.True);
        Assert.That(result.Data[1].Title, Is.EqualTo("middle pinned"));
        Assert.That(result.Data[1].IsPinned, Is.True);
        Assert.That(result.Data[2].Title, Is.EqualTo("old pinned"));
        Assert.That(result.Data[2].IsPinned, Is.True);
        Assert.That(result.Data[3].Title, Is.EqualTo("fresh normal"));
        Assert.That(result.Data[3].IsPinned, Is.False);
    }

    #endregion

    #region 21. Deleted category: threads from deleted category are excluded

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenCategoryIsDeleted_ReturnsNoThreads(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        using var seedScope = appScope.ServiceScope.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var builder = new TestDataBuilder(seedScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random", isDeleted: true);

        builder.WithThreadAndOp("thread in deleted category");
        await builder.SaveAsync(cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert: no threads returned because category is deleted
        Assert.That(result.Data, Is.Empty);
        Assert.That(result.TotalItemCount, Is.EqualTo(0));
    }

    #endregion

    #region 22. Same LastBumpAt: threads sorted by Id descending as fallback

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenThreadsHaveSameLastBumpAt_SortsByIdDescending(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var sameTime = timeProvider.GetUtcNow().UtcDateTime;
        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                // All threads have the same LastBumpAt - should fallback to Id desc
                builder.WithThreadAndOp("thread A", createdAt: sameTime, lastBumpAt: sameTime);
                builder.WithThreadAndOp("thread B", createdAt: sameTime, lastBumpAt: sameTime);
                builder.WithThreadAndOp("thread C", createdAt: sameTime, lastBumpAt: sameTime);
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert: sorted by Id descending (thread C was created last, so has highest Id)
        Assert.That(result.Data, Has.Count.EqualTo(3));
        Assert.That(result.Data, Is.Ordered.By(nameof(ThreadPreviewModel.Id)).Descending);
    }

    #endregion

    #region 23. Empty category: returns empty result

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenCategoryHasNoThreads_ReturnsEmptyResult(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await CreateBaseBuilderAsync(
            appScope,
            _ => { }, // No threads added
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert
        Assert.That(result.Data, Is.Empty);
        Assert.That(result.TotalItemCount, Is.EqualTo(0));
        Assert.That(result.TotalPageCount, Is.EqualTo(0));
    }

    #endregion

    #region 24. BumpLimit fallback: uses category default when thread BumpLimit is 0

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenThreadBumpLimitIsZero_UsesCategoryDefaultBumpLimit(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        using var seedScope = appScope.ServiceScope.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var builder = new TestDataBuilder(seedScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random", defaultBumpLimit: 100);

        builder.WithThreadAndOp("thread with zero bump limit", bumpLimit: 0);
        await builder.SaveAsync(cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert: BumpLimit should fallback to category's default (100)
        Assert.That(result.Data, Has.Count.EqualTo(1));
        Assert.That(result.Data[0].BumpLimit, Is.EqualTo(100));
    }

    #endregion

    #region 25. Thread fields: CreatedAt and ModifiedAt are correctly returned

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_Always_ReturnsCorrectCreatedAtAndModifiedAt(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();

        var createdAt = timeProvider.GetUtcNow().UtcDateTime;
        await CreateBaseBuilderAsync(
            appScope,
            builder => { builder.WithThreadAndOp("test thread", createdAt: createdAt, lastBumpAt: createdAt); },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(1));
        Assert.That(result.Data[0].CreatedAt, Is.EqualTo(createdAt));
        Assert.That(result.Data[0].ModifiedAt, Is.Null); // New thread has no modifications
    }

    #endregion

    #region 26. Category fields: CategoryId, CategoryAlias, CategoryName are correctly returned

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_Always_ReturnsCorrectCategoryFields(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await CreateBaseBuilderAsync(
            appScope,
            builder => { builder.WithThreadAndOp("test thread"); },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(1));
        Assert.That(result.Data[0].CategoryAlias, Is.EqualTo("b"));
        Assert.That(result.Data[0].CategoryName, Is.EqualTo("Random"));
        Assert.That(result.Data[0].CategoryId, Is.GreaterThan(0));
    }

    #endregion

    #region 27. Post fields: UserIpAddress, UserAgent, IsSageEnabled are correctly returned

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_Always_ReturnsCorrectPostFields(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await CreateBaseBuilderAsync(
            appScope,
            builder =>
            {
                builder
                    .WithThread("test thread")
                    .WithPost("OP post", ipAddress: "192.168.1.100", userAgent: "TestBrowser/1.0", isOriginalPost: true)
                    .WithPost("sage post", isSageEnabled: true, createdAtOffset: TimeSpan.FromSeconds(1));
            },
            cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert
        var thread = result.Data[0];
        var opPost = thread.Posts.First(p => p.MessageHtml == "OP post");
        var sagePost = thread.Posts.First(p => p.MessageHtml == "sage post");

        Assert.That(opPost.UserIpAddress, Is.EqualTo(IPAddress.Parse("192.168.1.100").GetAddressBytes()));
        Assert.That(opPost.UserAgent, Is.EqualTo("TestBrowser/1.0"));
        Assert.That(opPost.IsSageEnabled, Is.False);
        Assert.That(sagePost.IsSageEnabled, Is.True);
    }

    #endregion

    #region 28. ShowThreadLocalUserHash: correctly inherited from category

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviews_WhenCategoryHasShowThreadLocalUserHash_ReturnsCorrectFlag(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        using var seedScope = appScope.ServiceScope.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var builder = new TestDataBuilder(seedScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random", showThreadLocalUserHash: true);

        builder.WithThreadAndOp("thread with user hash");
        await builder.SaveAsync(cancellationToken);

        // Act
        var result = await QueryPageAsync(appScope, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(1));
        Assert.That(result.Data[0].ShowThreadLocalUserHash, Is.True);
        Assert.That(result.Data[0].Posts[0].ShowThreadLocalUserHash, Is.True);
    }

    #endregion
}
