using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Blake3;
using Hikkaba.Application.Contracts;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Entities.Attachments;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Constants;
using Hikkaba.Tests.Integration.Constants;
using Hikkaba.Tests.Integration.Extensions;
using Hikkaba.Tests.Integration.Services;
using Hikkaba.Tests.Integration.Utils;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Thread = Hikkaba.Data.Entities.Thread;

namespace Hikkaba.Tests.Integration.Tests.Repositories;

[TestFixture]
[Parallelizable(scope: ParallelScope.Fixtures)]
internal sealed class ThreadRepositoryTests
{
    private static readonly GuidGenerator GuidGenerator = new();

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
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hashService = scope.ServiceProvider.GetRequiredService<IHashService>();

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
            SecurityStamp = "896e8014-c237-41f5-a925-dabf640ee4c4",
            ConcurrencyStamp = "43035b63-359d-4c23-8812-29bbc5affbf2",
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
            ShowThreadLocalUserHash = false,
            ShowOs = false,
            ShowBrowser = false,
            ShowCountry = false,
            MaxThreadCount = Defaults.MaxThreadCountInCategory,
            Board = board,
            CreatedBy = admin,
        };
        dbContext.Categories.Add(category);

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var thread = new Thread
        {
            CreatedAt = utcNow,
            LastBumpAt = utcNow,
            Title = "test thread 1 Buzz",
            IsPinned = false,
            IsClosed = false,
            BumpLimit = 500,
            Salt = GuidGenerator.GenerateSeededGuid(),
            Category = category,
        };
        dbContext.Threads.Add(thread);

        var userIp = IPAddress.Parse("127.0.0.1").GetAddressBytes();
        var post0 = new Post
        {
            IsOriginalPost = true,
            BlobContainerId = new Guid("545917CA-374F-4C34-80B9-7D8DF0842D72"),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime.AddSeconds(1),
            IsSageEnabled = false,
            MessageText = "test post 0",
            MessageHtml = "test post 0",
            UserIpAddress = userIp,
            UserAgent = "Firefox",
            ThreadLocalUserHash = hashService.GetHashBytes(thread.Salt, userIp),
            Thread = thread,
        };
        var post1 = new Post
        {
            IsOriginalPost = false,
            BlobContainerId = new Guid("502FACD5-C207-4684-960B-274949E6D043"),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime.AddSeconds(2),
            IsSageEnabled = false,
            MessageText = "test post 1",
            MessageHtml = "test post 1",
            UserIpAddress = userIp,
            UserAgent = "Chrome",
            ThreadLocalUserHash = hashService.GetHashBytes(thread.Salt, userIp),
            Thread = thread,
            Audios = new List<Audio>
            {
                new()
                {
                    BlobId = new Guid("6D3CD116-6336-47BC-BBE7-5DB289AC6C51"),
                    FileNameWithoutExtension = "Extended electric guitar solo",
                    FileExtension = "mp3",
                    FileSize = 3671469,
                    FileContentType = "audio/mpeg",
                    FileHash = Hasher.Hash("f61d4fbb-4cbd-4d4e-8df1-6c22c58de9cf"u8)
                        .AsSpan()
                        .ToArray(),
                    Title = "Extended electric guitar solo",
                    Album = "My Album",
                    Artist = "AI Generated Music",
                    DurationSeconds = 120,
                },
            },
        };
        var post2 = new Post
        {
            IsOriginalPost = false,
            BlobContainerId = new Guid("91F9A825-FFC0-45FA-B8CF-EA0435F414BC"),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime.AddSeconds(3),
            IsSageEnabled = false,
            MessageText = "test post 2",
            MessageHtml = "test post 2",
            UserIpAddress = userIp,
            UserAgent = "Chrome",
            ThreadLocalUserHash = hashService.GetHashBytes(thread.Salt, userIp),
            Thread = thread,
            Pictures = new List<Picture>
            {
                new()
                {
                    BlobId = new Guid("668B2737-0540-4DDD-A23E-58FA031A933F"),
                    FileNameWithoutExtension = "photo_2024-10-31_16-20-39",
                    FileExtension = "jpg",
                    FileSize = 204316,
                    FileContentType = "image/jpeg",
                    FileHash = Hasher.Hash("6e84e6b4-5370-44c6-a319-a03a027f3905"u8).AsSpan().ToArray(),
                    Width = 1280,
                    Height = 960,
                    ThumbnailExtension = "jpg",
                    ThumbnailWidth = 128,
                    ThumbnailHeight = 96,
                },
            },
        };
        var post3 = new Post
        {
            IsOriginalPost = false,
            BlobContainerId = new Guid("BD852887-CBE3-4BAB-9FAC-F501EC3DA439"),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime.AddSeconds(4),
            IsSageEnabled = false,
            MessageText = "test post 3",
            MessageHtml = "test post 3",
            UserIpAddress = userIp,
            UserAgent = "Chrome",
            ThreadLocalUserHash = hashService.GetHashBytes(thread.Salt, userIp),
            Thread = thread,
        };
        var post4 = new Post
        {
            IsOriginalPost = false,
            BlobContainerId = new Guid("2FA199CC-CD14-402D-8209-0A1B8353E463"),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime.AddSeconds(5),
            IsSageEnabled = false,
            MessageText = "test post 4",
            MessageHtml = "test post 4",
            UserIpAddress = userIp,
            UserAgent = "Chrome",
            ThreadLocalUserHash = hashService.GetHashBytes(thread.Salt, userIp),
            Thread = thread,
        };
        var post5 = new Post
        {
            IsOriginalPost = false,
            BlobContainerId = new Guid("1F657883-6C50-48FE-982C-5E1B552918D3"),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime.AddSeconds(6),
            IsSageEnabled = false,
            MessageText = "test post 5",
            MessageHtml = "test post 5",
            UserIpAddress = userIp,
            UserAgent = "Chrome",
            ThreadLocalUserHash = hashService.GetHashBytes(thread.Salt, userIp),
            Thread = thread,
        };
        List<Post> allPosts = [post0, post1, post2, post3, post4, post5];
        dbContext.AddRange(allPosts);

        await dbContext.SaveChangesAsync(cancellationToken);

        var repository = scope.ServiceProvider.GetRequiredService<IThreadRepository>();

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

        // all posts have valid hash
        foreach (var actualPost in new List<PostDetailsModel> { actualPost0, actualPost1, actualPost2, actualPost3 })
        {
            Assert.That(actualPost.ThreadLocalUserHash, Is.EqualTo(hashService.GetHashBytes(thread.Salt, userIp)));
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
        await using var customAppFactory = await CreateAppFactoryAsync();
        using (var seedScope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var seedTimeProvider = seedScope.ServiceProvider.GetRequiredService<TimeProvider>();
            var seedDbContext = seedScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var hashService = seedScope.ServiceProvider.GetRequiredService<IHashService>();
            var timeProvider = seedScope.ServiceProvider.GetRequiredService<TimeProvider>();

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
                SecurityStamp = "896e8014-c237-41f5-a925-dabf640ee4c4",
                ConcurrencyStamp = "43035b63-359d-4c23-8812-29bbc5affbf2",
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
                ShowThreadLocalUserHash = false,
                ShowOs = false,
                ShowBrowser = false,
                ShowCountry = false,
                MaxThreadCount = Defaults.MaxThreadCountInCategory,
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
                ShowThreadLocalUserHash = false,
                ShowOs = false,
                ShowBrowser = false,
                ShowCountry = false,
                MaxThreadCount = Defaults.MaxThreadCountInCategory,
                Board = board,
                CreatedBy = admin,
            };
            seedDbContext.Categories.AddRange(category1, category2);

            var salt1 = GuidGenerator.GenerateSeededGuid();
            var salt2 = GuidGenerator.GenerateSeededGuid();
            var userIp = IPAddress.Parse($"127.0.0.1").GetAddressBytes();
            var utcNow = timeProvider.GetUtcNow().UtcDateTime;
            var deletedThread = new Thread
            {
                CreatedAt = utcNow,
                LastBumpAt = utcNow,
                Title = "deleted thread",
                IsPinned = true,
                IsClosed = true,
                IsDeleted = true,
                BumpLimit = 500,
                Salt = salt1,
                Category = category1,
                Posts =
                [
                    new Post
                    {
                        IsOriginalPost = true,
                        BlobContainerId = new Guid("CC8B3B30-A82B-4634-BE98-17E6FE646E1A"),
                        CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                        IsSageEnabled = false,
                        IsDeleted = false,
                        MessageText = $"test post in deleted thread",
                        MessageHtml = $"test post in deleted thread",
                        UserIpAddress = userIp,
                        UserAgent = "Firefox",
                        ThreadLocalUserHash = hashService.GetHashBytes(salt1, userIp),
                    },
                ],
            };
            var anotherCategoryThread = new Thread
            {
                CreatedAt = utcNow,
                LastBumpAt = utcNow,
                Title = "another category thread",
                IsPinned = false,
                IsClosed = false,
                IsDeleted = false,
                BumpLimit = 500,
                Salt = salt2,
                Category = category2,
                Posts =
                [
                    new Post
                    {
                        IsOriginalPost = true,
                        BlobContainerId = new Guid("8B6789E0-9086-456F-94AA-AC070DF868B5"),
                        CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                        IsSageEnabled = false,
                        IsDeleted = false,
                        MessageText = $"test post in deleted thread",
                        MessageHtml = $"test post in deleted thread",
                        UserIpAddress = userIp,
                        UserAgent = "Firefox",
                        ThreadLocalUserHash = hashService.GetHashBytes(salt2, userIp),
                    },
                ],
            };

            var threads = Enumerable
                .Range(0, totalThreadCount)
                .Select(i => new Thread
                {
                    CreatedAt = utcNow.AddSeconds(i),
                    LastBumpAt = utcNow.AddSeconds(i),
                    Title = $"test thread {i}",
                    IsPinned = false,
                    IsClosed = false,
                    IsDeleted = false,
                    BumpLimit = 500,
                    Salt = GuidGenerator.GenerateSeededGuid(),
                    Category = category1,
                })
                .Union([deletedThread, anotherCategoryThread])
                .ToList();
            seedDbContext.Threads.AddRange(threads);

            var posts = threads.SelectMany(t => Enumerable
                    .Range(0, totalPostCountPerThread)
                    .Select(i => new Post
                    {
                        IsOriginalPost = i == 0,
                        BlobContainerId = GuidGenerator.GenerateSeededGuid(),
                        CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime.AddMinutes(i),
                        IsSageEnabled = i % 2 == 0,
                        IsDeleted = false,
                        MessageText = $"test post {i}",
                        MessageHtml = $"test post {i}",
                        UserIpAddress = IPAddress.Parse($"127.0.0.{i}").GetAddressBytes(),
                        UserAgent = "Firefox",
                        ThreadLocalUserHash = hashService.GetHashBytes(t.Salt, IPAddress.Parse($"127.0.0.{i}").GetAddressBytes()),
                        Thread = t,
                    })
                    .Union([
                        new Post
                        {
                            IsOriginalPost = false,
                            BlobContainerId = GuidGenerator.GenerateSeededGuid(),
                            CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime.AddYears(1),
                            IsSageEnabled = false,
                            IsDeleted = true,
                            MessageText = $"deleted post",
                            MessageHtml = $"deleted post",
                            UserIpAddress = IPAddress.Parse($"127.0.0.1").GetAddressBytes(),
                            UserAgent = "Firefox",
                            ThreadLocalUserHash = hashService.GetHashBytes(t.Salt, IPAddress.Parse($"127.0.0.1").GetAddressBytes()),
                            Thread = t,
                        },
                    ])
                    .ToList())
                .ToList();
            seedDbContext.Posts.AddRange(posts);

            await seedDbContext.SaveChangesAsync(cancellationToken);
        }

        // Act
        using (var actScope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
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

            // N threads shown (except deleted)
            Assert.That(actualThreadPreviews.Data, Has.Count.EqualTo(expectedThreadCount));

            // All threads found (except deleted)
            Assert.That(actualThreadPreviews.TotalItemCount, Is.EqualTo(totalThreadCount));

            Assert.That(actualThreadPreviews.TotalPageCount, Is.EqualTo(Math.Ceiling((decimal)totalThreadCount / pageSize)));
            Assert.That(actualThreadPreviews.PageNumber, Is.EqualTo(pageNumber));
            Assert.That(actualThreadPreviews.PageSize, Is.EqualTo(pageSize));
            Assert.That(actualThreadPreviews.SkippedItemCount, Is.EqualTo((pageNumber - 1) * pageSize));

            // check that category is correct
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => x.CategoryAlias == "b"));

            // check that there are no deleted threads
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => !x.IsDeleted));

            // check that there are no deleted posts
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => x.Posts.All(p => !p.IsDeleted)));

            // check that every next thread updated earlier than the previous one (sort by LastPostCreatedAt desc)
            Assert.That(actualThreadPreviews.Data, Is.Ordered
                .By(nameof(ThreadPreviewModel.IsPinned))
                .Descending
                .Then
                .By(nameof(ThreadPreviewModel.LastBumpAt))
                .Descending);

            foreach (var thread in actualThreadPreviews.Data)
            {
                // Every thread has M posts
                Assert.That(thread.PostCount, Is.EqualTo(totalPostCountPerThread));

                // But only K posts are shown (OP post + latest posts)
                Assert.That(thread.Posts, Has.Count.EqualTo(Defaults.LatestPostsCountInThreadPreview + 1));

                // check that posts are sorted by date ascending
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
        await using var customAppFactory = await CreateAppFactoryAsync();
        using (var seedScope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var seedTimeProvider = customAppFactory.Services.GetRequiredService<TimeProvider>();
            var seedDbContext = seedScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var hashService = seedScope.ServiceProvider.GetRequiredService<IHashService>();

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
                SecurityStamp = "896e8014-c237-41f5-a925-dabf640ee4c4",
                ConcurrencyStamp = "43035b63-359d-4c23-8812-29bbc5affbf2",
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
                ShowThreadLocalUserHash = false,
                ShowOs = false,
                ShowBrowser = false,
                ShowCountry = false,
                MaxThreadCount = Defaults.MaxThreadCountInCategory,
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
                ShowThreadLocalUserHash = false,
                ShowOs = false,
                ShowBrowser = false,
                ShowCountry = false,
                MaxThreadCount = Defaults.MaxThreadCountInCategory,
                Board = board,
                CreatedBy = admin,
            };
            seedDbContext.Categories.AddRange(category1, category2);

            var salt1 = GuidGenerator.GenerateSeededGuid();
            var salt2 = GuidGenerator.GenerateSeededGuid();
            var utcNow = seedTimeProvider.GetUtcNow().UtcDateTime;
            var deletedThread = new Thread
            {
                CreatedAt = utcNow,
                LastBumpAt = utcNow,
                Title = "deleted thread",
                IsPinned = true,
                IsClosed = true,
                IsDeleted = true,
                BumpLimit = 500,
                Salt = salt1,
                Category = category1,
                Posts =
                [
                    new Post
                    {
                        IsOriginalPost = true,
                        BlobContainerId = new Guid("4C708859-478D-451F-9EFD-315EAC9ABCAF"),
                        CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                        IsSageEnabled = false,
                        IsDeleted = false,
                        MessageText = $"test post in deleted thread",
                        MessageHtml = $"test post in deleted thread",
                        UserIpAddress = IPAddress.Parse($"127.0.0.1").GetAddressBytes(),
                        UserAgent = "Firefox",
                        ThreadLocalUserHash = hashService.GetHashBytes(salt1, IPAddress.Parse("127.0.0.1").GetAddressBytes()),
                    },
                ],
            };
            var anotherCategoryThread = new Thread
            {
                CreatedAt = utcNow,
                LastBumpAt = utcNow,
                Title = "another category thread",
                IsPinned = false,
                IsClosed = false,
                IsDeleted = false,
                BumpLimit = 500,
                Salt = salt2,
                Category = category2,
                Posts =
                [
                    new Post
                    {
                        IsOriginalPost = true,
                        BlobContainerId = new Guid("B4041A4C-10CD-4332-AFA2-7D04A9D130DD"),
                        CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                        IsSageEnabled = false,
                        IsDeleted = false,
                        MessageText = $"test post in deleted thread",
                        MessageHtml = $"test post in deleted thread",
                        UserIpAddress = IPAddress.Parse($"127.0.0.1").GetAddressBytes(),
                        UserAgent = "Firefox",
                        ThreadLocalUserHash = hashService.GetHashBytes(salt2, IPAddress.Parse("127.0.0.1").GetAddressBytes()),
                    },
                ],
            };

            var threads = Enumerable
                .Range(0, totalThreadCount)
                .Select(i => new Thread
                {
                    CreatedAt = utcNow,
                    LastBumpAt = utcNow,
                    Title = $"test thread {i}",
                    IsPinned = i == 3,
                    IsClosed = false,
                    IsDeleted = false,
                    BumpLimit = 500,
                    Salt = GuidGenerator.GenerateSeededGuid(),
                    Category = category1,
                })
                .Union([deletedThread, anotherCategoryThread])
                .ToList();
            seedDbContext.Threads.AddRange(threads);

            var posts = threads.SelectMany(t => Enumerable
                    .Range(0, totalPostCountPerThread)
                    .Select(i => new Post
                    {
                        IsOriginalPost = i == 0,
                        BlobContainerId = GuidGenerator.GenerateSeededGuid(),
                        CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime.AddMinutes(i),
                        IsSageEnabled = i % 2 == 0,
                        IsDeleted = false,
                        MessageText = $"test post {i}",
                        MessageHtml = $"test post {i}",
                        UserIpAddress = IPAddress.Parse($"127.0.0.{i}").GetAddressBytes(),
                        UserAgent = "Firefox",
                        ThreadLocalUserHash = hashService.GetHashBytes(t.Salt, IPAddress.Parse($"127.0.0.{i}").GetAddressBytes()),
                        Thread = t,
                    })
                    .Union([
                        new Post
                        {
                            IsOriginalPost = false,
                            BlobContainerId = GuidGenerator.GenerateSeededGuid(),
                            CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime.AddYears(1),
                            IsSageEnabled = false,
                            IsDeleted = true,
                            MessageText = $"deleted post",
                            MessageHtml = $"deleted post",
                            UserIpAddress = IPAddress.Parse($"127.0.0.1").GetAddressBytes(),
                            UserAgent = "Firefox",
                            ThreadLocalUserHash = hashService.GetHashBytes(t.Salt, IPAddress.Parse($"127.0.0.1").GetAddressBytes()),
                            Thread = t,
                        },
                    ])
                    .ToList())
                .ToList();
            seedDbContext.Posts.AddRange(posts);

            await seedDbContext.SaveChangesAsync(cancellationToken);
        }

        // Act
        using (var actScope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
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
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => x.CategoryAlias == "b"));

            // check that there are no deleted threads
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => !x.IsDeleted));

            // check that there are no deleted posts
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => x.Posts.All(p => !p.IsDeleted)));

            // check that every next thread updated earlier than the previous one (sort by LastPostCreatedAt desc)
            Assert.That(actualThreadPreviews.Data, Is.Ordered
                .By(nameof(ThreadPreviewModel.IsPinned))
                .Descending
                .Then
                .By(nameof(ThreadPreviewModel.LastBumpAt))
                .Descending);

            foreach (var thread in actualThreadPreviews.Data)
            {
                // Every thread has M posts
                Assert.That(thread.PostCount, Is.EqualTo(totalPostCountPerThread));

                // But only K posts are shown (OP post + latest posts)
                Assert.That(thread.Posts, Has.Count.EqualTo(Defaults.LatestPostsCountInThreadPreview + 1));

                // check that posts are sorted by date ascending
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
        await using var customAppFactory = await CreateAppFactoryAsync();
        using (var seedScope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var seedTimeProvider = seedScope.ServiceProvider.GetRequiredService<TimeProvider>();
            var seedDbContext = seedScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var hashService = seedScope.ServiceProvider.GetRequiredService<IHashService>();

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
                SecurityStamp = "896e8014-c237-41f5-a925-dabf640ee4c4",
                ConcurrencyStamp = "43035b63-359d-4c23-8812-29bbc5affbf2",
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
                ShowThreadLocalUserHash = false,
                ShowOs = false,
                ShowBrowser = false,
                ShowCountry = false,
                MaxThreadCount = Defaults.MaxThreadCountInCategory,
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
                ShowThreadLocalUserHash = false,
                ShowOs = false,
                ShowBrowser = false,
                ShowCountry = false,
                MaxThreadCount = Defaults.MaxThreadCountInCategory,
                Board = board,
                CreatedBy = admin,
            };
            seedDbContext.Categories.AddRange(category1, category2);

            var salt1 = GuidGenerator.GenerateSeededGuid();
            var salt2 = GuidGenerator.GenerateSeededGuid();
            var utcNow = seedTimeProvider.GetUtcNow().UtcDateTime;
            var deletedThread = new Thread
            {
                CreatedAt = utcNow,
                LastBumpAt = utcNow,
                Title = "deleted thread",
                IsPinned = true,
                IsClosed = true,
                IsDeleted = true,
                BumpLimit = 500,
                Salt = salt1,
                Category = category1,
                Posts =
                [
                    new Post
                    {
                        IsOriginalPost = true,
                        BlobContainerId = new Guid("F115A07E-3B7F-4F54-8140-A9481EBE3F0A"),
                        CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                        IsSageEnabled = false,
                        IsDeleted = false,
                        MessageText = $"test post in deleted thread",
                        MessageHtml = $"test post in deleted thread",
                        UserIpAddress = IPAddress.Parse($"127.0.0.1").GetAddressBytes(),
                        UserAgent = "Firefox",
                        ThreadLocalUserHash = hashService.GetHashBytes(salt1, IPAddress.Parse("127.0.0.1").GetAddressBytes()),
                    },
                ],
            };
            var anotherCategoryThread = new Thread
            {
                CreatedAt = utcNow,
                LastBumpAt = utcNow,
                Title = "another category thread",
                IsPinned = false,
                IsClosed = false,
                IsDeleted = false,
                BumpLimit = 500,
                Salt = salt2,
                Category = category2,
                Posts =
                [
                    new Post
                    {
                        IsOriginalPost = true,
                        BlobContainerId = new Guid("A4129657-90E4-4B5C-95A6-CB9D1B9746EC"),
                        CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                        IsSageEnabled = false,
                        IsDeleted = false,
                        MessageText = $"test post in deleted thread",
                        MessageHtml = $"test post in deleted thread",
                        UserIpAddress = IPAddress.Parse($"127.0.0.1").GetAddressBytes(),
                        UserAgent = "Firefox",
                        ThreadLocalUserHash = hashService.GetHashBytes(salt2, IPAddress.Parse("127.0.0.1").GetAddressBytes()),
                    },
                ],
            };

            var threads = Enumerable
                .Range(0, totalThreadCount)
                .Select(i => new Thread
                {
                    CreatedAt = utcNow,
                    LastBumpAt = utcNow,
                    Title = $"test thread {i}",
                    IsPinned = false,
                    IsClosed = false,
                    IsDeleted = false,
                    BumpLimit = 500,
                    Salt = GuidGenerator.GenerateSeededGuid(),
                    Category = category1,
                })
                .Union([deletedThread, anotherCategoryThread])
                .ToList();
            seedDbContext.Threads.AddRange(threads);

            var salt3 = GuidGenerator.GenerateSeededGuid();
            var postWithoutSage = new Post
            {
                IsOriginalPost = false,
                BlobContainerId = new Guid("9BE82B5A-0C4C-475C-9A3B-29F498E079E5"),
                CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime.Subtract(TimeSpan.FromDays(20)),
                IsDeleted = false,
                IsSageEnabled = false,
                MessageText = "no-sage post",
                MessageHtml = "no-sage post",
                UserIpAddress = IPAddress.Parse("127.0.0.1").GetAddressBytes(),
                UserAgent = "Chrome",
                ThreadLocalUserHash = hashService.GetHashBytes(salt3, IPAddress.Parse("127.0.0.1").GetAddressBytes()),
            };
            var threadWithPostWithoutSage = new Thread
            {
                CreatedAt = utcNow,
                LastBumpAt = utcNow,
                Title = "thread with no-sage post",
                IsPinned = false,
                IsClosed = false,
                IsDeleted = false,
                BumpLimit = 500,
                Salt = salt3,
                Category = category1,
                Posts = [postWithoutSage],
            };
            seedDbContext.Threads.Add(threadWithPostWithoutSage);

            var posts = threads.SelectMany(t => Enumerable
                    .Range(0, totalPostCountPerThread)
                    .Select(i => new Post
                    {
                        IsOriginalPost = i == 0,
                        BlobContainerId = GuidGenerator.GenerateSeededGuid(),
                        CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime.AddMinutes(i),
                        IsSageEnabled = true,
                        IsDeleted = false,
                        MessageText = $"test post {i}",
                        MessageHtml = $"test post {i}",
                        UserIpAddress = IPAddress.Parse($"127.0.0.{i}").GetAddressBytes(),
                        UserAgent = "Firefox",
                        ThreadLocalUserHash = hashService.GetHashBytes(t.Salt, IPAddress.Parse($"127.0.0.{i}").GetAddressBytes()),
                        Thread = t,
                    })
                    .Union([
                        new Post
                        {
                            IsOriginalPost = false,
                            BlobContainerId = GuidGenerator.GenerateSeededGuid(),
                            CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime.AddYears(1),
                            IsSageEnabled = false,
                            IsDeleted = true,
                            MessageText = $"deleted post",
                            MessageHtml = $"deleted post",
                            UserIpAddress = IPAddress.Parse($"127.0.0.1").GetAddressBytes(),
                            UserAgent = "Firefox",
                            ThreadLocalUserHash = hashService.GetHashBytes(t.Salt, IPAddress.Parse($"127.0.0.1").GetAddressBytes()),
                            Thread = t,
                        },
                    ])
                    .ToList())
                .ToList();
            seedDbContext.Posts.AddRange(posts);

            await seedDbContext.SaveChangesAsync(cancellationToken);
        }

        // Act
        using (var actScope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
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

            // check that category is correct
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => x.CategoryAlias == "b"));

            // check that there are no deleted threads
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => !x.IsDeleted));

            // check that there are no deleted posts
            Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => x.Posts.All(p => !p.IsDeleted)));

            // check that every next thread updated earlier than the previous one (sort by LastPostCreatedAt desc)
            Assert.That(actualThreadPreviews.Data, Is.Ordered
                .By(nameof(ThreadPreviewModel.IsPinned))
                .Descending
                .Then
                .By(nameof(ThreadPreviewModel.LastBumpAt))
                .Descending);

            foreach (var thread in actualThreadPreviews.Data)
            {
                // check that posts are sorted by date ascending
                Assert.That(thread.Posts, Is.Ordered.By(nameof(PostDetailsModel.CreatedAt)).Ascending);
            }
        }
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListThreadPreviewsPaginatedAsync_WhenBumpLimitReached_ReturnsCorrectResult(CancellationToken cancellationToken)
    {
        void AddPosts(Thread thread, DateTime startingAt, int count, bool isSageEnabled, bool isDeleted, IHashService hashService)
        {
            for (var i = 0; i < count; i++)
            {
                thread.Posts.Add(new Post
                {
                    IsOriginalPost = i == 0,
                    BlobContainerId = GuidGenerator.GenerateSeededGuid(),
                    CreatedAt = startingAt.AddSeconds(i),
                    IsSageEnabled = isSageEnabled,
                    IsDeleted = isDeleted,
                    MessageText = $"test post {i} in thread {thread.Title}",
                    MessageHtml = $"test post {i} in thread {thread.Title}",
                    UserIpAddress = IPAddress.Parse($"127.0.0.{i}").GetAddressBytes(),
                    UserAgent = "Firefox",
                    ThreadLocalUserHash = hashService.GetHashBytes(thread.Salt, IPAddress.Parse($"127.0.0.{i}").GetAddressBytes()),
                });
            }
        }

        const int bumpLimit = 5;
        const int pageNumber = 1;
        const int pageSize = 10;

        // Arrange
        await using var customAppFactory = await CreateAppFactoryAsync();
        using var seedScope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var seedTimeProvider = seedScope.ServiceProvider.GetRequiredService<TimeProvider>();
        var seedDbContext = seedScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hashService = seedScope.ServiceProvider.GetRequiredService<IHashService>();

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
            SecurityStamp = "896e8014-c237-41f5-a925-dabf640ee4c4",
            ConcurrencyStamp = "43035b63-359d-4c23-8812-29bbc5affbf2",
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
            ShowThreadLocalUserHash = false,
            ShowOs = false,
            ShowBrowser = false,
            ShowCountry = false,
            MaxThreadCount = Defaults.MaxThreadCountInCategory,
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
            ShowThreadLocalUserHash = false,
            ShowOs = false,
            ShowBrowser = false,
            ShowCountry = false,
            MaxThreadCount = Defaults.MaxThreadCountInCategory,
            Board = board,
            CreatedBy = admin,
        };
        seedDbContext.Categories.AddRange(category1, category2);

        var salt1 = GuidGenerator.GenerateSeededGuid();
        var salt2 = GuidGenerator.GenerateSeededGuid();
        var utcNow = seedTimeProvider.GetUtcNow().UtcDateTime;
        var deletedThread = new Thread
        {
            CreatedAt = utcNow,
            LastBumpAt = utcNow,
            Title = "deleted thread",
            IsPinned = true,
            IsClosed = true,
            IsDeleted = true,
            BumpLimit = 500,
            Salt = salt1,
            Category = category1,
            Posts =
            [
                new Post
                {
                    IsOriginalPost = true,
                    BlobContainerId = new Guid("F115A07E-3B7F-4F54-8140-A9481EBE3F0A"),
                    CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                    IsSageEnabled = false,
                    IsDeleted = false,
                    MessageText = $"test post in deleted thread",
                    MessageHtml = $"test post in deleted thread",
                    UserIpAddress = IPAddress.Parse($"127.0.0.1").GetAddressBytes(),
                    UserAgent = "Firefox",
                    ThreadLocalUserHash = hashService.GetHashBytes(salt1, IPAddress.Parse($"127.0.0.1").GetAddressBytes()),
                },
            ],
        };
        var anotherCategoryThread = new Thread
        {
            CreatedAt = utcNow,
            LastBumpAt = utcNow,
            Title = "another category thread",
            IsPinned = false,
            IsClosed = false,
            IsDeleted = false,
            BumpLimit = 500,
            Salt = salt2,
            Category = category2,
            Posts =
            [
                new Post
                {
                    IsOriginalPost = true,
                    BlobContainerId = new Guid("A4129657-90E4-4B5C-95A6-CB9D1B9746EC"),
                    CreatedAt = seedTimeProvider.GetUtcNow().UtcDateTime,
                    IsSageEnabled = false,
                    IsDeleted = false,
                    MessageText = $"test post in deleted thread",
                    MessageHtml = $"test post in deleted thread",
                    UserIpAddress = IPAddress.Parse($"127.0.0.1").GetAddressBytes(),
                    UserAgent = "Firefox",
                    ThreadLocalUserHash = hashService.GetHashBytes(salt1, IPAddress.Parse($"127.0.0.1").GetAddressBytes()),
                },
            ],
        };
        var thread1 = new Thread
        {
            CreatedAt = utcNow.AddMinutes(1),
            LastBumpAt = utcNow.AddMinutes(1),
            Title = "thread with bump limit 1",
            BumpLimit = bumpLimit,
            Salt = GuidGenerator.GenerateSeededGuid(),
            Category = category1,
        };
        var thread2 = new Thread
        {
            CreatedAt = utcNow.AddMinutes(2),
            LastBumpAt = utcNow.AddMinutes(2),
            Title = "thread with bump limit 2",
            BumpLimit = bumpLimit,
            Salt = GuidGenerator.GenerateSeededGuid(),
            Category = category1,
        };
        var thread3 = new Thread
        {
            CreatedAt = utcNow.AddMinutes(3),
            LastBumpAt = utcNow.AddMinutes(3),
            Title = "thread with bump limit 3",
            BumpLimit = bumpLimit,
            Salt = GuidGenerator.GenerateSeededGuid(),
            Category = category1,
        };
        var thread4 = new Thread
        {
            CreatedAt = utcNow.AddMinutes(4),
            LastBumpAt = utcNow.AddMinutes(4),
            Title = "thread with bump limit 4",
            BumpLimit = bumpLimit,
            Salt = GuidGenerator.GenerateSeededGuid(),
            Category = category1,
        };
        List<Thread> allThreads = [deletedThread, anotherCategoryThread, thread1, thread2, thread3, thread4];
        seedDbContext.Threads.AddRange(allThreads);

        // these threads contain the newest posts, but they aren't included in our query
        AddPosts(deletedThread, seedTimeProvider.GetUtcNow().UtcDateTime.AddYears(2), bumpLimit + 2, false, false, hashService);
        AddPosts(anotherCategoryThread, seedTimeProvider.GetUtcNow().UtcDateTime.AddYears(2), bumpLimit + 2, false, false, hashService);

        // thread1 contains several old posts before bump limit and several new posts after bump limit, which shouldn't affect the result
        AddPosts(thread1, seedTimeProvider.GetUtcNow().UtcDateTime.AddYears(-1), bumpLimit, false, false, hashService);
        AddPosts(thread1, seedTimeProvider.GetUtcNow().UtcDateTime.AddYears(1), 2, false, false, hashService);
        thread1.LastBumpAt = thread1.Posts.Where(p => !p.IsSageEnabled).Max(x => x.CreatedAt);

        // thread2 contains several new posts
        AddPosts(thread2, seedTimeProvider.GetUtcNow().UtcDateTime.AddDays(1).AddHours(1), 1, true, false, hashService);
        AddPosts(thread2, seedTimeProvider.GetUtcNow().UtcDateTime.AddDays(1), bumpLimit, false, false, hashService);
        thread2.LastBumpAt = thread2.Posts.Where(p => !p.IsSageEnabled).Max(x => x.CreatedAt);

        // thread3 contains even newer posts
        AddPosts(thread3, seedTimeProvider.GetUtcNow().UtcDateTime.AddDays(1).AddHours(3), 1, true, false, hashService);
        AddPosts(thread3, seedTimeProvider.GetUtcNow().UtcDateTime.AddDays(1).AddSeconds(1), bumpLimit, false, false, hashService);
        thread3.LastBumpAt = thread3.Posts.Where(p => !p.IsSageEnabled).Max(x => x.CreatedAt);

        // thread4 contains a lot of posts
        AddPosts(thread4, seedTimeProvider.GetUtcNow().UtcDateTime, bumpLimit + 10, false, false, hashService);
        AddPosts(thread4, seedTimeProvider.GetUtcNow().UtcDateTime.AddYears(5), bumpLimit + 10, false, false, hashService);
        thread4.LastBumpAt = thread4.Posts.Where(p => !p.IsSageEnabled).Max(x => x.CreatedAt);

        await seedDbContext.SaveChangesAsync(cancellationToken);

        // Act
        using var actScope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
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

        // check that category is correct
        Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => x.CategoryAlias == "b"));

        // check that there are no deleted threads
        Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => !x.IsDeleted));

        // check that there are no deleted posts
        Assert.That(actualThreadPreviews.Data, Is.All.Matches<ThreadPreviewModel>(x => x.Posts.All(p => !p.IsDeleted)));

        // check that every next thread updated earlier than the previous one (sort by LastPostCreatedAt desc)
        Assert.That(actualThreadPreviews.Data, Is.Ordered
            .By(nameof(ThreadPreviewModel.IsPinned))
            .Descending
            .Then
            .By(nameof(ThreadPreviewModel.LastBumpAt))
            .Descending);

        foreach (var thread in actualThreadPreviews.Data)
        {
            // check that posts are sorted by date ascending
            Assert.That(thread.Posts, Is.Ordered.By(nameof(PostDetailsModel.CreatedAt)).Ascending);
        }
    }
}
