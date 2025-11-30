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
using Hikkaba.Shared.Constants;
using Hikkaba.Tests.Integration.Utils;
using Microsoft.Extensions.DependencyInjection;
using Thread = Hikkaba.Data.Entities.Thread;

namespace Hikkaba.Tests.Integration.Builders;

internal sealed class ThreadTestDataBuilder
{
    private static readonly GuidGenerator GuidGenerator = new();

    private readonly ApplicationDbContext _dbContext;
    private readonly List<Category> _categories = [];
    private readonly List<Thread> _threads = [];

    private ApplicationUser? _admin;

    public ThreadTestDataBuilder(IServiceScope scope)
    {
        _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        HashService = scope.ServiceProvider.GetRequiredService<IHashService>();
        TimeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
    }

    public ApplicationUser Admin => _admin ?? throw new InvalidOperationException("Admin not created. Call WithDefaultAdmin() first.");
    public IReadOnlyList<Category> Categories => _categories;
    public IReadOnlyList<Thread> Threads => _threads;
    public IHashService HashService { get; }

    public TimeProvider TimeProvider { get; }

    public ThreadTestDataBuilder WithDefaultAdmin()
    {
        _admin = new ApplicationUser
        {
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = "896e8014-c237-41f5-a925-dabf640ee4c4",
            ConcurrencyStamp = "43035b63-359d-4c23-8812-29bbc5affbf2",
            CreatedAt = TimeProvider.GetUtcNow().UtcDateTime,
        };
        _dbContext.Users.Add(_admin);
        return this;
    }

    public ThreadTestDataBuilder WithCategory(string alias, string name)
    {
        EnsureAdminExists();

        var category = new Category
        {
            IsDeleted = false,
            CreatedAt = TimeProvider.GetUtcNow().UtcDateTime,
            ModifiedAt = null,
            Alias = alias,
            Name = name,
            IsHidden = false,
            DefaultBumpLimit = 500,
            ShowThreadLocalUserHash = false,
            ShowOs = false,
            ShowBrowser = false,
            ShowCountry = false,
            MaxThreadCount = Defaults.MaxThreadCountInCategory,
            CreatedBy = Admin,
        };
        _categories.Add(category);
        _dbContext.Categories.Add(category);
        return this;
    }

    public Category GetCategory(string alias) =>
        _categories.Find(c => c.Alias == alias)
        ?? throw new InvalidOperationException($"Category with alias '{alias}' not found.");

    public ThreadTestDataBuilder WithThread(
        string categoryAlias,
        string title,
        bool isPinned = false,
        bool isClosed = false,
        bool isDeleted = false,
        int bumpLimit = 500,
        DateTime? createdAt = null,
        DateTime? lastBumpAt = null)
    {
        var category = GetCategory(categoryAlias);
        var utcNow = createdAt ?? TimeProvider.GetUtcNow().UtcDateTime;

        var thread = new Thread
        {
            CreatedAt = utcNow,
            LastBumpAt = lastBumpAt ?? utcNow,
            Title = title,
            IsPinned = isPinned,
            IsClosed = isClosed,
            IsDeleted = isDeleted,
            BumpLimit = bumpLimit,
            Salt = GuidGenerator.GenerateSeededGuid(),
            Category = category,
        };
        _threads.Add(thread);
        _dbContext.Threads.Add(thread);
        return this;
    }

    public Thread GetThread(string title) =>
        _threads.Find(t => t.Title == title)
        ?? throw new InvalidOperationException($"Thread with title '{title}' not found.");

    public Thread GetLastThread() =>
        _threads.LastOrDefault()
        ?? throw new InvalidOperationException("No threads created yet.");

    public ThreadTestDataBuilder WithPost(
        string threadTitle,
        Guid blobContainerId,
        string messageText,
        string ipAddress = "127.0.0.1",
        string userAgent = "Firefox",
        bool isOriginalPost = false,
        bool isSageEnabled = false,
        bool isDeleted = false,
        TimeSpan? createdAtOffset = null)
    {
        var thread = GetThread(threadTitle);
        var ip = IPAddress.Parse(ipAddress);

        var post = new Post
        {
            IsOriginalPost = isOriginalPost,
            BlobContainerId = blobContainerId,
            CreatedAt = TimeProvider.GetUtcNow().UtcDateTime.Add(createdAtOffset ?? TimeSpan.Zero),
            IsSageEnabled = isSageEnabled,
            IsDeleted = isDeleted,
            MessageText = messageText,
            MessageHtml = messageText,
            UserIpAddress = ip.GetAddressBytes(),
            UserAgent = userAgent,
            ThreadLocalUserHash = HashService.GetHashBytes(thread.Salt, ip.GetAddressBytes()),
            Thread = thread,
        };
        _dbContext.Posts.Add(post);
        return this;
    }

    public ThreadTestDataBuilder WithPostWithAudio(
        string threadTitle,
        Guid blobContainerId,
        string messageText,
        Guid audioBlobId,
        string audioFileName,
        string ipAddress = "127.0.0.1",
        string userAgent = "Chrome",
        TimeSpan? createdAtOffset = null)
    {
        var thread = GetThread(threadTitle);
        var ip = IPAddress.Parse(ipAddress);

        var post = new Post
        {
            IsOriginalPost = false,
            BlobContainerId = blobContainerId,
            CreatedAt = TimeProvider.GetUtcNow().UtcDateTime.Add(createdAtOffset ?? TimeSpan.Zero),
            IsSageEnabled = false,
            MessageText = messageText,
            MessageHtml = messageText,
            UserIpAddress = ip.GetAddressBytes(),
            UserAgent = userAgent,
            ThreadLocalUserHash = HashService.GetHashBytes(thread.Salt, ip.GetAddressBytes()),
            Thread = thread,
            Audios =
            [
                new Audio
                {
                    BlobId = audioBlobId,
                    FileNameWithoutExtension = audioFileName,
                    FileExtension = "mp3",
                    FileSize = 3671469,
                    FileContentType = "audio/mpeg",
                    FileHash = Hasher.Hash("f61d4fbb-4cbd-4d4e-8df1-6c22c58de9cf"u8).AsSpan().ToArray(),
                    Title = audioFileName,
                    Album = "My Album",
                    Artist = "AI Generated Music",
                    DurationSeconds = 120,
                },
            ],
        };
        _dbContext.Posts.Add(post);
        return this;
    }

    public ThreadTestDataBuilder WithPostWithPicture(
        string threadTitle,
        Guid blobContainerId,
        string messageText,
        Guid pictureBlobId,
        string pictureFileName,
        string ipAddress = "127.0.0.1",
        string userAgent = "Chrome",
        TimeSpan? createdAtOffset = null)
    {
        var thread = GetThread(threadTitle);
        var ip = IPAddress.Parse(ipAddress);

        var post = new Post
        {
            IsOriginalPost = false,
            BlobContainerId = blobContainerId,
            CreatedAt = TimeProvider.GetUtcNow().UtcDateTime.Add(createdAtOffset ?? TimeSpan.Zero),
            IsSageEnabled = false,
            MessageText = messageText,
            MessageHtml = messageText,
            UserIpAddress = ip.GetAddressBytes(),
            UserAgent = userAgent,
            ThreadLocalUserHash = HashService.GetHashBytes(thread.Salt, ip.GetAddressBytes()),
            Thread = thread,
            Pictures =
            [
                new Picture
                {
                    BlobId = pictureBlobId,
                    FileNameWithoutExtension = pictureFileName,
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
            ],
        };
        _dbContext.Posts.Add(post);
        return this;
    }

    public ThreadTestDataBuilder WithThreadAndPosts(
        string categoryAlias,
        string title,
        int postCount,
        bool isPinned = false,
        bool isClosed = false,
        bool isDeleted = false,
        int bumpLimit = 500,
        bool allPostsSage = false,
        bool includeDeletedPost = false,
        DateTime? threadCreatedAt = null)
    {
        var category = GetCategory(categoryAlias);
        var utcNow = threadCreatedAt ?? TimeProvider.GetUtcNow().UtcDateTime;
        var salt = GuidGenerator.GenerateSeededGuid();

        var thread = new Thread
        {
            CreatedAt = utcNow,
            LastBumpAt = utcNow,
            Title = title,
            IsPinned = isPinned,
            IsClosed = isClosed,
            IsDeleted = isDeleted,
            BumpLimit = bumpLimit,
            Salt = salt,
            Category = category,
        };

        var posts = Enumerable.Range(0, postCount)
            .Select(i =>
            {
                var ip = IPAddress.Parse($"127.0.0.{i % 256}").GetAddressBytes();
                return new Post
                {
                    IsOriginalPost = i == 0,
                    BlobContainerId = GuidGenerator.GenerateSeededGuid(),
                    CreatedAt = TimeProvider.GetUtcNow().UtcDateTime.AddMinutes(i),
                    IsSageEnabled = allPostsSage || i % 2 == 0,
                    IsDeleted = false,
                    MessageText = $"test post {i}",
                    MessageHtml = $"test post {i}",
                    UserIpAddress = ip,
                    UserAgent = "Firefox",
                    ThreadLocalUserHash = HashService.GetHashBytes(salt, ip),
                    Thread = thread,
                };
            })
            .ToList();

        if (includeDeletedPost)
        {
            var deletedPostIp = IPAddress.Parse("127.0.0.1").GetAddressBytes();
            posts.Add(new Post
            {
                IsOriginalPost = false,
                BlobContainerId = GuidGenerator.GenerateSeededGuid(),
                CreatedAt = TimeProvider.GetUtcNow().UtcDateTime.AddYears(1),
                IsSageEnabled = false,
                IsDeleted = true,
                MessageText = "deleted post",
                MessageHtml = "deleted post",
                UserIpAddress = deletedPostIp,
                UserAgent = "Firefox",
                ThreadLocalUserHash = HashService.GetHashBytes(salt, deletedPostIp),
                Thread = thread,
            });
        }

        _threads.Add(thread);
        _dbContext.Threads.Add(thread);
        _dbContext.Posts.AddRange(posts);
        return this;
    }

    public ThreadTestDataBuilder WithManyThreadsAndPosts(
        string categoryAlias,
        int threadCount,
        int postCountPerThread,
        bool includeDeletedPost = false,
        Func<int, bool>? isPinnedSelector = null,
        Func<int, DateTime>? threadCreatedAtSelector = null)
    {
        for (var i = 0; i < threadCount; i++)
        {
            var createdAt = threadCreatedAtSelector?.Invoke(i) ?? TimeProvider.GetUtcNow().UtcDateTime.AddSeconds(i);
            WithThreadAndPosts(
                categoryAlias,
                $"test thread {i}",
                postCountPerThread,
                isPinned: isPinnedSelector?.Invoke(i) ?? false,
                includeDeletedPost: includeDeletedPost,
                threadCreatedAt: createdAt);
        }
        return this;
    }

    public ThreadTestDataBuilder AddPostsToThread(
        string threadTitle,
        DateTime startingAt,
        int count,
        bool isSageEnabled = false,
        bool isDeleted = false)
    {
        var thread = GetThread(threadTitle);

        for (var i = 0; i < count; i++)
        {
            var ip = IPAddress.Parse($"127.0.0.{i % 256}").GetAddressBytes();
            var post = new Post
            {
                IsOriginalPost = thread.Posts.Count == 0 && i == 0,
                BlobContainerId = GuidGenerator.GenerateSeededGuid(),
                CreatedAt = startingAt.AddSeconds(i),
                IsSageEnabled = isSageEnabled,
                IsDeleted = isDeleted,
                MessageText = $"test post {i} in thread {thread.Title}",
                MessageHtml = $"test post {i} in thread {thread.Title}",
                UserIpAddress = ip,
                UserAgent = "Firefox",
                ThreadLocalUserHash = HashService.GetHashBytes(thread.Salt, ip),
                Thread = thread,
            };
            thread.Posts.Add(post);
        }
        return this;
    }

    public ThreadTestDataBuilder UpdateThreadLastBumpAt(string threadTitle)
    {
        var thread = GetThread(threadTitle);
        var lastNonSagePost = thread.Posts.Where(p => p is { IsSageEnabled: false, IsDeleted: false }).MaxBy(p => p.CreatedAt);
        if (lastNonSagePost != null)
        {
            thread.LastBumpAt = lastNonSagePost.CreatedAt;
        }
        return this;
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private void EnsureAdminExists()
    {
        if (_admin == null)
        {
            throw new InvalidOperationException("Admin must be created first. Call WithDefaultAdmin().");
        }
    }
}
