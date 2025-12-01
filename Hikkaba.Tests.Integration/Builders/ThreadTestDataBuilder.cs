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
    private readonly GuidGenerator _guidGenerator = new();

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

    public ThreadTestDataBuilder WithCategory(
        string alias,
        string name,
        bool isDeleted = false,
        int defaultBumpLimit = 500,
        bool showThreadLocalUserHash = false)
    {
        EnsureAdminExists();

        var category = new Category
        {
            IsDeleted = isDeleted,
            CreatedAt = TimeProvider.GetUtcNow().UtcDateTime,
            ModifiedAt = null,
            Alias = alias,
            Name = name,
            IsHidden = false,
            DefaultBumpLimit = defaultBumpLimit,
            ShowThreadLocalUserHash = showThreadLocalUserHash,
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
        bool isCyclic = false,
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
            IsCyclic = isCyclic,
            IsDeleted = isDeleted,
            BumpLimit = bumpLimit,
            Salt = _guidGenerator.GenerateSeededGuid(),
            Category = category,
        };
        _threads.Add(thread);
        _dbContext.Threads.Add(thread);
        return this;
    }

    public ThreadTestDataBuilder WithThreadAndOp(
        string categoryAlias,
        string title,
        bool isPinned = false,
        bool isClosed = false,
        bool isCyclic = false,
        bool isDeleted = false,
        int bumpLimit = 500,
        DateTime? createdAt = null,
        DateTime? lastBumpAt = null,
        ApplicationUser? modifiedBy = null)
    {
        WithThread(categoryAlias, title, isPinned, isClosed, isCyclic, isDeleted, bumpLimit, createdAt, lastBumpAt);

        var thread = GetThread(title);
        var ip = IPAddress.Parse("127.0.0.1").GetAddressBytes();

        if (modifiedBy != null)
        {
            thread.ModifiedBy = modifiedBy;
            thread.ModifiedAt = TimeProvider.GetUtcNow().UtcDateTime;
        }

        var post = new Post
        {
            IsOriginalPost = true,
            BlobContainerId = _guidGenerator.GenerateSeededGuid(),
            CreatedAt = thread.CreatedAt,
            IsSageEnabled = false,
            IsDeleted = false,
            MessageText = $"OP post in {title}",
            MessageHtml = $"OP post in {title}",
            UserIpAddress = ip,
            UserAgent = "Firefox",
            ThreadLocalUserHash = HashService.GetHashBytes(thread.Salt, ip),
            Thread = thread,
        };
        _dbContext.Posts.Add(post);
        return this;
    }

    public ThreadTestDataBuilder WithEnrichedThreadAndPosts(
        string categoryAlias,
        string title,
        ApplicationUser modifiedBy,
        DateTime? createdAt = null,
        DateTime? lastBumpAt = null)
    {
        var category = GetCategory(categoryAlias);
        var utcNow = createdAt ?? TimeProvider.GetUtcNow().UtcDateTime;
        var salt = _guidGenerator.GenerateSeededGuid();

        var thread = new Thread
        {
            CreatedAt = utcNow,
            LastBumpAt = lastBumpAt ?? utcNow,
            Title = title,
            IsPinned = false,
            IsClosed = false,
            IsCyclic = false,
            IsDeleted = false,
            BumpLimit = 500,
            Salt = salt,
            Category = category,
            ModifiedBy = modifiedBy,
            ModifiedAt = utcNow,
        };

        _threads.Add(thread);
        _dbContext.Threads.Add(thread);

        // Create OP post with all attachment types
        var opIp = IPAddress.Parse("127.0.0.1").GetAddressBytes();
        var opPost = new Post
        {
            IsOriginalPost = true,
            BlobContainerId = _guidGenerator.GenerateSeededGuid(),
            CreatedAt = utcNow,
            IsSageEnabled = false,
            IsDeleted = false,
            MessageText = $"OP post in {title}",
            MessageHtml = $"OP post in {title}",
            UserIpAddress = opIp,
            UserAgent = "Firefox",
            ThreadLocalUserHash = HashService.GetHashBytes(salt, opIp),
            Thread = thread,
            ModifiedBy = modifiedBy,
            ModifiedAt = utcNow,
            Audios =
            [
                new Audio
                {
                    BlobId = _guidGenerator.GenerateSeededGuid(),
                    FileNameWithoutExtension = "test_audio",
                    FileExtension = ".mp3",
                    FileSize = 1024,
                    FileContentType = "audio/mpeg",
                    FileHash = new byte[32],
                    Title = "Test Title",
                    Album = "Test Album",
                    Artist = "Test Artist",
                    DurationSeconds = 180,
                },
            ],
            Documents =
            [
                new Document
                {
                    BlobId = _guidGenerator.GenerateSeededGuid(),
                    FileNameWithoutExtension = "test_document",
                    FileExtension = ".pdf",
                    FileSize = 2048,
                    FileContentType = "application/pdf",
                    FileHash = new byte[32],
                },
            ],
            Notices =
            [
                new Notice
                {
                    BlobId = _guidGenerator.GenerateSeededGuid(),
                    Text = "Admin notice",
                    CreatedAt = utcNow,
                    CreatedBy = modifiedBy,
                },
            ],
            Pictures =
            [
                new Picture
                {
                    BlobId = _guidGenerator.GenerateSeededGuid(),
                    FileNameWithoutExtension = "test_picture",
                    FileExtension = ".jpg",
                    FileSize = 4096,
                    FileContentType = "image/jpeg",
                    FileHash = new byte[32],
                    Width = 800,
                    Height = 600,
                    ThumbnailExtension = ".jpg",
                    ThumbnailWidth = 200,
                    ThumbnailHeight = 150,
                },
            ],
            Videos =
            [
                new Video
                {
                    BlobId = _guidGenerator.GenerateSeededGuid(),
                    FileNameWithoutExtension = "test_video",
                    FileExtension = ".mp4",
                    FileSize = 8192,
                    FileContentType = "video/mp4",
                    FileHash = new byte[32],
                },
            ],
        };
        _dbContext.Posts.Add(opPost);

        // Create reply post that mentions OP
        var replyIp = IPAddress.Parse("127.0.0.2").GetAddressBytes();
        var replyPost = new Post
        {
            IsOriginalPost = false,
            BlobContainerId = _guidGenerator.GenerateSeededGuid(),
            CreatedAt = utcNow.AddMinutes(1),
            IsSageEnabled = false,
            IsDeleted = false,
            MessageText = $"Reply in {title}",
            MessageHtml = $"Reply in {title}",
            UserIpAddress = replyIp,
            UserAgent = "Chrome",
            ThreadLocalUserHash = HashService.GetHashBytes(salt, replyIp),
            Thread = thread,
            Pictures =
            [
                new Picture
                {
                    BlobId = _guidGenerator.GenerateSeededGuid(),
                    FileNameWithoutExtension = "reply_picture",
                    FileExtension = ".png",
                    FileSize = 2048,
                    FileContentType = "image/png",
                    FileHash = new byte[32],
                    Width = 640,
                    Height = 480,
                    ThumbnailExtension = ".png",
                    ThumbnailWidth = 160,
                    ThumbnailHeight = 120,
                },
            ],
        };
        _dbContext.Posts.Add(replyPost);

        // Create PostToReply relationship (reply mentions OP)
        var postToReply = new PostToReply
        {
            Post = opPost,
            Reply = replyPost,
        };
        _dbContext.PostsToReplies.Add(postToReply);

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
        string messageText,
        string ipAddress = "127.0.0.1",
        string userAgent = "Firefox",
        bool isOriginalPost = false,
        bool isSageEnabled = false,
        bool isDeleted = false,
        TimeSpan? createdAtOffset = null,
        Guid? blobContainerId = null)
    {
        var thread = GetThread(threadTitle);
        var ip = IPAddress.Parse(ipAddress);

        var post = new Post
        {
            IsOriginalPost = isOriginalPost,
            BlobContainerId = blobContainerId ?? _guidGenerator.GenerateSeededGuid(),
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
        string messageText,
        string audioFileName,
        string ipAddress = "127.0.0.1",
        string userAgent = "Chrome",
        TimeSpan? createdAtOffset = null,
        Guid? blobContainerId = null,
        Guid? audioBlobId = null)
    {
        var thread = GetThread(threadTitle);
        var ip = IPAddress.Parse(ipAddress);

        var post = new Post
        {
            IsOriginalPost = false,
            BlobContainerId = blobContainerId ?? _guidGenerator.GenerateSeededGuid(),
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
                    BlobId = audioBlobId ?? _guidGenerator.GenerateSeededGuid(),
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
        string messageText,
        string pictureFileName,
        string ipAddress = "127.0.0.1",
        string userAgent = "Chrome",
        TimeSpan? createdAtOffset = null,
        Guid? blobContainerId = null,
        Guid? pictureBlobId = null)
    {
        var thread = GetThread(threadTitle);
        var ip = IPAddress.Parse(ipAddress);

        var post = new Post
        {
            IsOriginalPost = false,
            BlobContainerId = blobContainerId ?? _guidGenerator.GenerateSeededGuid(),
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
                    BlobId = pictureBlobId ?? _guidGenerator.GenerateSeededGuid(),
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
        var salt = _guidGenerator.GenerateSeededGuid();

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
                    BlobContainerId = _guidGenerator.GenerateSeededGuid(),
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
                BlobContainerId = _guidGenerator.GenerateSeededGuid(),
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
                BlobContainerId = _guidGenerator.GenerateSeededGuid(),
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

        // Get non-sage, non-deleted posts ordered by creation date
        var eligiblePosts = thread.Posts
            .Where(p => p is { IsSageEnabled: false, IsDeleted: false })
            .OrderBy(p => p.CreatedAt)
            .ToList();

        // If bump limit is set, only posts within the limit can bump the thread
        var bumpLimit = thread.BumpLimit;
        if (bumpLimit > 0 && eligiblePosts.Count > bumpLimit)
        {
            eligiblePosts = eligiblePosts.Take(bumpLimit).ToList();
        }

        var lastEligiblePost = eligiblePosts.LastOrDefault();
        if (lastEligiblePost != null)
        {
            thread.LastBumpAt = lastEligiblePost.CreatedAt;
        }
        return this;
    }

    /// <summary>
    /// Creates a post in replyThread that replies to the OP post in mentionedThread.
    /// The new post will have a PostToReply record linking it to the mentioned thread's OP.
    /// </summary>
    public ThreadTestDataBuilder WithCrossThreadReply(
        string replyThreadTitle,
        string mentionedThreadTitle,
        string messageText,
        string ipAddress = "127.0.0.1",
        string userAgent = "Firefox")
    {
        var replyThread = GetThread(replyThreadTitle);
        var mentionedThread = GetThread(mentionedThreadTitle);

        // Get the OP post from the mentioned thread
        var mentionedPost = mentionedThread.Posts.FirstOrDefault(p => p.IsOriginalPost)
            ?? throw new InvalidOperationException($"No OP post found in thread '{mentionedThreadTitle}'");

        var ip = IPAddress.Parse(ipAddress);
        var replyPost = new Post
        {
            IsOriginalPost = false,
            BlobContainerId = _guidGenerator.GenerateSeededGuid(),
            CreatedAt = TimeProvider.GetUtcNow().UtcDateTime,
            IsSageEnabled = false,
            IsDeleted = false,
            MessageText = messageText,
            MessageHtml = messageText,
            UserIpAddress = ip.GetAddressBytes(),
            UserAgent = userAgent,
            ThreadLocalUserHash = HashService.GetHashBytes(replyThread.Salt, ip.GetAddressBytes()),
            Thread = replyThread,
        };
        _dbContext.Posts.Add(replyPost);

        // Create cross-thread PostToReply relationship
        var postToReply = new PostToReply
        {
            Post = mentionedPost,
            Reply = replyPost,
        };
        _dbContext.PostsToReplies.Add(postToReply);

        return this;
    }

    /// <summary>
    /// Alias for <see cref="WithCrossThreadReply"/>.
    /// Creates a post in replyThread that replies to the OP post in mentionedThread.
    /// </summary>
    public ThreadTestDataBuilder WithCrossThreadMention(
        string replyThreadTitle,
        string mentionedThreadTitle,
        string messageText,
        string ipAddress = "127.0.0.1",
        string userAgent = "Firefox")
    {
        return WithCrossThreadReply(replyThreadTitle, mentionedThreadTitle, messageText, ipAddress, userAgent);
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
