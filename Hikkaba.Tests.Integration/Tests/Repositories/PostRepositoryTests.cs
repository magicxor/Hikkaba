using System;
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
using Thread = Hikkaba.Data.Entities.Thread;

namespace Hikkaba.Tests.Integration.Tests.Repositories;

[TestFixture]
[Parallelizable(scope: ParallelScope.Fixtures)]
public sealed class PostRepositoryTests
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
    [TestCase("Fizz", 0)]
    [TestCase("Foo", 0)]
    [TestCase("Buzz", 1)]
    [TestCase("test", 2)]
    [TestCase("abc", 1)]
    [TestCase("def", 1)]
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
            MaxThreadCount = Defaults.MaxThreadCountInCategory,
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
            Salt = GuidGenerator.GenerateSeededGuid(),
            Category = category,
            CreatedBy = null,
        };
        dbContext.Threads.Add(thread);

        var post1 = new Post
        {
            BlobContainerId = new Guid("243D7DB4-4EE8-4285-8888-E7185A7CB1B2"),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            IsSageEnabled = false,
            MessageText = "test post 1 abc",
            MessageHtml = "test post 1 abc",
            UserIpAddress = IPAddress.Parse("127.0.0.1").GetAddressBytes(),
            UserAgent = "Firefox",
            ThreadLocalUserHash = hashService.GetHashBytes(thread.Salt, IPAddress.Parse("127.0.0.1").GetAddressBytes()),
            Thread = thread,
        };
        var post2 = new Post
        {
            BlobContainerId = new Guid("D9AED982-37D6-4C5C-B235-E1AADC342236"),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            IsSageEnabled = false,
            MessageText = "test post 2 def",
            MessageHtml = "test post 2 def",
            UserIpAddress = IPAddress.Parse("127.0.0.1").GetAddressBytes(),
            UserAgent = "Chrome",
            ThreadLocalUserHash = hashService.GetHashBytes(thread.Salt, IPAddress.Parse("127.0.0.1").GetAddressBytes()),
            Thread = thread,
        };
        var post3 = new Post
        {
            BlobContainerId = new Guid("C8393E45-20AE-4214-A1EF-5F6AE0D93477"),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            IsDeleted = true,
            IsSageEnabled = false,
            MessageText = "test abc def Fizz Foo Buzz",
            MessageHtml = "test abc def Fizz Foo Buzz",
            UserIpAddress = IPAddress.Parse("127.0.0.1").GetAddressBytes(),
            UserAgent = "Chrome",
            ThreadLocalUserHash = hashService.GetHashBytes(thread.Salt, IPAddress.Parse("127.0.0.1").GetAddressBytes()),
            Thread = thread,
        };
        dbContext.AddRange(post1, post2, post3);

        await dbContext.SaveChangesAsync(cancellationToken);

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
