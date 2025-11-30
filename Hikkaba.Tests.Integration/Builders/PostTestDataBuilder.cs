using System;
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
    private static readonly GuidGenerator GuidGenerator = new();

    private readonly ApplicationDbContext _dbContext;
    private readonly IHashService _hashService;
    private readonly TimeProvider _timeProvider;

    private ApplicationUser? _admin;
    private Category? _category;
    private Thread? _thread;

    public PostTestDataBuilder(IServiceScope scope)
    {
        _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _hashService = scope.ServiceProvider.GetRequiredService<IHashService>();
        _timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
    }

    public ApplicationUser Admin => _admin ?? throw new InvalidOperationException("Admin not created. Call WithDefaultAdmin() first.");
    public Category Category => _category ?? throw new InvalidOperationException("Category not created. Call WithCategory() first.");
    public Thread Thread => _thread ?? throw new InvalidOperationException("Thread not created. Call WithThread() first.");

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

    public PostTestDataBuilder WithCategory(string alias, string name)
    {
        EnsureAdminExists();

        _category = new Category
        {
            IsDeleted = false,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            ModifiedAt = null,
            Alias = alias,
            Name = name,
            IsHidden = false,
            DefaultBumpLimit = 500,
            ShowThreadLocalUserHash = false,
            MaxThreadCount = Defaults.MaxThreadCountInCategory,
            CreatedBy = Admin,
        };
        _dbContext.Categories.Add(_category);
        return this;
    }

    public PostTestDataBuilder WithThread(string title)
    {
        EnsureCategoryExists();

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        _thread = new Thread
        {
            CreatedAt = utcNow,
            LastBumpAt = utcNow,
            Title = title,
            IsPinned = false,
            IsClosed = false,
            BumpLimit = 500,
            Salt = GuidGenerator.GenerateSeededGuid(),
            Category = Category,
        };
        _dbContext.Threads.Add(_thread);
        return this;
    }

    public PostTestDataBuilder WithPost(
        Guid blobContainerId,
        string messageText,
        string ipAddress,
        string userAgent,
        bool isOriginalPost = false,
        bool isDeleted = false)
    {
        EnsureThreadExists();

        var ip = IPAddress.Parse(ipAddress);
        var post = new Post
        {
            IsOriginalPost = isOriginalPost,
            IsDeleted = isDeleted,
            BlobContainerId = blobContainerId,
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
