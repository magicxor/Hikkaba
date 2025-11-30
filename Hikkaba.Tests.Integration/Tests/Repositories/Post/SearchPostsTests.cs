using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Post;
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
using Microsoft.Extensions.Logging;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Post;

[TestFixture]
[Parallelizable(scope: ParallelScope.Fixtures)]
internal sealed class SearchPostsTests
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

    private static async Task SeedSearchPostsDataAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        await new PostTestDataBuilder(scope)
            .WithDefaultAdmin()
            .WithCategory("b", "category")
            .WithThread("thread")
            .WithPost("post", "127.0.0.1", "Firefox", isOriginalPost: true)
            .WithPost("blah blah blah", "127.0.0.1", "Chrome")
            .WithPost("blah blah post blah", "127.0.0.1", "Chrome")
            .WithPost("blah blah post blah", "127.0.0.1", "Chrome", isDeleted: true)
            .SaveAsync(cancellationToken);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase("category", 0)] // we only search by post content and thread title
    [TestCase("thread", 1, Ignore = "Temporary disabled due to ongoing query performance improvements")] // search by thread title is temporarily disabled
    [TestCase("post", 2)] // only 2 non-deleted posts are returned
    [TestCase("blah", 2)] // only 2 non-deleted posts are returned
    [TestCase("hedgehog", 0)] // no results
    public async Task SearchPosts_WhenSearchQueryIsProvided_ReturnsExpectedResultsAsync(
        string searchQuery,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        await SeedSearchPostsDataAsync(appScope.Scope, cancellationToken);

        var dbContext = appScope.Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = appScope.Scope.ServiceProvider.GetRequiredService<ILogger<SearchPostsTests>>();
        await DbUtils.WaitForFulltextIndexAsync(logger, dbContext, ["Posts", "Threads"], cancellationToken: cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act
        var result = await repository.SearchPostsAsync(new SearchPostsPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Desc }],
            SearchQuery = searchQuery,
        }, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(expectedCount));
    }
}
