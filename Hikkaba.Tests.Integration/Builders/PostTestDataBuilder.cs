using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
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
        foreach (var mentionedPostId in mentionedPostIds)
        {
            var postToReply = new PostToReply
            {
                PostId = mentionedPostId,
                Reply = post,
            };
            _dbContext.PostsToReplies.Add(postToReply);
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
