using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Models.Ban.PostingRestrictions;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Enums;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Ban;

internal sealed class GetPostingRestrictionStatusTests : IntegrationTestBase
{
    private static async Task<long> SeedBasicDataAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        var builder = new TestDataBuilder(scope)
            .WithDefaultAdmin()
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);
        return builder.LastThread.Id;
    }

    private static async Task<long> SeedDataWithBanAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        var builder = new TestDataBuilder(scope)
            .WithDefaultAdmin()
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "192.168.1.100", isOriginalPost: true)
            .WithExactBan("192.168.1.100", "you are banned");

        await builder.SaveAsync(cancellationToken);
        return builder.LastThread.Id;
    }

    private static async Task<long> SeedClosedThreadDataAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        var builder = new TestDataBuilder(scope)
            .WithDefaultAdmin()
            .WithDefaultCategory()
            .WithDefaultThread(isClosed: true)
            .WithPost("test post", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);
        return builder.LastThread.Id;
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetPostingRestrictionStatus_WhenNoRestrictions_ReturnsSuccess(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var threadId = await SeedBasicDataAsync(appScope.ServiceScope, cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();
        var ip = IPAddress.Parse("127.0.0.1");

        // Act
        var result = await repository.GetPostingRestrictionStatusAsync(new PostingRestrictionsRequestModel
        {
            CategoryAlias = "b",
            ThreadId = threadId,
            UserIpAddress = ip.GetAddressBytes(),
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.InstanceOf<PostingRestrictionsResponseSuccessModel>());
        Assert.That(result.RestrictionType, Is.EqualTo(PostingRestrictionType.NoRestriction));

        var successResult = (PostingRestrictionsResponseSuccessModel)result;
        Assert.That(successResult.ThreadSalt, Is.Not.Null);
        Assert.That(successResult.IsClosed, Is.False);
        Assert.That(successResult.BumpLimit, Is.GreaterThan(0));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetPostingRestrictionStatus_WhenUserIsBanned_ReturnsBanModel(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var threadId = await SeedDataWithBanAsync(appScope.ServiceScope, cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();
        var ip = IPAddress.Parse("192.168.1.100");

        // Act
        var result = await repository.GetPostingRestrictionStatusAsync(new PostingRestrictionsRequestModel
        {
            CategoryAlias = "b",
            ThreadId = threadId,
            UserIpAddress = ip.GetAddressBytes(),
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.InstanceOf<PostingRestrictionsResponseBanModel>());
        Assert.That(result.RestrictionType, Is.EqualTo(PostingRestrictionType.IpAddressBanned));

        var banResult = (PostingRestrictionsResponseBanModel)result;
        Assert.That(banResult.RestrictionReason, Is.EqualTo("you are banned"));
        Assert.That(banResult.RestrictionEndsAt, Is.Not.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetPostingRestrictionStatus_WhenCategoryNotFound_ReturnsFailure(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        await SeedBasicDataAsync(appScope.ServiceScope, cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();
        var ip = IPAddress.Parse("127.0.0.1");

        // Act
        var result = await repository.GetPostingRestrictionStatusAsync(new PostingRestrictionsRequestModel
        {
            CategoryAlias = "nonexistent",
            ThreadId = null,
            UserIpAddress = ip.GetAddressBytes(),
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.InstanceOf<PostingRestrictionsResponseFailureModel>());
        Assert.That(result.RestrictionType, Is.EqualTo(PostingRestrictionType.CategoryNotFound));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetPostingRestrictionStatus_WhenThreadNotFound_ReturnsFailure(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        await SeedBasicDataAsync(appScope.ServiceScope, cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();
        var ip = IPAddress.Parse("127.0.0.1");

        // Act
        var result = await repository.GetPostingRestrictionStatusAsync(new PostingRestrictionsRequestModel
        {
            CategoryAlias = "b",
            ThreadId = 999999,
            UserIpAddress = ip.GetAddressBytes(),
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.InstanceOf<PostingRestrictionsResponseFailureModel>());
        Assert.That(result.RestrictionType, Is.EqualTo(PostingRestrictionType.ThreadNotFound));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetPostingRestrictionStatus_WhenIpAddressIsNull_ReturnsIpNotFound(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        await SeedBasicDataAsync(appScope.ServiceScope, cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var result = await repository.GetPostingRestrictionStatusAsync(new PostingRestrictionsRequestModel
        {
            CategoryAlias = "b",
            ThreadId = null,
            UserIpAddress = null,
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.InstanceOf<PostingRestrictionsResponseFailureModel>());
        Assert.That(result.RestrictionType, Is.EqualTo(PostingRestrictionType.IpAddressNotFound));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetPostingRestrictionStatus_WhenThreadIsClosed_ReturnsThreadClosed(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var threadId = await SeedClosedThreadDataAsync(appScope.ServiceScope, cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();
        var ip = IPAddress.Parse("127.0.0.1");

        // Act
        var result = await repository.GetPostingRestrictionStatusAsync(new PostingRestrictionsRequestModel
        {
            CategoryAlias = "b",
            ThreadId = threadId,
            UserIpAddress = ip.GetAddressBytes(),
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.InstanceOf<PostingRestrictionsResponseFailureModel>());
        Assert.That(result.RestrictionType, Is.EqualTo(PostingRestrictionType.ThreadClosed));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetPostingRestrictionStatus_WhenCreatingNewThread_ReturnsSuccessWithNullThreadSalt(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        await SeedBasicDataAsync(appScope.ServiceScope, cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();
        var ip = IPAddress.Parse("127.0.0.1");

        // Act
        var result = await repository.GetPostingRestrictionStatusAsync(new PostingRestrictionsRequestModel
        {
            CategoryAlias = "b",
            ThreadId = null,
            UserIpAddress = ip.GetAddressBytes(),
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.InstanceOf<PostingRestrictionsResponseSuccessModel>());
        Assert.That(result.RestrictionType, Is.EqualTo(PostingRestrictionType.NoRestriction));

        var successResult = (PostingRestrictionsResponseSuccessModel)result;
        Assert.That(successResult.ThreadSalt, Is.Null);
        Assert.That(successResult.PostCount, Is.EqualTo(0));
    }
}
