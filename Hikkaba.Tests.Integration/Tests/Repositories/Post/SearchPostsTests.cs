using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Post;

internal sealed class SearchPostsTests : IntegrationTestBase
{
    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase("category", 0)] // we only search by post content and thread title
    [TestCase("thread", 1, Ignore = "Temporary disabled due to ongoing query performance improvements")] // search by thread title is temporarily disabled
    [TestCase("post", 2)] // only 2 non-deleted posts are returned
    [TestCase("capybara", 2)] // only 2 non-deleted posts are returned
    [TestCase("hedgehog", 0)] // no results
    public async Task SearchPosts_WhenSearchQueryIsProvided_ReturnsExpectedResultsAsync(
        string searchQuery,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "category")
            .WithThread("thread")
            .WithPost("post", isOriginalPost: true)
            .WithPost("capybara capybara capybara", "127.0.0.1", "Chrome")
            .WithPost("capybara capybara post capybara", "127.0.0.1", "Chrome")
            .WithPost("capybara capybara post capybara", "127.0.0.1", "Chrome", isDeleted: true)
            .SaveAsync(cancellationToken);

        PagedResult<PostDetailsModel> result = null!;
        var attempt = 0;

        // retry 10 times in case fulltext index is not ready yet
        while (attempt < 50)
        {
            using var actScope = appScope.ServiceScope.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var repository = actScope.ServiceProvider.GetRequiredService<IPostRepository>();

            // Act
            result = await repository.SearchPostsAsync(new SearchPostsPagingFilter
            {
                PageNumber = 1,
                PageSize = 10,
                OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Desc }],
                SearchQuery = searchQuery,
            }, cancellationToken);

            if (result.Data.Count == expectedCount)
            {
                break;
            }
            else
            {
                attempt++;
                await Task.Delay(500, cancellationToken);
            }
        }

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(expectedCount), $"Expected {expectedCount} results, but got {result.Data.Count} after {attempt} attempts.");
    }
}
