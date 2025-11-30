using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Constants;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Hikkaba.Tests.Integration.Extensions;
using Hikkaba.Tests.Integration.Models;
using Hikkaba.Tests.Integration.Services;
using Hikkaba.Tests.Integration.Utils;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories;

[TestFixture]
[Parallelizable(scope: ParallelScope.Fixtures)]
internal sealed class ThreadRepositoryTests
{
    private RespawnableContextManager<ApplicationDbContext>? _contextManager;

    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        _contextManager = await TestDbUtils.CreateNewRandomDbContextManagerAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await _contextManager.StopIfNotNullAsync();
    }

    [MustDisposeResource]
    private async Task<IAppScope> CreateAppScopeAsync(CancellationToken cancellationToken)
    {
        var connectionString = await _contextManager!.CreateRespawnedDbConnectionStringAsync();
        var customAppFactory = new CustomAppFactory(connectionString);

        var scope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if ((await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        return new AppScope
        {
            Scope = scope,
            AppFactory = customAppFactory,
        };
    }

    #region Seed Methods

    private static async Task<(ThreadTestDataBuilder Builder, int PostCount)> SeedOnePageDataAsync(
        IServiceScope scope,
        CancellationToken cancellationToken)
    {
        var builder = new ThreadTestDataBuilder(scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("b", "test thread 1")
            .WithPost("test thread 1", new Guid("545917CA-374F-4C34-80B9-7D8DF0842D72"), "test post 0", isOriginalPost: true, createdAtOffset: TimeSpan.FromSeconds(1))
            .WithPostWithAudio("test thread 1", new Guid("502FACD5-C207-4684-960B-274949E6D043"), "test post 1", new Guid("6D3CD116-6336-47BC-BBE7-5DB289AC6C51"), "Extended electric guitar solo", createdAtOffset: TimeSpan.FromSeconds(2))
            .WithPostWithPicture("test thread 1", new Guid("91F9A825-FFC0-45FA-B8CF-EA0435F414BC"), "test post 2", new Guid("668B2737-0540-4DDD-A23E-58FA031A933F"), "photo_2024-10-31_16-20-39", createdAtOffset: TimeSpan.FromSeconds(3))
            .WithPost("test thread 1", new Guid("BD852887-CBE3-4BAB-9FAC-F501EC3DA439"), "test post 3", createdAtOffset: TimeSpan.FromSeconds(4))
            .WithPost("test thread 1", new Guid("2FA199CC-CD14-402D-8209-0A1B8353E463"), "test post 4", createdAtOffset: TimeSpan.FromSeconds(5))
            .WithPost("test thread 1", new Guid("1F657883-6C50-48FE-982C-5E1B552918D3"), "test post 5", createdAtOffset: TimeSpan.FromSeconds(6));

        await builder.SaveAsync(cancellationToken);
        return (builder, 6);
    }

    private static async Task SeedManyPagesDataAsync(
        IServiceScope scope,
        int totalThreadCount,
        int totalPostCountPerThread,
        CancellationToken cancellationToken)
    {
        var builder = new ThreadTestDataBuilder(scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithCategory("a", "Anime");

        // Deleted thread (should be excluded)
        builder.WithThreadAndPosts("b", "deleted thread", 1, isPinned: true, isClosed: true, isDeleted: true);

        // Thread in another category (should be excluded)
        builder.WithThreadAndPosts("a", "another category thread", 1);

        // Main threads
        builder.WithManyThreadsAndPosts(
            "b",
            totalThreadCount,
            totalPostCountPerThread,
            includeDeletedPost: true,
            threadCreatedAtSelector: i => builder.TimeProvider.GetUtcNow().UtcDateTime.AddSeconds(i));

        await builder.SaveAsync(cancellationToken);
    }

    private static async Task SeedPinnedThreadDataAsync(
        IServiceScope scope,
        int totalThreadCount,
        int totalPostCountPerThread,
        CancellationToken cancellationToken)
    {
        var builder = new ThreadTestDataBuilder(scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithCategory("a", "Anime");

        // Deleted thread
        builder.WithThreadAndPosts("b", "deleted thread", 1, isPinned: true, isClosed: true, isDeleted: true);

        // Thread in another category
        builder.WithThreadAndPosts("a", "another category thread", 1);

        // Main threads with one pinned
        builder.WithManyThreadsAndPosts(
            "b",
            totalThreadCount,
            totalPostCountPerThread,
            includeDeletedPost: true,
            isPinnedSelector: i => i == 3);

        await builder.SaveAsync(cancellationToken);
    }

    private static async Task SeedSagePostDataAsync(
        IServiceScope scope,
        int totalThreadCount,
        int totalPostCountPerThread,
        CancellationToken cancellationToken)
    {
        var builder = new ThreadTestDataBuilder(scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithCategory("a", "Anime");

        // Deleted thread
        builder.WithThreadAndPosts("b", "deleted thread", 1, isPinned: true, isClosed: true, isDeleted: true);

        // Thread in another category
        builder.WithThreadAndPosts("a", "another category thread", 1);

        // Main threads with all sage posts
        for (var i = 0; i < totalThreadCount; i++)
        {
            builder.WithThreadAndPosts(
                "b",
                $"test thread {i}",
                totalPostCountPerThread,
                allPostsSage: true,
                includeDeletedPost: true);
        }

        // Thread with non-sage post that should appear first
        var utcNow = builder.TimeProvider.GetUtcNow().UtcDateTime;
        builder.WithThread("b", "thread with no-sage post", lastBumpAt: utcNow)
            .WithPost(
                "thread with no-sage post",
                new Guid("9BE82B5A-0C4C-475C-9A3B-29F498E079E5"),
                "no-sage post",
                userAgent: "Chrome",
                createdAtOffset: TimeSpan.FromDays(-20));

        await builder.SaveAsync(cancellationToken);
    }

    private static async Task SeedBumpLimitDataAsync(IServiceScope scope, int bumpLimit, CancellationToken cancellationToken)
    {
        var builder = new ThreadTestDataBuilder(scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithCategory("a", "Anime");

        var utcNow = builder.TimeProvider.GetUtcNow().UtcDateTime;

        // Deleted thread
        builder.WithThreadAndPosts("b", "deleted thread", 1, isPinned: true, isClosed: true, isDeleted: true);

        // Thread in another category
        builder.WithThreadAndPosts("a", "another category thread", 1);

        // Threads with bump limit scenarios
        builder
            .WithThread("b", "thread with bump limit 1", bumpLimit: bumpLimit, createdAt: utcNow.AddMinutes(1), lastBumpAt: utcNow.AddMinutes(1))
            .WithThread("b", "thread with bump limit 2", bumpLimit: bumpLimit, createdAt: utcNow.AddMinutes(2), lastBumpAt: utcNow.AddMinutes(2))
            .WithThread("b", "thread with bump limit 3", bumpLimit: bumpLimit, createdAt: utcNow.AddMinutes(3), lastBumpAt: utcNow.AddMinutes(3))
            .WithThread("b", "thread with bump limit 4", bumpLimit: bumpLimit, createdAt: utcNow.AddMinutes(4), lastBumpAt: utcNow.AddMinutes(4));

        // Add posts to deleted thread and another category thread (newest posts, but excluded from query)
        builder
            .AddPostsToThread("deleted thread", utcNow.AddYears(2), bumpLimit + 2)
            .AddPostsToThread("another category thread", utcNow.AddYears(2), bumpLimit + 2);

        // thread1: old posts before bump limit + new posts after bump limit (shouldn't affect result)
        builder
            .AddPostsToThread("thread with bump limit 1", utcNow.AddYears(-1), bumpLimit)
            .AddPostsToThread("thread with bump limit 1", utcNow.AddYears(1), 2)
            .UpdateThreadLastBumpAt("thread with bump limit 1");

        // thread2: several new posts
        builder
            .AddPostsToThread("thread with bump limit 2", utcNow.AddDays(1).AddHours(1), 1, isSageEnabled: true)
            .AddPostsToThread("thread with bump limit 2", utcNow.AddDays(1), bumpLimit)
            .UpdateThreadLastBumpAt("thread with bump limit 2");

        // thread3: even newer posts
        builder
            .AddPostsToThread("thread with bump limit 3", utcNow.AddDays(1).AddHours(3), 1, isSageEnabled: true)
            .AddPostsToThread("thread with bump limit 3", utcNow.AddDays(1).AddSeconds(1), bumpLimit)
            .UpdateThreadLastBumpAt("thread with bump limit 3");

        // thread4: a lot of posts
        builder
            .AddPostsToThread("thread with bump limit 4", utcNow, bumpLimit + 10)
            .AddPostsToThread("thread with bump limit 4", utcNow.AddYears(5), bumpLimit + 10)
            .UpdateThreadLastBumpAt("thread with bump limit 4");

        await builder.SaveAsync(cancellationToken);
    }

    #endregion

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviewsPaginatedAsync_WhenOnePageExists_ReturnsCorrectResult(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var (builder, postCount) = await SeedOnePageDataAsync(appScope.Scope, cancellationToken);

        var thread = builder.GetThread("test thread 1");
        var userIp = System.Net.IPAddress.Parse("127.0.0.1").GetAddressBytes();
        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IThreadRepository>();

        // Act
        var result = await repository.ListThreadPreviewsPaginatedAsync(new ThreadPreviewFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy =
            [
                new OrderByItem { Field = nameof(ThreadPreviewModel.IsPinned), Direction = OrderByDirection.Desc },
                new OrderByItem { Field = nameof(ThreadPreviewModel.LastBumpAt), Direction = OrderByDirection.Desc },
                new OrderByItem { Field = nameof(ThreadPreviewModel.Id), Direction = OrderByDirection.Desc },
            ],
            CategoryAlias = "b",
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data, Has.Count.EqualTo(1));
        Assert.That(result.TotalItemCount, Is.EqualTo(1));
        Assert.That(result.TotalPageCount, Is.EqualTo(1));
        Assert.That(result.PageNumber, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(10));
        Assert.That(result.SkippedItemCount, Is.EqualTo(0));

        var firstThread = result.Data[0];
        Assert.That(firstThread.PostCount, Is.EqualTo(postCount));

        // Only 4 posts are shown (OP + 3 latest)
        Assert.That(firstThread.Posts[0].MessageHtml, Is.EqualTo("test post 0"));
        Assert.That(firstThread.Posts[1].MessageHtml, Is.EqualTo("test post 3"));
        Assert.That(firstThread.Posts[2].MessageHtml, Is.EqualTo("test post 4"));
        Assert.That(firstThread.Posts[3].MessageHtml, Is.EqualTo("test post 5"));

        // All posts have valid hash
        foreach (var actualPost in firstThread.Posts)
        {
            Assert.That(actualPost.ThreadLocalUserHash, Is.EqualTo(builder.HashService.GetHashBytes(thread.Salt, userIp)));
        }
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase(1, 10, 10)]
    [TestCase(2, 10, 10)]
    [TestCase(3, 10, 10)]
    [TestCase(4, 10, 10)]
    [TestCase(5, 10, 10)]
    [TestCase(6, 10, 5)]
    [TestCase(7, 10, 0)]
    [TestCase(1, 100, 55)]
    [TestCase(1, 1, 1)]
    public async Task ListThreadPreviewsPaginatedAsync_WhenManyPagesExist_ReturnsCorrectResult(
        int pageNumber,
        int pageSize,
        int expectedThreadCount,
        CancellationToken cancellationToken)
    {
        const int totalThreadCount = 55;
        const int totalPostCountPerThread = 55;

        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        using (var seedScope = appScope.Scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            await SeedManyPagesDataAsync(seedScope, totalThreadCount, totalPostCountPerThread, cancellationToken);
        }

        // Act
        using (var actScope = appScope.Scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var threadRepository = actScope.ServiceProvider.GetRequiredService<IThreadRepository>();

            var actualThreadPreviews = await threadRepository.ListThreadPreviewsPaginatedAsync(new ThreadPreviewFilter
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                OrderBy =
                [
                    new OrderByItem { Field = nameof(ThreadPreviewModel.IsPinned), Direction = OrderByDirection.Desc },
                    new OrderByItem { Field = nameof(ThreadPreviewModel.LastBumpAt), Direction = OrderByDirection.Desc },
                    new OrderByItem { Field = nameof(ThreadPreviewModel.Id), Direction = OrderByDirection.Desc },
                ],
                CategoryAlias = "b",
                IncludeDeleted = false,
            }, cancellationToken);

            // Assert
            Assert.That(actualThreadPreviews, Is.Not.Null);
            Assert.That(actualThreadPreviews.Data, Has.Count.EqualTo(expectedThreadCount));
            Assert.That(actualThreadPreviews.TotalItemCount, Is.EqualTo(totalThreadCount));
            Assert.That(actualThreadPreviews.TotalPageCount, Is.EqualTo(Math.Ceiling((decimal)totalThreadCount / pageSize)));
            Assert.That(actualThreadPreviews.PageNumber, Is.EqualTo(pageNumber));
            Assert.That(actualThreadPreviews.PageSize, Is.EqualTo(pageSize));
            Assert.That(actualThreadPreviews.SkippedItemCount, Is.EqualTo((pageNumber - 1) * pageSize));

            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => x is { CategoryAlias: "b", IsDeleted: false }
                                                                                           && x.Posts.All(p => !p.IsDeleted)));

            Assert.That(actualThreadPreviews.Data, Is.Ordered
                .By(nameof(ThreadPreviewModel.IsPinned)).Descending
                .Then.By(nameof(ThreadPreviewModel.LastBumpAt)).Descending);

            foreach (var thread in actualThreadPreviews.Data)
            {
                Assert.That(thread.PostCount, Is.EqualTo(totalPostCountPerThread));
                Assert.That(thread.Posts, Has.Count.EqualTo(Defaults.LatestPostsCountInThreadPreview + 1));
                Assert.That(thread.Posts, Is.Ordered.By(nameof(PostDetailsModel.CreatedAt)).Ascending);
                Assert.That(thread.Posts, Is.All.Matches<PostDetailsModel>(x => x.ThreadLocalUserHash.Length == 32));
            }
        }
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase(1, 10, 10)]
    [TestCase(2, 10, 10)]
    [TestCase(3, 10, 5)]
    [TestCase(4, 10, 0)]
    [TestCase(1, 100, 25)]
    [TestCase(1, 1, 1)]
    public async Task ListThreadPreviewsPaginatedAsync_WhenPinnedThreadExist_ReturnsCorrectResult(
        int pageNumber,
        int pageSize,
        int expectedThreadCount,
        CancellationToken cancellationToken)
    {
        const int totalThreadCount = 25;
        const int totalPostCountPerThread = 25;

        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        using (var seedScope = appScope.Scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            await SeedPinnedThreadDataAsync(seedScope, totalThreadCount, totalPostCountPerThread, cancellationToken);
        }

        // Act
        using (var actScope = appScope.Scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var threadRepository = actScope.ServiceProvider.GetRequiredService<IThreadRepository>();

            var actualThreadPreviews = await threadRepository.ListThreadPreviewsPaginatedAsync(new ThreadPreviewFilter
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                OrderBy =
                [
                    new OrderByItem { Field = nameof(ThreadPreviewModel.IsPinned), Direction = OrderByDirection.Desc },
                    new OrderByItem { Field = nameof(ThreadPreviewModel.LastBumpAt), Direction = OrderByDirection.Desc },
                    new OrderByItem { Field = nameof(ThreadPreviewModel.Id), Direction = OrderByDirection.Desc },
                ],
                CategoryAlias = "b",
                IncludeDeleted = false,
            }, cancellationToken);

            // Assert
            Assert.That(actualThreadPreviews, Is.Not.Null);
            Assert.That(actualThreadPreviews.Data, Has.Count.EqualTo(expectedThreadCount));
            Assert.That(actualThreadPreviews.TotalItemCount, Is.EqualTo(totalThreadCount));
            Assert.That(actualThreadPreviews.TotalPageCount, Is.EqualTo(Math.Ceiling((decimal)totalThreadCount / pageSize)));
            Assert.That(actualThreadPreviews.PageNumber, Is.EqualTo(pageNumber));
            Assert.That(actualThreadPreviews.PageSize, Is.EqualTo(pageSize));
            Assert.That(actualThreadPreviews.SkippedItemCount, Is.EqualTo((pageNumber - 1) * pageSize));

            if (pageNumber == 1)
            {
                Assert.That(actualThreadPreviews.Data[0].IsPinned, Is.True);
            }

            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => x.CategoryAlias == "b"));
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => !x.IsDeleted));
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => x.Posts.All(p => !p.IsDeleted)));

            Assert.That(actualThreadPreviews.Data, Is.Ordered
                .By(nameof(ThreadPreviewModel.IsPinned)).Descending
                .Then.By(nameof(ThreadPreviewModel.LastBumpAt)).Descending);

            foreach (var thread in actualThreadPreviews.Data)
            {
                Assert.That(thread.PostCount, Is.EqualTo(totalPostCountPerThread));
                Assert.That(thread.Posts, Has.Count.EqualTo(Defaults.LatestPostsCountInThreadPreview + 1));
                Assert.That(thread.Posts, Is.Ordered.By(nameof(PostDetailsModel.CreatedAt)).Ascending);
            }
        }
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase(1, 10, 10)]
    [TestCase(2, 10, 10)]
    [TestCase(3, 10, 10)]
    [TestCase(4, 10, 10)]
    [TestCase(5, 10, 10)]
    [TestCase(6, 10, 5)]
    [TestCase(7, 10, 0)]
    [TestCase(1, 100, 55)]
    [TestCase(1, 1, 1)]
    public async Task ListThreadPreviewsPaginatedAsync_WhenSagePostExist_ReturnsCorrectResult(
        int pageNumber,
        int pageSize,
        int expectedThreadCount,
        CancellationToken cancellationToken)
    {
        const int totalThreadCount = 25;
        const int totalPostCountPerThread = 25;

        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        using (var seedScope = appScope.Scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            await SeedSagePostDataAsync(seedScope, totalThreadCount, totalPostCountPerThread, cancellationToken);
        }

        // Act
        using (var actScope = appScope.Scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var threadRepository = actScope.ServiceProvider.GetRequiredService<IThreadRepository>();

            var actualThreadPreviews = await threadRepository.ListThreadPreviewsPaginatedAsync(new ThreadPreviewFilter
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                OrderBy =
                [
                    new OrderByItem { Field = nameof(ThreadPreviewModel.IsPinned), Direction = OrderByDirection.Desc },
                    new OrderByItem { Field = nameof(ThreadPreviewModel.LastBumpAt), Direction = OrderByDirection.Desc },
                    new OrderByItem { Field = nameof(ThreadPreviewModel.Id), Direction = OrderByDirection.Desc },
                ],
                CategoryAlias = "b",
                IncludeDeleted = false,
            }, cancellationToken);

            // Assert
            Assert.That(actualThreadPreviews, Is.Not.Null);

            if (pageNumber == 1)
            {
                Assert.That(actualThreadPreviews.Data[0].Title, Is.EqualTo("thread with no-sage post"));
            }

            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => x.CategoryAlias == "b"));
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => !x.IsDeleted));
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => x.Posts.All(p => !p.IsDeleted)));

            Assert.That(actualThreadPreviews.Data, Is.Ordered
                .By(nameof(ThreadPreviewModel.IsPinned)).Descending
                .Then.By(nameof(ThreadPreviewModel.LastBumpAt)).Descending);

            foreach (var thread in actualThreadPreviews.Data)
            {
                Assert.That(thread.Posts, Is.Ordered.By(nameof(PostDetailsModel.CreatedAt)).Ascending);
            }
        }
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviewsPaginatedAsync_WhenBumpLimitReached_ReturnsCorrectResult(CancellationToken cancellationToken)
    {
        const int bumpLimit = 5;
        const int pageNumber = 1;
        const int pageSize = 10;

        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        using (var seedScope = appScope.Scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            await SeedBumpLimitDataAsync(seedScope, bumpLimit, cancellationToken);
        }

        // Act
        using (var actScope = appScope.Scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var threadRepository = actScope.ServiceProvider.GetRequiredService<IThreadRepository>();

            var actualThreadPreviews = await threadRepository.ListThreadPreviewsPaginatedAsync(new ThreadPreviewFilter
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                OrderBy =
                [
                    new OrderByItem { Field = nameof(ThreadPreviewModel.IsPinned), Direction = OrderByDirection.Desc },
                    new OrderByItem { Field = nameof(ThreadPreviewModel.LastBumpAt), Direction = OrderByDirection.Desc },
                    new OrderByItem { Field = nameof(ThreadPreviewModel.Id), Direction = OrderByDirection.Desc },
                ],
                CategoryAlias = "b",
                IncludeDeleted = false,
            }, cancellationToken);

            // Assert
            Assert.That(actualThreadPreviews, Is.Not.Null);
            Assert.That(actualThreadPreviews.Data, Has.Count.EqualTo(4));

            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => x.CategoryAlias == "b"));
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => !x.IsDeleted));
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => x.Posts.All(p => !p.IsDeleted)));

            Assert.That(actualThreadPreviews.Data, Is.Ordered
                .By(nameof(ThreadPreviewModel.IsPinned)).Descending
                .Then.By(nameof(ThreadPreviewModel.LastBumpAt)).Descending);

            foreach (var thread in actualThreadPreviews.Data)
            {
                Assert.That(thread.Posts, Is.Ordered.By(nameof(PostDetailsModel.CreatedAt)).Ascending);
            }
        }
    }
}
