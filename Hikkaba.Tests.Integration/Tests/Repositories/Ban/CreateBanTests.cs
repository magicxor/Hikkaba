using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Enums;
using Hikkaba.Shared.Models;
using Hikkaba.Shared.Services.Contracts;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Ban;

internal sealed class CreateBanTests : IntegrationTestBase
{
    private static async Task<(long threadId, long postId, int adminId)> SeedBasicDataAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        var builder = new TestDataBuilder(scope)
            .WithDefaultAdmin()
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "192.168.1.100", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);
        return (builder.LastThread.Id, builder.LastPostId, builder.Admin.Id);
    }

    private static void SetupUserContext(IServiceScope scope, int adminId)
    {
        var userContext = scope.ServiceProvider.GetRequiredService<IUserContext>();
        userContext.SetUser(new CurrentUser
        {
            Id = adminId,
            UserName = "admin",
            Roles = ["Administrator"],
            ModeratedCategories = [],
        });
    }

    private static DateTime GetBanEndsAt(IServiceScope scope)
    {
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
        return timeProvider.GetUtcNow().UtcDateTime.AddDays(7);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateBan_WhenValidRequest_CreatesBanSuccessfully(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var (threadId, postId, adminId) = await SeedBasicDataAsync(appScope.ServiceScope, cancellationToken);
        SetupUserContext(appScope.ServiceScope, adminId);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();
        var ip = IPAddress.Parse("192.168.1.100");

        // Act
        var result = await repository.CreateBanAsync(new BanCreateRequestModel
        {
            EndsAt = GetBanEndsAt(appScope.ServiceScope),
            IpAddressType = IpAddressType.IpV4,
            BannedIpAddress = ip.GetAddressBytes(),
            BannedCidrLowerIpAddress = null,
            BannedCidrUpperIpAddress = null,
            BanInAllCategories = true,
            CountryIsoCode = null,
            AutonomousSystemNumber = null,
            AutonomousSystemOrganization = null,
            AdditionalAction = BanAdditionalAction.None,
            Reason = "Test ban reason",
            RelatedPostId = postId,
            RelatedThreadId = threadId,
            CategoryAlias = "b",
        }, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True, "Expected success result");
        var successResult = result.AsT0;
        Assert.That(successResult.BanId, Is.GreaterThan(0));

        // Verify ban was created in DB
        var ban = await repository.GetBanAsync(successResult.BanId, cancellationToken);
        Assert.That(ban, Is.Not.Null);
        Assert.That(ban!.Reason, Is.EqualTo("Test ban reason"));
        Assert.That(ban.CreatedById, Is.EqualTo(adminId));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateBan_WhenBanForSamePostExists_ReturnsConflictError(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var (threadId, postId, adminId) = await SeedBasicDataAsync(appScope.ServiceScope, cancellationToken);
        SetupUserContext(appScope.ServiceScope, adminId);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();
        var ip = IPAddress.Parse("192.168.1.100");

        var request = new BanCreateRequestModel
        {
            EndsAt = GetBanEndsAt(appScope.ServiceScope),
            IpAddressType = IpAddressType.IpV4,
            BannedIpAddress = ip.GetAddressBytes(),
            BannedCidrLowerIpAddress = null,
            BannedCidrUpperIpAddress = null,
            BanInAllCategories = true,
            CountryIsoCode = null,
            AutonomousSystemNumber = null,
            AutonomousSystemOrganization = null,
            AdditionalAction = BanAdditionalAction.None,
            Reason = "First ban",
            RelatedPostId = postId,
            RelatedThreadId = threadId,
            CategoryAlias = "b",
        };

        // Create first ban
        await repository.CreateBanAsync(request, cancellationToken);

        // Act - try to create second ban for the same post
        request.Reason = "Second ban";
        var result = await repository.CreateBanAsync(request, cancellationToken);

        // Assert
        Assert.That(result.IsT1, Is.True, "Expected error result");
        var error = result.AsT1;
        Assert.That(error.StatusCode, Is.EqualTo(409)); // Conflict
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateBan_WhenCategorySpecific_CreatesBanForCategory(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var (threadId, _, adminId) = await SeedBasicDataAsync(appScope.ServiceScope, cancellationToken);
        SetupUserContext(appScope.ServiceScope, adminId);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();
        var ip = IPAddress.Parse("10.0.0.1");

        // Act
        var result = await repository.CreateBanAsync(new BanCreateRequestModel
        {
            EndsAt = GetBanEndsAt(appScope.ServiceScope),
            IpAddressType = IpAddressType.IpV4,
            BannedIpAddress = ip.GetAddressBytes(),
            BannedCidrLowerIpAddress = null,
            BannedCidrUpperIpAddress = null,
            BanInAllCategories = false,
            CountryIsoCode = null,
            AutonomousSystemNumber = null,
            AutonomousSystemOrganization = null,
            AdditionalAction = BanAdditionalAction.None,
            Reason = "Category-specific ban",
            RelatedPostId = null,
            RelatedThreadId = threadId,
            CategoryAlias = "b",
        }, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True, "Expected success result");
        var successResult = result.AsT0;

        var ban = await repository.GetBanAsync(successResult.BanId, cancellationToken);
        Assert.That(ban, Is.Not.Null);
        Assert.That(ban!.CategoryAlias, Is.EqualTo("b"));
        Assert.That(ban.CategoryId, Is.Not.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateBan_WithDeletePostAction_DeletesRelatedPost(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var (threadId, postId, adminId) = await SeedBasicDataAsync(appScope.ServiceScope, cancellationToken);
        SetupUserContext(appScope.ServiceScope, adminId);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var ip = IPAddress.Parse("192.168.1.100");

        // Act
        var result = await repository.CreateBanAsync(new BanCreateRequestModel
        {
            EndsAt = GetBanEndsAt(appScope.ServiceScope),
            IpAddressType = IpAddressType.IpV4,
            BannedIpAddress = ip.GetAddressBytes(),
            BannedCidrLowerIpAddress = null,
            BannedCidrUpperIpAddress = null,
            BanInAllCategories = true,
            CountryIsoCode = null,
            AutonomousSystemNumber = null,
            AutonomousSystemOrganization = null,
            AdditionalAction = BanAdditionalAction.DeletePost,
            Reason = "Ban with post deletion",
            RelatedPostId = postId,
            RelatedThreadId = threadId,
            CategoryAlias = "b",
        }, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True, "Expected success result");

        var post = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);
        Assert.That(post, Is.Not.Null);
        Assert.That(post!.IsDeleted, Is.True);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateBan_WithRangeBan_CreatesBanWithCidrRange(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var (threadId, _, adminId) = await SeedBasicDataAsync(appScope.ServiceScope, cancellationToken);
        SetupUserContext(appScope.ServiceScope, adminId);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();
        var ip = IPAddress.Parse("192.168.1.50");
        var lowerIp = IPAddress.Parse("192.168.1.1");
        var upperIp = IPAddress.Parse("192.168.1.254");

        // Act
        var result = await repository.CreateBanAsync(new BanCreateRequestModel
        {
            EndsAt = GetBanEndsAt(appScope.ServiceScope),
            IpAddressType = IpAddressType.IpV4,
            BannedIpAddress = ip.GetAddressBytes(),
            BannedCidrLowerIpAddress = lowerIp.GetAddressBytes(),
            BannedCidrUpperIpAddress = upperIp.GetAddressBytes(),
            BanInAllCategories = true,
            CountryIsoCode = null,
            AutonomousSystemNumber = null,
            AutonomousSystemOrganization = null,
            AdditionalAction = BanAdditionalAction.None,
            Reason = "Range ban",
            RelatedPostId = null,
            RelatedThreadId = threadId,
            CategoryAlias = "b",
        }, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True, "Expected success result");
        var successResult = result.AsT0;

        var ban = await repository.GetBanAsync(successResult.BanId, cancellationToken);
        Assert.That(ban, Is.Not.Null);
        Assert.That(ban!.BannedCidrLowerIpAddress, Is.Not.Null);
        Assert.That(ban.BannedCidrUpperIpAddress, Is.Not.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateBan_WithDeleteAllPostsInThread_DeletesAllPostsFromSameIp(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        // First, create posts from the IP that will be banned
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "192.168.1.100", isOriginalPost: true)
            .WithPost("test post", "192.168.1.100", "Chrome");

        await builder.SaveAsync(cancellationToken);

        // Save the post ID from the banned IP
        var bannedIpPostId = builder.LastPostId;

        // Add post from different IP (should not be deleted)
        builder.WithPost("test post", "192.168.1.200", "Safari");
        await builder.SaveAsync(cancellationToken);

        SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var ip = IPAddress.Parse("192.168.1.100");

        // Act
        var result = await repository.CreateBanAsync(new BanCreateRequestModel
        {
            EndsAt = GetBanEndsAt(appScope.ServiceScope),
            IpAddressType = IpAddressType.IpV4,
            BannedIpAddress = ip.GetAddressBytes(),
            BannedCidrLowerIpAddress = null,
            BannedCidrUpperIpAddress = null,
            BanInAllCategories = true,
            CountryIsoCode = null,
            AutonomousSystemNumber = null,
            AutonomousSystemOrganization = null,
            AdditionalAction = BanAdditionalAction.DeleteAllPostsInThread,
            Reason = "Ban with all posts deletion",
            RelatedPostId = bannedIpPostId,
            RelatedThreadId = builder.LastThread.Id,
            CategoryAlias = "b",
        }, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True, "Expected success result");

        var posts = await dbContext.Posts
            .IgnoreQueryFilters()
            .Where(p => p.ThreadId == builder.LastThread.Id)
            .ToListAsync(cancellationToken);

        // Posts from 192.168.1.100 should be deleted
        var deletedPosts = posts.Where(p => p.UserIpAddress != null && p.UserIpAddress.SequenceEqual(ip.GetAddressBytes())).ToList();
        Assert.That(deletedPosts, Has.Count.EqualTo(2));
        Assert.That(deletedPosts.All(p => p.IsDeleted), Is.True);

        // Post from different IP should not be deleted
        var otherIp = IPAddress.Parse("192.168.1.200").GetAddressBytes();
        var notDeletedPost = posts.First(p => p.UserIpAddress != null && p.UserIpAddress.SequenceEqual(otherIp));
        Assert.That(notDeletedPost.IsDeleted, Is.False);
    }
}
