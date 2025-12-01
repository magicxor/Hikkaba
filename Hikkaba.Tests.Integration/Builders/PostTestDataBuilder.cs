using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Entities.Attachments;
using Hikkaba.Shared.Constants;
using Hikkaba.Tests.Integration.Utils;
using Microsoft.Extensions.DependencyInjection;
using Thread = Hikkaba.Data.Entities.Thread;

namespace Hikkaba.Tests.Integration.Builders;

internal sealed class PostTestDataBuilder
{
    private readonly GuidGenerator _guidGenerator = new();

    private readonly ApplicationDbContext _dbContext;
    private readonly IHashService _hashService;
    private readonly TimeProvider _timeProvider;

    private readonly List<Post> _posts = new();

    private ApplicationUser? _admin;
    private Category? _category;
    private Thread? _thread;
    private Post? _lastPost;

    public PostTestDataBuilder(IServiceScope scope)
    {
        _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _hashService = scope.ServiceProvider.GetRequiredService<IHashService>();
        _timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
    }

    public ApplicationUser Admin => _admin ?? throw new InvalidOperationException("Admin not created. Call WithDefaultAdmin() first.");
    public Category Category => _category ?? throw new InvalidOperationException("Category not created. Call WithCategory() first.");
    public Thread Thread => _thread ?? throw new InvalidOperationException("Thread not created. Call WithThread() first.");
    public long LastPostId => _lastPost?.Id ?? throw new InvalidOperationException("No post created yet.");
    public Post LastPost => _lastPost ?? throw new InvalidOperationException("No post created yet.");
    public IReadOnlyList<Post> Posts => _posts;

    public PostTestDataBuilder WithDefaultAdmin()
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
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
        };
        _dbContext.Users.Add(_admin);
        return this;
    }

    public PostTestDataBuilder WithCategory(string alias, string name, bool isHidden = false, bool isDeleted = false)
    {
        EnsureAdminExists();

        _category = new Category
        {
            IsDeleted = isDeleted,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            ModifiedAt = null,
            Alias = alias,
            Name = name,
            IsHidden = isHidden,
            DefaultBumpLimit = 500,
            ShowThreadLocalUserHash = false,
            MaxThreadCount = Defaults.MaxThreadCountInCategory,
            CreatedBy = Admin,
        };
        _dbContext.Categories.Add(_category);
        return this;
    }

    public PostTestDataBuilder WithThread(string title, bool isClosed = false, bool isDeleted = false, int bumpLimit = 500, bool isCyclic = false)
    {
        EnsureCategoryExists();

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        _thread = new Thread
        {
            CreatedAt = utcNow,
            LastBumpAt = utcNow,
            Title = title,
            IsPinned = false,
            IsClosed = isClosed,
            IsDeleted = isDeleted,
            BumpLimit = bumpLimit,
            IsCyclic = isCyclic,
            Salt = _guidGenerator.GenerateSeededGuid(),
            Category = Category,
        };
        _dbContext.Threads.Add(_thread);
        return this;
    }

    public PostTestDataBuilder WithPost(
        string messageText,
        string ipAddress,
        string userAgent,
        bool isOriginalPost = false,
        bool isDeleted = false,
        bool isSageEnabled = false,
        Guid? blobContainerId = null)
    {
        EnsureThreadExists();

        var ip = IPAddress.Parse(ipAddress);
        var post = new Post
        {
            IsOriginalPost = isOriginalPost,
            IsDeleted = isDeleted,
            BlobContainerId = blobContainerId ?? _guidGenerator.GenerateSeededGuid(),
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            IsSageEnabled = isSageEnabled,
            MessageText = messageText,
            MessageHtml = messageText,
            UserIpAddress = ip.GetAddressBytes(),
            UserAgent = userAgent,
            ThreadLocalUserHash = _hashService.GetHashBytes(Thread.Salt, ip.GetAddressBytes()),
            Thread = Thread,
        };
        _dbContext.Posts.Add(post);
        _lastPost = post;
        _posts.Add(post);
        return this;
    }

    public PostTestDataBuilder WithPostReplyingTo(
        string messageText,
        string ipAddress,
        string userAgent,
        IReadOnlyList<long> mentionedPostIds)
    {
        EnsureThreadExists();

        var ip = IPAddress.Parse(ipAddress);
        var post = new Post
        {
            IsOriginalPost = false,
            IsDeleted = false,
            BlobContainerId = _guidGenerator.GenerateSeededGuid(),
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            IsSageEnabled = false,
            MessageText = messageText,
            MessageHtml = messageText,
            UserIpAddress = ip.GetAddressBytes(),
            UserAgent = userAgent,
            ThreadLocalUserHash = _hashService.GetHashBytes(Thread.Salt, ip.GetAddressBytes()),
            Thread = Thread,
        };
        _dbContext.Posts.Add(post);
        _lastPost = post;
        _posts.Add(post);

        // Create replies to mentioned posts (they must be saved first to have IDs)
        var postsToReplies = mentionedPostIds.Select(mentionedPostId => new PostToReply
        {
            PostId = mentionedPostId,
            Reply = post,
        });
        _dbContext.PostsToReplies.AddRange(postsToReplies);

        return this;
    }

    public PostTestDataBuilder WithModifiedBy(ApplicationUser modifiedBy)
    {
        EnsureLastPostExists();
        _lastPost!.ModifiedBy = modifiedBy;
        _lastPost.ModifiedAt = _timeProvider.GetUtcNow().UtcDateTime;
        return this;
    }

    public PostTestDataBuilder WithAudio(
        string fileNameWithoutExtension = "test_audio",
        string fileExtension = ".mp3",
        long fileSize = 1024,
        string fileContentType = "audio/mpeg",
        string? title = "Test Title",
        string? album = "Test Album",
        string? artist = "Test Artist",
        int? durationSeconds = 180)
    {
        EnsureLastPostExists();
        var audio = new Audio
        {
            BlobId = _guidGenerator.GenerateSeededGuid(),
            Post = _lastPost!,
            FileNameWithoutExtension = fileNameWithoutExtension,
            FileExtension = fileExtension,
            FileSize = fileSize,
            FileContentType = fileContentType,
            FileHash = new byte[32],
            Title = title,
            Album = album,
            Artist = artist,
            DurationSeconds = durationSeconds,
        };
        _dbContext.Audios.Add(audio);
        _lastPost!.Audios.Add(audio);
        return this;
    }

    public PostTestDataBuilder WithDocument(
        string fileNameWithoutExtension = "test_document",
        string fileExtension = ".pdf",
        long fileSize = 2048,
        string fileContentType = "application/pdf")
    {
        EnsureLastPostExists();
        var document = new Document
        {
            BlobId = _guidGenerator.GenerateSeededGuid(),
            Post = _lastPost!,
            FileNameWithoutExtension = fileNameWithoutExtension,
            FileExtension = fileExtension,
            FileSize = fileSize,
            FileContentType = fileContentType,
            FileHash = new byte[32],
        };
        _dbContext.Documents.Add(document);
        _lastPost!.Documents.Add(document);
        return this;
    }

    public PostTestDataBuilder WithNotice(string text)
    {
        EnsureLastPostExists();
        EnsureAdminExists();
        var notice = new Notice
        {
            BlobId = _guidGenerator.GenerateSeededGuid(),
            Post = _lastPost!,
            Text = text,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            CreatedBy = _admin!,
        };
        _dbContext.Notices.Add(notice);
        _lastPost!.Notices.Add(notice);
        return this;
    }

    public PostTestDataBuilder WithPicture(
        string fileNameWithoutExtension = "test_picture",
        string fileExtension = ".jpg",
        long fileSize = 4096,
        string fileContentType = "image/jpeg",
        int width = 800,
        int height = 600,
        string thumbnailExtension = ".jpg",
        int thumbnailWidth = 200,
        int thumbnailHeight = 150)
    {
        EnsureLastPostExists();
        var picture = new Picture
        {
            BlobId = _guidGenerator.GenerateSeededGuid(),
            Post = _lastPost!,
            FileNameWithoutExtension = fileNameWithoutExtension,
            FileExtension = fileExtension,
            FileSize = fileSize,
            FileContentType = fileContentType,
            FileHash = new byte[32],
            Width = width,
            Height = height,
            ThumbnailExtension = thumbnailExtension,
            ThumbnailWidth = thumbnailWidth,
            ThumbnailHeight = thumbnailHeight,
        };
        _dbContext.Pictures.Add(picture);
        _lastPost!.Pictures.Add(picture);
        return this;
    }

    public PostTestDataBuilder WithVideo(
        string fileNameWithoutExtension = "test_video",
        string fileExtension = ".mp4",
        long fileSize = 8192,
        string fileContentType = "video/mp4")
    {
        EnsureLastPostExists();
        var video = new Video
        {
            BlobId = _guidGenerator.GenerateSeededGuid(),
            Post = _lastPost!,
            FileNameWithoutExtension = fileNameWithoutExtension,
            FileExtension = fileExtension,
            FileSize = fileSize,
            FileContentType = fileContentType,
            FileHash = new byte[32],
        };
        _dbContext.Videos.Add(video);
        _lastPost!.Videos.Add(video);
        return this;
    }

    /// <summary>
    /// Creates a post in the current thread that replies to a post in another thread.
    /// The post will have a PostToReply record where this post is the Reply (ReplyId).
    /// </summary>
    public PostTestDataBuilder WithPostReplyingToCrossThread(
        long mentionedPostIdInOtherThread,
        string messageText = "Reply to post in another thread",
        string ipAddress = "127.0.0.50",
        string userAgent = "CrossThreadReplyAgent")
    {
        EnsureThreadExists();

        var ip = IPAddress.Parse(ipAddress);
        var post = new Post
        {
            IsOriginalPost = false,
            IsDeleted = false,
            BlobContainerId = _guidGenerator.GenerateSeededGuid(),
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            IsSageEnabled = false,
            MessageText = messageText,
            MessageHtml = messageText,
            UserIpAddress = ip.GetAddressBytes(),
            UserAgent = userAgent,
            ThreadLocalUserHash = _hashService.GetHashBytes(Thread.Salt, ip.GetAddressBytes()),
            Thread = Thread,
        };
        _dbContext.Posts.Add(post);
        _lastPost = post;
        _posts.Add(post);

        var postToReply = new PostToReply
        {
            PostId = mentionedPostIdInOtherThread,
            Reply = post,
        };
        _dbContext.PostsToReplies.Add(postToReply);

        return this;
    }

    /// <summary>
    /// Creates a new thread with an OP post that replies to a post in the current thread.
    /// The new post will have a PostToReply record where the specified post is the mentioned post (PostId).
    /// </summary>
    public PostTestDataBuilder WithCrossThreadReplyToPost(
        long mentionedPostId,
        string threadTitle = "Cross-thread reply thread")
    {
        EnsureCategoryExists();

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var crossThread = new Thread
        {
            CreatedAt = utcNow,
            LastBumpAt = utcNow,
            Title = threadTitle,
            IsPinned = false,
            IsClosed = false,
            IsDeleted = false,
            BumpLimit = 500,
            IsCyclic = false,
            Salt = _guidGenerator.GenerateSeededGuid(),
            Category = Category,
        };
        _dbContext.Threads.Add(crossThread);

        var ip = IPAddress.Parse("192.168.100.1");
        var crossThreadPost = new Post
        {
            IsOriginalPost = true,
            IsDeleted = false,
            BlobContainerId = _guidGenerator.GenerateSeededGuid(),
            CreatedAt = utcNow,
            IsSageEnabled = false,
            MessageText = $"Cross-thread reply to post {mentionedPostId}",
            MessageHtml = $"Cross-thread reply to post {mentionedPostId}",
            UserIpAddress = ip.GetAddressBytes(),
            UserAgent = "CrossThreadAgent",
            ThreadLocalUserHash = _hashService.GetHashBytes(crossThread.Salt, ip.GetAddressBytes()),
            Thread = crossThread,
        };
        _dbContext.Posts.Add(crossThreadPost);

        var postToReply = new PostToReply
        {
            PostId = mentionedPostId,
            Reply = crossThreadPost,
        };
        _dbContext.PostsToReplies.Add(postToReply);

        return this;
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private void EnsureLastPostExists()
    {
        if (_lastPost == null)
        {
            throw new InvalidOperationException("Post must be created first. Call WithPost().");
        }
    }

    private void EnsureAdminExists()
    {
        if (_admin == null)
        {
            throw new InvalidOperationException("Admin must be created first. Call WithDefaultAdmin().");
        }
    }

    private void EnsureCategoryExists()
    {
        if (_category == null)
        {
            throw new InvalidOperationException("Category must be created first. Call WithCategory().");
        }
    }

    private void EnsureThreadExists()
    {
        if (_thread == null)
        {
            throw new InvalidOperationException("Thread must be created first. Call WithThread().");
        }
    }
}
