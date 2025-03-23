using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blake3;
using Hikkaba.Common.Constants;
using Hikkaba.Common.Enums;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Entities.Attachments;
using Hikkaba.Data.Entities.Attachments.Base;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Repositories.Contracts;
using Hikkaba.Repositories.Implementations;
using Hikkaba.Tests.Integration.Constants;
using Hikkaba.Tests.Integration.Extensions;
using Hikkaba.Tests.Integration.Services;
using Hikkaba.Tests.Integration.Utils;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Thread = Hikkaba.Data.Entities.Thread;

namespace Hikkaba.Tests.Integration.Tests;

[TestFixture]
[Parallelizable(scope: ParallelScope.Fixtures)]
public sealed class ThreadRepositoryTests
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
    private async Task<CustomAppFactory> CreateAppFactoryAsync()
    {
        var connectionString = await _contextManager!.CreateRespawnedDbConnectionStringAsync();
        return new CustomAppFactory(connectionString);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviewsPaginatedAsync_WhenOnePageExists_ReturnsCorrectResult(
        CancellationToken cancellationToken)
    {
        // Arrange
        await using var customAppFactory = await CreateAppFactoryAsync();
        using var scope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var timeProvider = customAppFactory.Services.GetRequiredService<TimeProvider>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if ((await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        // Seed
        var admin = new ApplicationUser
        {
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
        };
        dbContext.Users.Add(admin);

        var board = new Board
        {
            Name = "Test Board Fizz",
        };
        dbContext.Boards.Add(board);

        var category = new Category
        {
            IsDeleted = false,
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            ModifiedAt = null,
            Alias = "b",
            Name = "Random Foo",
            IsHidden = false,
            DefaultBumpLimit = 500,
            DefaultShowThreadLocalUserHash = false,
            Board = board,
            CreatedBy = admin,
        };
        dbContext.Categories.Add(category);

        var thread = new Thread
        {
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            Title = "test thread 1 Buzz",
            IsPinned = false,
            IsClosed = false,
            BumpLimit = 500,
            ShowThreadLocalUserHash = false,
            Category = category,
            CreatedBy = null,
        };
        dbContext.Threads.Add(thread);

        var post0 = new Post
        {
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime.AddSeconds(1),
            IsSageEnabled = false,
            MessageText = "test post 0",
            MessageHtml = "test post 0",
            UserIpAddress = IPAddress.Parse("127.0.0.1").GetAddressBytes(),
            UserAgent = "Firefox",
            Thread = thread,
        };
        var post1 = new Post
        {
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime.AddSeconds(2),
            IsSageEnabled = false,
            MessageText = "test post 1",
            MessageHtml = "test post 1",
            UserIpAddress = IPAddress.Parse("127.0.0.1").GetAddressBytes(),
            UserAgent = "Chrome",
            Thread = thread,
            Attachments = new List<Attachment>
            {
                new Audio
                {
                    FileName = "Extended electric guitar solo",
                    FileExtension = "mp3",
                    FileSize = 3671469,
                    FileHash = Hasher.Hash("f61d4fbb-4cbd-4d4e-8df1-6c22c58de9cf"u8).AsSpan().ToArray(),
                },
            },
        };
        var post2 = new Post
        {
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime.AddSeconds(3),
            IsSageEnabled = false,
            MessageText = "test post 2",
            MessageHtml = "test post 2",
            UserIpAddress = IPAddress.Parse("127.0.0.1").GetAddressBytes(),
            UserAgent = "Chrome",
            Thread = thread,
            Attachments = new List<Attachment>
            {
                new Picture
                {
                    FileName = "photo_2024-10-31_16-20-39",
                    FileExtension = "jpg",
                    FileSize = 204316,
                    FileHash = Hasher.Hash("6e84e6b4-5370-44c6-a319-a03a027f3905"u8).AsSpan().ToArray(),
                    Width = 1280,
                    Height = 960,
                },
            },
        };
        var post3 = new Post
        {
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime.AddSeconds(4),
            IsSageEnabled = false,
            MessageText = "test post 3",
            MessageHtml = "test post 3",
            UserIpAddress = IPAddress.Parse("127.0.0.1").GetAddressBytes(),
            UserAgent = "Chrome",
            Thread = thread,
        };
        var post4 = new Post
        {
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime.AddSeconds(5),
            IsSageEnabled = false,
            MessageText = "test post 4",
            MessageHtml = "test post 4",
            UserIpAddress = IPAddress.Parse("127.0.0.1").GetAddressBytes(),
            UserAgent = "Chrome",
            Thread = thread,
        };
        var post5 = new Post
        {
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime.AddSeconds(6),
            IsSageEnabled = false,
            MessageText = "test post 5",
            MessageHtml = "test post 5",
            UserIpAddress = IPAddress.Parse("127.0.0.1").GetAddressBytes(),
            UserAgent = "Chrome",
            Thread = thread,
        };
        List<Post> allPosts = [post0, post1, post2, post3, post4, post5];
        dbContext.AddRange(allPosts);

        await dbContext.SaveChangesAsync(cancellationToken);

        var repository = new ThreadRepository(dbContext);

        // Act
        var result = await repository.ListThreadPreviewsPaginatedAsync(new ThreadPreviewsFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [
                new OrderByItem { Field = nameof(ThreadPreviewSm.IsPinned), Direction = OrderByDirection.Desc },
                new OrderByItem { Field = nameof(ThreadPreviewSm.LastPostCreatedAt), Direction = OrderByDirection.Desc },
                new OrderByItem { Field = nameof(ThreadPreviewSm.Id), Direction = OrderByDirection.Desc },
            ],
            CategoryAlias = category.Alias,
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);

        // 1 thread shown
        Assert.That(result.Data, Has.Count.EqualTo(1));

        // 1 thread found
        Assert.That(result.TotalItemCount, Is.EqualTo(1));

        Assert.That(result.TotalPageCount, Is.EqualTo(1));
        Assert.That(result.PageNumber, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(10));
        Assert.That(result.SkippedItemCount, Is.EqualTo(0));

        // thread has 6 posts
        var firstThread = result.Data[0];
        Assert.That(firstThread.PostCount, Is.EqualTo(allPosts.Count));

        var actualPost0 = firstThread.Posts[0];
        var actualPost1 = firstThread.Posts[1];
        var actualPost2 = firstThread.Posts[2];
        var actualPost3 = firstThread.Posts[3];

        // only 4 posts are shown
        Assert.That(actualPost0.MessageHtml, Is.EqualTo(post0.MessageHtml));
        Assert.That(actualPost1.MessageHtml, Is.EqualTo(post3.MessageHtml));
        Assert.That(actualPost2.MessageHtml, Is.EqualTo(post4.MessageHtml));
        Assert.That(actualPost3.MessageHtml, Is.EqualTo(post5.MessageHtml));
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
        await using var customAppFactory = await CreateAppFactoryAsync();
        using (var seedScope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var seedTimeProvider = customAppFactory.Services.GetRequiredService<TimeProvider>();
            var seedDbContext = seedScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if ((await seedDbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            {
                await seedDbContext.Database.MigrateAsync(cancellationToken);
            }

            // Seed
            var admin = new ApplicationUser
            {
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
            };
            seedDbContext.Users.Add(admin);

            var board = new Board
            {
                Name = "Test Board Fizz",
            };
            seedDbContext.Boards.Add(board);

            var category1 = new Category
            {
                IsDeleted = false,
                CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                ModifiedAt = null,
                Alias = "b",
                Name = "Random Foo",
                IsHidden = false,
                DefaultBumpLimit = 500,
                DefaultShowThreadLocalUserHash = false,
                Board = board,
                CreatedBy = admin,
            };
            var category2 = new Category
            {
                IsDeleted = false,
                CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                ModifiedAt = null,
                Alias = "a",
                Name = "Random Bar",
                IsHidden = false,
                DefaultBumpLimit = 500,
                DefaultShowThreadLocalUserHash = false,
                Board = board,
                CreatedBy = admin,
            };
            seedDbContext.Categories.AddRange(category1, category2);

            var deletedThread = new Thread
            {
                CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                Title = "deleted thread",
                IsPinned = true,
                IsClosed = true,
                IsDeleted = true,
                BumpLimit = 500,
                ShowThreadLocalUserHash = false,
                Category = category1,
                CreatedBy = null,
                Posts = [new Post
                {
                    CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                    IsSageEnabled = false,
                    IsDeleted = false,
                    MessageText = $"test post in deleted thread",
                    MessageHtml = $"test post in deleted thread",
                    UserIpAddress = IPAddress.Parse($"127.0.0.1").GetAddressBytes(),
                    UserAgent = "Firefox",
                }],
            };
            var anotherCategoryThread = new Thread
            {
                CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                Title = "another category thread",
                IsPinned = false,
                IsClosed = false,
                IsDeleted = false,
                BumpLimit = 500,
                ShowThreadLocalUserHash = false,
                Category = category2,
                CreatedBy = null,
                Posts = [new Post
                {
                    CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                    IsSageEnabled = false,
                    IsDeleted = false,
                    MessageText = $"test post in deleted thread",
                    MessageHtml = $"test post in deleted thread",
                    UserIpAddress = IPAddress.Parse($"127.0.0.1").GetAddressBytes(),
                    UserAgent = "Firefox",
                }],
            };

            var threads = Enumerable
                .Range(0, totalThreadCount)
                .Select(i => new Thread
                {
                    CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime.AddSeconds(i),
                    Title = $"test thread {i}",
                    IsPinned = false,
                    IsClosed = false,
                    IsDeleted = false,
                    BumpLimit = 500,
                    ShowThreadLocalUserHash = false,
                    Category = category1,
                    CreatedBy = null,
                })
                .Union([deletedThread, anotherCategoryThread])
                .ToList();
            seedDbContext.Threads.AddRange(threads);

            var posts = threads.SelectMany(t => Enumerable
                    .Range(0, totalPostCountPerThread)
                    .Select(i => new Post
                    {
                        CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime.AddMinutes(i),
                        IsSageEnabled = i % 2 == 0,
                        IsDeleted = false,
                        MessageText = $"test post {i}",
                        MessageHtml = $"test post {i}",
                        UserIpAddress = IPAddress.Parse($"127.0.0.{i}").GetAddressBytes(),
                        UserAgent = "Firefox",
                        Thread = t,
                    })
                    .Union([new Post
                    {
                        CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime.AddYears(1),
                        IsSageEnabled = false,
                        IsDeleted = true,
                        MessageText = $"deleted post",
                        MessageHtml = $"deleted post",
                        UserIpAddress = IPAddress.Parse($"127.0.0.1").GetAddressBytes(),
                        UserAgent = "Firefox",
                        Thread = t,
                    }])
                    .ToList())
                .ToList();
            seedDbContext.Posts.AddRange(posts);

            await seedDbContext.SaveChangesAsync(cancellationToken);
        }

        // Act
        using (var actScope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var threadRepository = actScope.ServiceProvider.GetRequiredService<IThreadRepository>();
            var actualThreadPreviews = await threadRepository.ListThreadPreviewsPaginatedAsync(new ThreadPreviewsFilter
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                OrderBy = [
                    new OrderByItem { Field = nameof(ThreadPreviewSm.IsPinned), Direction = OrderByDirection.Desc },
                    new OrderByItem { Field = nameof(ThreadPreviewSm.LastPostCreatedAt), Direction = OrderByDirection.Desc },
                    new OrderByItem { Field = nameof(ThreadPreviewSm.Id), Direction = OrderByDirection.Desc },
                ],
                CategoryAlias = "b",
                IncludeDeleted = false,
            }, cancellationToken);

            // Assert
            Assert.That(actualThreadPreviews, Is.Not.Null);

            // N threads shown (except deleted)
            Assert.That(actualThreadPreviews.Data, Has.Count.EqualTo(expectedThreadCount));

            // All threads found (except deleted)
            Assert.That(actualThreadPreviews.TotalItemCount, Is.EqualTo(totalThreadCount));

            Assert.That(actualThreadPreviews.TotalPageCount, Is.EqualTo(Math.Ceiling((decimal)totalThreadCount / pageSize)));
            Assert.That(actualThreadPreviews.PageNumber, Is.EqualTo(pageNumber));
            Assert.That(actualThreadPreviews.PageSize, Is.EqualTo(pageSize));
            Assert.That(actualThreadPreviews.SkippedItemCount, Is.EqualTo((pageNumber - 1) * pageSize));

            // check that category is correct
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewSm>(x => x.CategoryAlias == "b"));

            // check that there are no deleted threads
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewSm>(x => x.IsDeleted == false));

            // check that there are no deleted posts
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewSm>(x => x.Posts.All(p => p.IsDeleted == false)));

            // check that every next thread updated earlier than the previous one (sort by LastPostCreatedAt desc)
            Assert.That(actualThreadPreviews.Data, Is.Ordered
                .By(nameof(ThreadPreviewSm.IsPinned))
                .Descending
                .Then
                .By(nameof(ThreadPreviewSm.LastPostCreatedAt))
                .Descending);

            foreach (var thread in actualThreadPreviews.Data)
            {
                // Every thread has M posts
                Assert.That(thread.PostCount, Is.EqualTo(totalPostCountPerThread));

                // But only K posts are shown (OP post + latest posts)
                Assert.That(thread.Posts, Has.Count.EqualTo(Defaults.LatestPostsCountInThreadPreview + 1));

                // check that posts are sorted by date ascending
                Assert.That(thread.Posts, Is.Ordered.By(nameof(PostPreviewDto.CreatedAt)).Ascending);
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
        await using var customAppFactory = await CreateAppFactoryAsync();
        using (var seedScope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var seedTimeProvider = customAppFactory.Services.GetRequiredService<TimeProvider>();
            var seedDbContext = seedScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if ((await seedDbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            {
                await seedDbContext.Database.MigrateAsync(cancellationToken);
            }

            // Seed
            var admin = new ApplicationUser
            {
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
            };
            seedDbContext.Users.Add(admin);

            var board = new Board
            {
                Name = "Test Board Fizz",
            };
            seedDbContext.Boards.Add(board);

            var category1 = new Category
            {
                IsDeleted = false,
                CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                ModifiedAt = null,
                Alias = "b",
                Name = "Random Foo",
                IsHidden = false,
                DefaultBumpLimit = 500,
                DefaultShowThreadLocalUserHash = false,
                Board = board,
                CreatedBy = admin,
            };
            var category2 = new Category
            {
                IsDeleted = false,
                CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                ModifiedAt = null,
                Alias = "a",
                Name = "Random Bar",
                IsHidden = false,
                DefaultBumpLimit = 500,
                DefaultShowThreadLocalUserHash = false,
                Board = board,
                CreatedBy = admin,
            };
            seedDbContext.Categories.AddRange(category1, category2);

            var deletedThread = new Thread
            {
                CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                Title = "deleted thread",
                IsPinned = true,
                IsClosed = true,
                IsDeleted = true,
                BumpLimit = 500,
                ShowThreadLocalUserHash = false,
                Category = category1,
                CreatedBy = null,
                Posts = [new Post
                {
                    CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                    IsSageEnabled = false,
                    IsDeleted = false,
                    MessageText = $"test post in deleted thread",
                    MessageHtml = $"test post in deleted thread",
                    UserIpAddress = IPAddress.Parse($"127.0.0.1").GetAddressBytes(),
                    UserAgent = "Firefox",
                }],
            };
            var anotherCategoryThread = new Thread
            {
                CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                Title = "another category thread",
                IsPinned = false,
                IsClosed = false,
                IsDeleted = false,
                BumpLimit = 500,
                ShowThreadLocalUserHash = false,
                Category = category2,
                CreatedBy = null,
                Posts = [new Post
                {
                    CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                    IsSageEnabled = false,
                    IsDeleted = false,
                    MessageText = $"test post in deleted thread",
                    MessageHtml = $"test post in deleted thread",
                    UserIpAddress = IPAddress.Parse($"127.0.0.1").GetAddressBytes(),
                    UserAgent = "Firefox",
                }],
            };

            var threads = Enumerable
                .Range(0, totalThreadCount)
                .Select(i => new Thread
                {
                    CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime.AddSeconds(i),
                    Title = $"test thread {i}",
                    IsPinned = i == 3,
                    IsClosed = false,
                    IsDeleted = false,
                    BumpLimit = 500,
                    ShowThreadLocalUserHash = false,
                    Category = category1,
                    CreatedBy = null,
                })
                .Union([deletedThread, anotherCategoryThread])
                .ToList();
            seedDbContext.Threads.AddRange(threads);

            var posts = threads.SelectMany(t => Enumerable
                    .Range(0, totalPostCountPerThread)
                    .Select(i => new Post
                    {
                        CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime.AddMinutes(i),
                        IsSageEnabled = i % 2 == 0,
                        IsDeleted = false,
                        MessageText = $"test post {i}",
                        MessageHtml = $"test post {i}",
                        UserIpAddress = IPAddress.Parse($"127.0.0.{i}").GetAddressBytes(),
                        UserAgent = "Firefox",
                        Thread = t,
                    })
                    .Union([new Post
                    {
                        CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime.AddYears(1),
                        IsSageEnabled = false,
                        IsDeleted = true,
                        MessageText = $"deleted post",
                        MessageHtml = $"deleted post",
                        UserIpAddress = IPAddress.Parse($"127.0.0.1").GetAddressBytes(),
                        UserAgent = "Firefox",
                        Thread = t,
                    }])
                    .ToList())
                .ToList();
            seedDbContext.Posts.AddRange(posts);

            await seedDbContext.SaveChangesAsync(cancellationToken);
        }

        // Act
        using (var actScope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var threadRepository = actScope.ServiceProvider.GetRequiredService<IThreadRepository>();
            var actualThreadPreviews = await threadRepository.ListThreadPreviewsPaginatedAsync(new ThreadPreviewsFilter
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                OrderBy = [
                    new OrderByItem { Field = nameof(ThreadPreviewSm.IsPinned), Direction = OrderByDirection.Desc },
                    new OrderByItem { Field = nameof(ThreadPreviewSm.LastPostCreatedAt), Direction = OrderByDirection.Desc },
                    new OrderByItem { Field = nameof(ThreadPreviewSm.Id), Direction = OrderByDirection.Desc },
                ],
                CategoryAlias = "b",
                IncludeDeleted = false,
            }, cancellationToken);

            // Assert
            Assert.That(actualThreadPreviews, Is.Not.Null);

            // N threads shown (except deleted)
            Assert.That(actualThreadPreviews.Data, Has.Count.EqualTo(expectedThreadCount));

            // All threads found (except deleted)
            Assert.That(actualThreadPreviews.TotalItemCount, Is.EqualTo(totalThreadCount));

            Assert.That(actualThreadPreviews.TotalPageCount, Is.EqualTo(Math.Ceiling((decimal)totalThreadCount / pageSize)));
            Assert.That(actualThreadPreviews.PageNumber, Is.EqualTo(pageNumber));
            Assert.That(actualThreadPreviews.PageSize, Is.EqualTo(pageSize));
            Assert.That(actualThreadPreviews.SkippedItemCount, Is.EqualTo((pageNumber - 1) * pageSize));

            if (pageNumber == 1)
            {
                Assert.That(actualThreadPreviews.Data[0].IsPinned, Is.True);
            }

            // check that category is correct
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewSm>(x => x.CategoryAlias == "b"));

            // check that there are no deleted threads
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewSm>(x => x.IsDeleted == false));

            // check that there are no deleted posts
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewSm>(x => x.Posts.All(p => p.IsDeleted == false)));

            // check that every next thread updated earlier than the previous one (sort by LastPostCreatedAt desc)
            Assert.That(actualThreadPreviews.Data, Is.Ordered
                .By(nameof(ThreadPreviewSm.IsPinned))
                .Descending
                .Then
                .By(nameof(ThreadPreviewSm.LastPostCreatedAt))
                .Descending);

            foreach (var thread in actualThreadPreviews.Data)
            {
                // Every thread has M posts
                Assert.That(thread.PostCount, Is.EqualTo(totalPostCountPerThread));

                // But only K posts are shown (OP post + latest posts)
                Assert.That(thread.Posts, Has.Count.EqualTo(Defaults.LatestPostsCountInThreadPreview + 1));

                // check that posts are sorted by date ascending
                Assert.That(thread.Posts, Is.Ordered.By(nameof(PostPreviewDto.CreatedAt)).Ascending);
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
        await using var customAppFactory = await CreateAppFactoryAsync();
        using (var seedScope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var seedTimeProvider = customAppFactory.Services.GetRequiredService<TimeProvider>();
            var seedDbContext = seedScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if ((await seedDbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            {
                await seedDbContext.Database.MigrateAsync(cancellationToken);
            }

            // Seed
            var admin = new ApplicationUser
            {
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
            };
            seedDbContext.Users.Add(admin);

            var board = new Board
            {
                Name = "Test Board Fizz",
            };
            seedDbContext.Boards.Add(board);

            var category1 = new Category
            {
                IsDeleted = false,
                CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                ModifiedAt = null,
                Alias = "b",
                Name = "Random Foo",
                IsHidden = false,
                DefaultBumpLimit = 500,
                DefaultShowThreadLocalUserHash = false,
                Board = board,
                CreatedBy = admin,
            };
            var category2 = new Category
            {
                IsDeleted = false,
                CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                ModifiedAt = null,
                Alias = "a",
                Name = "Random Bar",
                IsHidden = false,
                DefaultBumpLimit = 500,
                DefaultShowThreadLocalUserHash = false,
                Board = board,
                CreatedBy = admin,
            };
            seedDbContext.Categories.AddRange(category1, category2);

            var deletedThread = new Thread
            {
                CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                Title = "deleted thread",
                IsPinned = true,
                IsClosed = true,
                IsDeleted = true,
                BumpLimit = 500,
                ShowThreadLocalUserHash = false,
                Category = category1,
                CreatedBy = null,
                Posts = [new Post
                {
                    CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                    IsSageEnabled = false,
                    IsDeleted = false,
                    MessageText = $"test post in deleted thread",
                    MessageHtml = $"test post in deleted thread",
                    UserIpAddress = IPAddress.Parse($"127.0.0.1").GetAddressBytes(),
                    UserAgent = "Firefox",
                }],
            };
            var anotherCategoryThread = new Thread
            {
                CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                Title = "another category thread",
                IsPinned = false,
                IsClosed = false,
                IsDeleted = false,
                BumpLimit = 500,
                ShowThreadLocalUserHash = false,
                Category = category2,
                CreatedBy = null,
                Posts = [new Post
                {
                    CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                    IsSageEnabled = false,
                    IsDeleted = false,
                    MessageText = $"test post in deleted thread",
                    MessageHtml = $"test post in deleted thread",
                    UserIpAddress = IPAddress.Parse($"127.0.0.1").GetAddressBytes(),
                    UserAgent = "Firefox",
                }],
            };

            var threads = Enumerable
                .Range(0, totalThreadCount)
                .Select(i => new Thread
                {
                    CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime.AddSeconds(i),
                    Title = $"test thread {i}",
                    IsPinned = false,
                    IsClosed = false,
                    IsDeleted = false,
                    BumpLimit = 500,
                    ShowThreadLocalUserHash = false,
                    Category = category1,
                    CreatedBy = null,
                })
                .Union([deletedThread, anotherCategoryThread])
                .ToList();
            seedDbContext.Threads.AddRange(threads);

            var postWithoutSage = new Post
            {
                CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime.Subtract(TimeSpan.FromDays(20)),
                IsDeleted = false,
                IsSageEnabled = false,
                MessageText = "no-sage post",
                MessageHtml = "no-sage post",
                UserIpAddress = IPAddress.Parse("127.0.0.1").GetAddressBytes(),
                UserAgent = "Chrome",
            };
            var threadWithPostWithoutSage = new Thread
            {
                CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                Title = "thread with no-sage post",
                IsPinned = false,
                IsClosed = false,
                IsDeleted = false,
                BumpLimit = 500,
                ShowThreadLocalUserHash = false,
                Category = category1,
                CreatedBy = null,
                Posts = [postWithoutSage],
            };
            seedDbContext.Threads.Add(threadWithPostWithoutSage);

            var posts = threads.SelectMany(t => Enumerable
                    .Range(0, totalPostCountPerThread)
                    .Select(i => new Post
                    {
                        CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime.AddMinutes(i),
                        IsSageEnabled = true,
                        IsDeleted = false,
                        MessageText = $"test post {i}",
                        MessageHtml = $"test post {i}",
                        UserIpAddress = IPAddress.Parse($"127.0.0.{i}").GetAddressBytes(),
                        UserAgent = "Firefox",
                        Thread = t,
                    })
                    .Union([new Post
                    {
                        CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime.AddYears(1),
                        IsSageEnabled = false,
                        IsDeleted = true,
                        MessageText = $"deleted post",
                        MessageHtml = $"deleted post",
                        UserIpAddress = IPAddress.Parse($"127.0.0.1").GetAddressBytes(),
                        UserAgent = "Firefox",
                        Thread = t,
                    }])
                    .ToList())
                .ToList();
            seedDbContext.Posts.AddRange(posts);

            await seedDbContext.SaveChangesAsync(cancellationToken);
        }

        // Act
        using (var actScope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var threadRepository = actScope.ServiceProvider.GetRequiredService<IThreadRepository>();
            var actualThreadPreviews = await threadRepository.ListThreadPreviewsPaginatedAsync(new ThreadPreviewsFilter
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                OrderBy = [
                    new OrderByItem { Field = nameof(ThreadPreviewSm.IsPinned), Direction = OrderByDirection.Desc },
                    new OrderByItem { Field = nameof(ThreadPreviewSm.LastPostCreatedAt), Direction = OrderByDirection.Desc },
                    new OrderByItem { Field = nameof(ThreadPreviewSm.Id), Direction = OrderByDirection.Desc },
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

            // check that category is correct
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewSm>(x => x.CategoryAlias == "b"));

            // check that there are no deleted threads
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewSm>(x => x.IsDeleted == false));

            // check that there are no deleted posts
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewSm>(x => x.Posts.All(p => p.IsDeleted == false)));

            // check that every next thread updated earlier than the previous one (sort by LastPostCreatedAt desc)
            Assert.That(actualThreadPreviews.Data, Is.Ordered
                .By(nameof(ThreadPreviewSm.IsPinned))
                .Descending
                .Then
                .By(nameof(ThreadPreviewSm.LastPostCreatedAt))
                .Descending);

            foreach (var thread in actualThreadPreviews.Data)
            {
                // check that posts are sorted by date ascending
                Assert.That(thread.Posts, Is.Ordered.By(nameof(PostPreviewDto.CreatedAt)).Ascending);
            }
        }
    }
}
