using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
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

namespace Hikkaba.Tests.Integration.Tests.Repositories;

[TestFixture]
[Parallelizable(scope: ParallelScope.Fixtures)]
internal sealed class PostRepositoryTests
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
            .WithCategory("b", "Random CategorySearchTerm")
            .WithThread("BoardThreadPostSearchTerm thread 1 ThreadAndPostSearchTerm")
            .WithPost(new Guid("243D7DB4-4EE8-4285-8888-E7185A7CB1B2"), "BoardThreadPostSearchTerm post 1 Post1SearchTerm", "127.0.0.1", "Firefox", isOriginalPost: true)
            .WithPost(new Guid("D9AED982-37D6-4C5C-B235-E1AADC342236"), "BoardThreadPostSearchTerm post 2 Post2SearchTerm", "127.0.0.1", "Chrome")
            .WithPost(new Guid("C8393E45-20AE-4214-A1EF-5F6AE0D93477"), "BoardThreadPostSearchTerm Post1SearchTerm Post2SearchTerm BoardSearchTerm CategorySearchTerm ThreadAndPostSearchTerm", "127.0.0.1", "Chrome", isDeleted: true)
            .SaveAsync(cancellationToken);
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
        using var seedResult = await CreateAppScopeAsync(cancellationToken);
        await SeedSearchPostsDataAsync(seedResult.Scope, cancellationToken);

        var dbContext = seedResult.Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = seedResult.Scope.ServiceProvider.GetRequiredService<ILogger<PostRepositoryTests>>();
        await DbUtils.WaitForFulltextIndexAsync(logger, dbContext, ["Posts", "Threads"], cancellationToken: cancellationToken);

        var repository = seedResult.Scope.ServiceProvider.GetRequiredService<IPostRepository>();

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
