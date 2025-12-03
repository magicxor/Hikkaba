using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Entities.Attachments;
using Thread = Hikkaba.Data.Entities.Thread;

namespace Hikkaba.Tests.Integration.Builders;

internal sealed partial class TestDataBuilder
{
    private readonly List<Thread> _threads = [];

    /// <summary>
    ///     Returns the last created thread.
    /// </summary>
    public Thread LastThread =>
        _threads.LastOrDefault()
        ?? throw new InvalidOperationException("Thread not created. Call WithThread() first.");

    /// <summary>
    ///     Creates a thread in the last created category.
    /// </summary>
    public TestDataBuilder WithThread(
        string title,
        bool isPinned = false,
        bool isClosed = false,
        bool isCyclic = false,
        bool isDeleted = false,
        int bumpLimit = 500,
        DateTime? createdAt = null,
        DateTime? lastBumpAt = null)
    {
        EnsureCategoryExists();
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
            Category = LastCategory,
        };
        _threads.Add(thread);
        _dbContext.Threads.Add(thread);
        return this;
    }

    /// <summary>
    ///     Creates a default thread in the last created category.
    /// </summary>
    public TestDataBuilder WithDefaultThread(bool isClosed = false)
    {
        EnsureCategoryExists();

        var utcNow = TimeProvider.GetUtcNow().UtcDateTime;
        var thread = new Thread
        {
            CreatedAt = utcNow,
            LastBumpAt = utcNow,
            Title = isClosed ? "closed thread" : "test thread 1",
            IsPinned = false,
            IsClosed = isClosed,
            BumpLimit = 500,
            Salt = _guidGenerator.GenerateSeededGuid(),
            Category = LastCategory,
        };
        _threads.Add(thread);
        _dbContext.Threads.Add(thread);
        return this;
    }

    /// <summary>
    ///     Creates a thread with an OP post in the last created category.
    /// </summary>
    public TestDataBuilder WithThreadAndOp(
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
        WithThread(title, isPinned, isClosed, isCyclic, isDeleted, bumpLimit, createdAt, lastBumpAt);

        var ip = IPAddress.Parse("127.0.0.1").GetAddressBytes();

        if (modifiedBy != null)
        {
            LastThread.ModifiedBy = modifiedBy;
            LastThread.ModifiedAt = TimeProvider.GetUtcNow().UtcDateTime;
        }

        var post = new Post
        {
            IsOriginalPost = true,
            BlobContainerId = _guidGenerator.GenerateSeededGuid(),
            CreatedAt = LastThread.CreatedAt,
            IsSageEnabled = false,
            IsDeleted = false,
            MessageText = $"OP post in {title}",
            MessageHtml = $"OP post in {title}",
            UserIpAddress = ip,
            UserAgent = "Firefox",
            ThreadLocalUserHash = HashService.GetHashBytes(LastThread.Salt, ip),
            Thread = LastThread,
        };
        _dbContext.Posts.Add(post);
        _posts.Add(post);
        _lastPost = post;
        return this;
    }

    /// <summary>
    ///     Creates an enriched thread with posts (OP with all attachment types, reply with picture) in the last created
    ///     category.
    /// </summary>
    public TestDataBuilder WithEnrichedThreadAndPosts(
        string title,
        ApplicationUser modifiedBy,
        DateTime? createdAt = null,
        DateTime? lastBumpAt = null)
    {
        EnsureCategoryExists();
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
            Category = LastCategory,
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
        _posts.Add(opPost);
        _lastPost = opPost;

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
        _posts.Add(replyPost);

        // Create PostToReply relationship (reply mentions OP)
        var postToReply = new PostToReply
        {
            Post = opPost,
            Reply = replyPost,
        };
        _dbContext.PostsToReplies.Add(postToReply);

        return this;
    }

    public Thread GetThread(string title)
    {
        return _threads.Find(t => t.Title == title)
               ?? throw new InvalidOperationException($"Thread with title '{title}' not found.");
    }

    public TestDataBuilder AddPostsToThread(
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
            _posts.Add(post);
            _lastPost = post;
        }

        return this;
    }

    public TestDataBuilder UpdateThreadLastBumpAt(string threadTitle)
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

    private void EnsureThreadExists()
    {
        if (_threads.Count == 0)
        {
            throw new InvalidOperationException("Thread must be created first. Call WithThread() or WithDefaultThread().");
        }
    }
}
