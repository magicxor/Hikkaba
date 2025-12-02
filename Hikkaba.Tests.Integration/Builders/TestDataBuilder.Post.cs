using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Blake3;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Entities.Attachments;

namespace Hikkaba.Tests.Integration.Builders;

internal sealed partial class TestDataBuilder
{
    private readonly List<Post> _posts = [];
    private Post? _lastPost;

    public IReadOnlyList<Post> Posts => _posts;
    public Post LastPost => _lastPost ?? throw new InvalidOperationException("No post created yet.");
    public long LastPostId => _lastPost?.Id ?? throw new InvalidOperationException("No post created yet.");

    /// <summary>
    ///     Creates a post in the last created thread.
    /// </summary>
    public TestDataBuilder WithPost(
        string messageText,
        string ipAddress = "127.0.0.1",
        string userAgent = "Firefox",
        bool isOriginalPost = false,
        bool isSageEnabled = false,
        bool isDeleted = false,
        TimeSpan? createdAtOffset = null,
        Guid? blobContainerId = null)
    {
        EnsureThreadExists();
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
            ThreadLocalUserHash = HashService.GetHashBytes(LastThread.Salt, ip.GetAddressBytes()),
            Thread = LastThread,
        };
        _dbContext.Posts.Add(post);
        _posts.Add(post);
        _lastPost = post;
        return this;
    }

    /// <summary>
    ///     Creates a post that mentions (replies to) another post.
    ///     <list type="bullet">
    ///         <item>inThreadTitle: specifies which thread to create the post in (default: last thread)</item>
    ///         <item>No mention args: mentions the last post in the last thread</item>
    ///         <item>mentionedPostMessageText: finds post by message in the target thread</item>
    ///         <item>mentionedThreadTitle: finds thread by title, mentions its OP (or post by mentionedPostMessageText)</item>
    ///         <item>mentionedCategoryAlias: finds category, then thread, then post</item>
    ///     </list>
    /// </summary>
    public TestDataBuilder WithPostThatMentionsPost(
        string messageText,
        string? inThreadTitle = null,
        string? mentionedPostMessageText = null,
        string? mentionedThreadTitle = null,
        string? mentionedCategoryAlias = null,
        string ipAddress = "127.0.0.1",
        string userAgent = "Firefox",
        bool isOriginalPost = false)
    {
        EnsureThreadExists();

        // Determine which thread to create the post in
        var targetThread = inThreadTitle != null ? GetThread(inThreadTitle) : LastThread;

        // Determine the mentioned post based on provided parameters
        var mentionedPost = FindMentionedPost(mentionedPostMessageText, mentionedThreadTitle, mentionedCategoryAlias);

        var ip = IPAddress.Parse(ipAddress);
        var post = new Post
        {
            IsOriginalPost = isOriginalPost,
            IsDeleted = false,
            BlobContainerId = _guidGenerator.GenerateSeededGuid(),
            CreatedAt = TimeProvider.GetUtcNow().UtcDateTime,
            IsSageEnabled = false,
            MessageText = messageText,
            MessageHtml = messageText,
            UserIpAddress = ip.GetAddressBytes(),
            UserAgent = userAgent,
            ThreadLocalUserHash = HashService.GetHashBytes(targetThread.Salt, ip.GetAddressBytes()),
            Thread = targetThread,
        };
        _dbContext.Posts.Add(post);
        _posts.Add(post);
        _lastPost = post;

        // Create PostToReply relationship
        var postToReply = new PostToReply
        {
            Post = mentionedPost,
            Reply = post,
        };
        _dbContext.PostsToReplies.Add(postToReply);

        return this;
    }

    private Post FindMentionedPost(string? mentionedPostMessageText, string? mentionedThreadTitle, string? mentionedCategoryAlias)
    {
        // Determine target category (compare by reference, entities are not saved yet)
        Category targetCategory;
        if (mentionedCategoryAlias != null)
        {
            targetCategory = _categories.FirstOrDefault(c => c.Alias == mentionedCategoryAlias)
                ?? throw new InvalidOperationException($"Category with alias '{mentionedCategoryAlias}' not found");
        }
        else
        {
            targetCategory = LastCategory;
        }

        // Determine target thread (compare by reference to category)
        Thread targetThread;
        if (mentionedThreadTitle != null)
        {
            targetThread = _threads.FirstOrDefault(t => ReferenceEquals(t.Category, targetCategory) && t.Title == mentionedThreadTitle)
                ?? throw new InvalidOperationException($"Thread with title '{mentionedThreadTitle}' not found in category '{targetCategory.Alias}'");
        }
        else
        {
            targetThread = LastThread;
        }

        // Determine target post (compare by reference to thread)
        Post targetPost;
        if (mentionedPostMessageText != null)
        {
            targetPost = _posts.FirstOrDefault(p => ReferenceEquals(p.Thread, targetThread) && p.MessageText == mentionedPostMessageText)
                ?? throw new InvalidOperationException($"Post with message '{mentionedPostMessageText}' not found in thread '{targetThread.Title}'");
        }
        else if (mentionedThreadTitle != null)
        {
            // If thread title specified but no post text, use OP
            targetPost = _posts.FirstOrDefault(p => ReferenceEquals(p.Thread, targetThread) && p.IsOriginalPost)
                ?? throw new InvalidOperationException($"OP post not found in thread '{targetThread.Title}'");
        }
        else
        {
            // No parameters - use last post in last thread
            targetPost = _posts.LastOrDefault(p => ReferenceEquals(p.Thread, targetThread))
                ?? throw new InvalidOperationException($"No posts found in thread '{targetThread.Title}'");
        }

        return targetPost;
    }

    /// <summary>
    ///     Creates a post with audio attachment in the last created thread.
    /// </summary>
    public TestDataBuilder WithPostWithAudio(
        string messageText,
        string audioFileName,
        string ipAddress = "127.0.0.1",
        string userAgent = "Chrome",
        TimeSpan? createdAtOffset = null,
        Guid? blobContainerId = null,
        Guid? audioBlobId = null)
    {
        EnsureThreadExists();
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
            ThreadLocalUserHash = HashService.GetHashBytes(LastThread.Salt, ip.GetAddressBytes()),
            Thread = LastThread,
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
        _posts.Add(post);
        _lastPost = post;
        return this;
    }

    /// <summary>
    ///     Creates a post with picture attachment in the last created thread.
    /// </summary>
    public TestDataBuilder WithPostWithPicture(
        string messageText,
        string pictureFileName,
        string ipAddress = "127.0.0.1",
        string userAgent = "Chrome",
        TimeSpan? createdAtOffset = null,
        Guid? blobContainerId = null,
        Guid? pictureBlobId = null)
    {
        EnsureThreadExists();
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
            ThreadLocalUserHash = HashService.GetHashBytes(LastThread.Salt, ip.GetAddressBytes()),
            Thread = LastThread,
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
        _posts.Add(post);
        _lastPost = post;
        return this;
    }

    public TestDataBuilder WithModifiedBy(ApplicationUser modifiedBy)
    {
        EnsureLastPostExists();
        _lastPost!.ModifiedBy = modifiedBy;
        _lastPost.ModifiedAt = TimeProvider.GetUtcNow().UtcDateTime;
        return this;
    }

    private void EnsureLastPostExists()
    {
        if (_lastPost == null)
        {
            throw new InvalidOperationException("Post must be created first. Call WithPost() or WithPostInThread().");
        }
    }
}
