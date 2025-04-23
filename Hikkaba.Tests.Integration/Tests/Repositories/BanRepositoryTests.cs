using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Constants;
using Hikkaba.Shared.Enums;
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
internal sealed class BanRepositoryTests
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
    [TestCase("176.213.241.52", true)]
    [TestCase("b550:f112:2801:51d4:fdaf:21d8:6bbc:aaba", true)]
    [TestCase("176.213.241.53", false)]
    [TestCase("e226:df4a:8eb6:99b3:7dad:affa:5560:39d3", false)]
    public async Task ListBansPaginatedAsync_WhenSearchExact_ReturnsExpectedResult(
        string ipAddress,
        bool expectedFound,
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

        var board = new Board();
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
            ShowCountry = false,
            ShowOs = false,
            ShowBrowser = false,
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

        var post1 = new Post
        {
            IsOriginalPost = true,
            BlobContainerId = new Guid("05E219F7-35F2-495B-A0D3-D7EF7018C674"),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            IsSageEnabled = false,
            MessageText = "test post 1 abc",
            MessageHtml = "test post 1 abc",
            UserIpAddress = IPAddress.Parse("176.213.241.52").GetAddressBytes(),
            UserAgent = "Firefox",
            ThreadLocalUserHash = hashService.GetHashBytes(thread.Salt, IPAddress.Parse("176.213.241.52").GetAddressBytes()),
            Thread = thread,
        };
        var post2 = new Post
        {
            IsOriginalPost = false,
            BlobContainerId = new Guid("EADF6C08-1C14-432E-A9EB-0DDF67D55FC7"),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            IsSageEnabled = false,
            MessageText = "test post 2 def",
            MessageHtml = "test post 2 def",
            UserIpAddress = IPAddress.Parse("b550:f112:2801:51d4:fdaf:21d8:6bbc:aaba").GetAddressBytes(),
            UserAgent = "Chrome",
            ThreadLocalUserHash = hashService.GetHashBytes(thread.Salt, IPAddress.Parse("b550:f112:2801:51d4:fdaf:21d8:6bbc:aaba").GetAddressBytes()),
            Thread = thread,
        };
        dbContext.Posts.AddRange(post1, post2);

        var ban1 = new Ban
        {
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            EndsAt = timeProvider.GetUtcNow().UtcDateTime.AddYears(99),
            IpAddressType = IpAddressType.IpV4,
            BannedIpAddress = IPAddress.Parse("176.213.241.52").GetAddressBytes(),
            Reason = "ban reason 1",
            CreatedBy = admin,
        };
        var ban2 = new Ban
        {
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            EndsAt = timeProvider.GetUtcNow().UtcDateTime.AddYears(99),
            IpAddressType = IpAddressType.IpV6,
            BannedIpAddress = IPAddress.Parse("b550:f112:2801:51d4:fdaf:21d8:6bbc:aaba").GetAddressBytes(),
            Reason = "ban reason 2",
            CreatedBy = admin,
        };
        dbContext.Bans.AddRange(ban1, ban2);

        await dbContext.SaveChangesAsync(cancellationToken);

        var repository = scope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var result = await repository.ListBansPaginatedAsync(new BanPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [new OrderByItem { Field = nameof(Post.CreatedAt), Direction = OrderByDirection.Desc }],
            IpAddress = IPAddress.Parse(ipAddress),
        }, cancellationToken);

        // Assert
        var any = result.Data.Count != 0;
        Assert.That(any, Is.EqualTo(expectedFound));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase("176.213.224.37", true)]
    [TestCase("2001:4860:0000:bbbb:0000:0000:0000:0", true)]
    [TestCase("95.189.128.0", false)]
    [TestCase("e226:df4a:8eb6:99b3:7dad:affa:5560:39d3", false)]
    public async Task ListBansPaginatedAsync_WhenSearchInRange_ReturnsExpectedResult(
        string ipAddress,
        bool expectedFound,
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

        var board = new Board();
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
            ShowCountry = false,
            ShowOs = false,
            ShowBrowser = false,
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

        var post1 = new Post
        {
            IsOriginalPost = true,
            BlobContainerId = new Guid("64596344-BC44-489A-9D6E-1AA2BB5A27BF"),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            IsSageEnabled = false,
            MessageText = "test post 1 abc",
            MessageHtml = "test post 1 abc",
            UserIpAddress = IPAddress.Parse("176.213.224.37").GetAddressBytes(),
            UserAgent = "Firefox",
            ThreadLocalUserHash = hashService.GetHashBytes(thread.Salt, IPAddress.Parse("176.213.224.37").GetAddressBytes()),
            Thread = thread,
        };
        var post2 = new Post
        {
            IsOriginalPost = false,
            BlobContainerId = new Guid("9BC6094D-DD51-4C59-8EAB-444446DEEF62"),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            IsSageEnabled = false,
            MessageText = "test post 2 def",
            MessageHtml = "test post 2 def",
            UserIpAddress = IPAddress.Parse("2001:4860:0000:0000:0000:0000:ffff:0").GetAddressBytes(),
            UserAgent = "Chrome",
            ThreadLocalUserHash = hashService.GetHashBytes(thread.Salt, IPAddress.Parse("2001:4860:0000:0000:0000:0000:ffff:0").GetAddressBytes()),
            Thread = thread,
        };
        dbContext.Posts.AddRange(post1, post2);

        var ban1 = new Ban
        {
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            EndsAt = timeProvider.GetUtcNow().UtcDateTime.AddYears(99),
            IpAddressType = IpAddressType.IpV4,
            BannedIpAddress = IPAddress.Parse("176.213.224.40").GetAddressBytes(),
            BannedCidrLowerIpAddress = IPAddress.Parse("176.213.224.1").GetAddressBytes(),
            BannedCidrUpperIpAddress = IPAddress.Parse("176.213.224.254").GetAddressBytes(),
            Reason = "ban reason 1",
            CreatedBy = admin,
        };
        var ban2 = new Ban
        {
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            EndsAt = timeProvider.GetUtcNow().UtcDateTime.AddYears(99),
            IpAddressType = IpAddressType.IpV6,
            BannedIpAddress = IPAddress.Parse("2001:4860:0000:0000:ffff:0000:0000:0").GetAddressBytes(),
            BannedCidrLowerIpAddress = IPAddress.Parse("2001:4860:0000:0000:0000:0000:0000:0").GetAddressBytes(),
            BannedCidrUpperIpAddress = IPAddress.Parse("2001:4860:ffff:ffff:ffff:ffff:ffff:ffff").GetAddressBytes(),
            Reason = "ban reason 2",
            CreatedBy = admin,
        };
        dbContext.Bans.AddRange(ban1, ban2);

        await dbContext.SaveChangesAsync(cancellationToken);

        var repository = scope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var result = await repository.ListBansPaginatedAsync(new BanPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [new OrderByItem { Field = nameof(Post.CreatedAt), Direction = OrderByDirection.Desc }],
            IpAddress = IPAddress.Parse(ipAddress),
        }, cancellationToken);

        // Assert
        var any = result.Data.Count != 0;
        Assert.That(any, Is.EqualTo(expectedFound));
    }
}
