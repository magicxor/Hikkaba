using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Post;
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
using Microsoft.Extensions.Logging;
using Thread = Hikkaba.Data.Entities.Thread;

namespace Hikkaba.Tests.Integration.Tests.Repositories;

[TestFixture]
[Parallelizable(scope: ParallelScope.Fixtures)]
internal sealed class PostRepositoryTests
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
    [TestCase("BoardSearchTerm", 0)]
    [TestCase("CategorySearchTerm", 0)]
    [TestCase("ThreadAndPostSearchTerm", 1, Ignore = "Temporary disabled due to ongoing query performance improvements")]
    [TestCase("BoardThreadPostSearchTerm", 2)]
    [TestCase("Post1SearchTerm", 1)]
    [TestCase("Post2SearchTerm", 1)]
    public async Task SearchPostsPaginatedAsync_WhenSearchQueryIsProvided_ReturnsExpectedResultsAsync(
        string searchQuery,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        // Arrange
        await using var customAppFactory = await CreateAppFactoryAsync();
        using var scope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hashService = scope.ServiceProvider.GetRequiredService<IHashService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<PostRepositoryTests>>();

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
            Name = "BoardThreadPostSearchTerm Board BoardSearchTerm",
        };
        dbContext.Boards.Add(board);

        var category = new Category
        {
            IsDeleted = false,
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            ModifiedAt = null,
            Alias = "b",
            Name = "Random CategorySearchTerm",
            IsHidden = false,
            DefaultBumpLimit = 500,
            ShowThreadLocalUserHash = false,
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
            Title = "BoardThreadPostSearchTerm thread 1 ThreadAndPostSearchTerm",
            IsPinned = false,
            IsClosed = false,
            BumpLimit = 500,
            Salt = GuidGenerator.GenerateSeededGuid(),
            Category = category,
        };
        dbContext.Threads.Add(thread);

        var post1 = new Post
        {
            IsOriginalPost = true,
            BlobContainerId = new Guid("243D7DB4-4EE8-4285-8888-E7185A7CB1B2"),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            IsSageEnabled = false,
            MessageText = "BoardThreadPostSearchTerm post 1 Post1SearchTerm",
            MessageHtml = "BoardThreadPostSearchTerm post 1 Post1SearchTerm",
            UserIpAddress = IPAddress.Parse("127.0.0.1").GetAddressBytes(),
            UserAgent = "Firefox",
            ThreadLocalUserHash = hashService.GetHashBytes(thread.Salt, IPAddress.Parse("127.0.0.1").GetAddressBytes()),
            Thread = thread,
        };
        var post2 = new Post
        {
            IsOriginalPost = false,
            BlobContainerId = new Guid("D9AED982-37D6-4C5C-B235-E1AADC342236"),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            IsSageEnabled = false,
            MessageText = "BoardThreadPostSearchTerm post 2 Post2SearchTerm",
            MessageHtml = "BoardThreadPostSearchTerm post 2 Post2SearchTerm",
            UserIpAddress = IPAddress.Parse("127.0.0.1").GetAddressBytes(),
            UserAgent = "Chrome",
            ThreadLocalUserHash = hashService.GetHashBytes(thread.Salt, IPAddress.Parse("127.0.0.1").GetAddressBytes()),
            Thread = thread,
        };
        var post3 = new Post
        {
            IsOriginalPost = false,
            BlobContainerId = new Guid("C8393E45-20AE-4214-A1EF-5F6AE0D93477"),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            IsDeleted = true,
            IsSageEnabled = false,
            MessageText = "BoardThreadPostSearchTerm Post1SearchTerm Post2SearchTerm BoardSearchTerm CategorySearchTerm ThreadAndPostSearchTerm",
            MessageHtml = "BoardThreadPostSearchTerm Post1SearchTerm Post2SearchTerm BoardSearchTerm CategorySearchTerm ThreadAndPostSearchTerm",
            UserIpAddress = IPAddress.Parse("127.0.0.1").GetAddressBytes(),
            UserAgent = "Chrome",
            ThreadLocalUserHash = hashService.GetHashBytes(thread.Salt, IPAddress.Parse("127.0.0.1").GetAddressBytes()),
            Thread = thread,
        };
        IReadOnlyList<Post> allPosts = [post1, post2, post3];
        dbContext.Posts.AddRange(allPosts);

        var latestPostWithoutSage = allPosts
            .Where(x => x is { IsSageEnabled: false, IsDeleted: false })
            .MaxBy(x => x.CreatedAt);

        await dbContext.SaveChangesAsync(cancellationToken);

        await DbUtils.WaitForFulltextIndexAsync(logger, dbContext, ["Posts", "Threads"], cancellationToken: cancellationToken);

        var repository = scope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act
        var result = await repository.SearchPostsPaginatedAsync(new SearchPostsPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [new OrderByItem { Field = nameof(Post.CreatedAt), Direction = OrderByDirection.Desc }],
            SearchQuery = searchQuery,
        }, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(expectedCount));
    }
}
