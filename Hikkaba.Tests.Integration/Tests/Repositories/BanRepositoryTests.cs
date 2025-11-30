using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Hikkaba.Tests.Integration.Extensions;
using Hikkaba.Tests.Integration.Models;
using Hikkaba.Tests.Integration.Services;
using Hikkaba.Tests.Integration.Utils;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories;

[TestFixture]
[Parallelizable(scope: ParallelScope.Fixtures)]
internal sealed class BanRepositoryTests
{
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
    private async Task<IAppScope> CreateAppScopeAsync(CancellationToken cancellationToken)
    {
        var connectionString = await _contextManager!.CreateRespawnedDbConnectionStringAsync();
        var customAppFactory = new CustomAppFactory(connectionString);

        var scope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if ((await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        return new AppScope
        {
            Scope = scope,
            AppFactory = customAppFactory,
        };
    }

    private static async Task SeedExactBansDataAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        await new BanTestDataBuilder(scope)
            .WithDefaultAdmin()
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost(new Guid("05E219F7-35F2-495B-A0D3-D7EF7018C674"), "176.213.241.52", "Firefox", isOriginalPost: true)
            .WithPost(new Guid("EADF6C08-1C14-432E-A9EB-0DDF67D55FC7"), "b550:f112:2801:51d4:fdaf:21d8:6bbc:aaba", "Chrome")
            .WithExactBan("176.213.241.52", "ban reason 1")
            .WithExactBan("b550:f112:2801:51d4:fdaf:21d8:6bbc:aaba", "ban reason 2")
            .SaveAsync(cancellationToken);
    }

    private static async Task SeedRangeBansDataAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        await new BanTestDataBuilder(scope)
            .WithDefaultAdmin()
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost(new Guid("64596344-BC44-489A-9D6E-1AA2BB5A27BF"), "176.213.224.37", "Firefox", isOriginalPost: true)
            .WithPost(new Guid("9BC6094D-DD51-4C59-8EAB-444446DEEF62"), "2001:4860:0000:0000:0000:0000:ffff:0", "Chrome")
            .WithRangeBan("176.213.224.40", "176.213.224.1", "176.213.224.254", "ban reason 1")
            .WithRangeBan("2001:4860:0000:0000:ffff:0000:0000:0", "2001:4860:0000:0000:0000:0000:0000:0", "2001:4860:ffff:ffff:ffff:ffff:ffff:ffff", "ban reason 2")
            .SaveAsync(cancellationToken);
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
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        await SeedExactBansDataAsync(appScope.Scope, cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IBanRepository>();

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
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        await SeedRangeBansDataAsync(appScope.Scope, cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IBanRepository>();

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
