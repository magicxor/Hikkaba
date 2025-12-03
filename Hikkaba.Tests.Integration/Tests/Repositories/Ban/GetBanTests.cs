using System;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Enums;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Ban;

internal sealed class GetBanTests : IntegrationTestBase
{
    private static async Task<int> SeedExactBanDataAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        var builder = new TestDataBuilder(scope)
            .WithDefaultAdmin()
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "176.213.241.52", isOriginalPost: true)
            .WithExactBan("176.213.241.52", "test ban reason");

        await builder.SaveAsync(cancellationToken);
        return builder.LastBanId;
    }

    private static async Task<int> SeedRangeBanDataAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        var builder = new TestDataBuilder(scope)
            .WithDefaultAdmin()
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "192.168.1.1", "Chrome", isOriginalPost: true)
            .WithRangeBan("192.168.1.50", "192.168.1.1", "192.168.1.254", "range ban reason");

        await builder.SaveAsync(cancellationToken);
        return builder.LastBanId;
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetBan_WhenBanExists_ReturnsBanDetails(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var banId = await SeedExactBanDataAsync(appScope.ServiceScope, cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var result = await repository.GetBanAsync(banId, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(banId));
        Assert.That(result.Reason, Is.EqualTo("test ban reason"));
        Assert.That(result.IsDeleted, Is.False);
        Assert.That(result.IpAddressType, Is.EqualTo(IpAddressType.IpV4));
        Assert.That(result.BannedIpAddress, Is.Not.Null);
        Assert.That(result.BannedCidrLowerIpAddress, Is.Null);
        Assert.That(result.BannedCidrUpperIpAddress, Is.Null);
        Assert.That(result.CreatedAt, Is.Not.EqualTo(default(DateTime)));
        Assert.That(result.CreatedById, Is.GreaterThan(0));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetBan_WhenRangeBanExists_ReturnsBanWithCidrRange(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var banId = await SeedRangeBanDataAsync(appScope.ServiceScope, cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var result = await repository.GetBanAsync(banId, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(banId));
        Assert.That(result.Reason, Is.EqualTo("range ban reason"));
        Assert.That(result.BannedCidrLowerIpAddress, Is.Not.Null);
        Assert.That(result.BannedCidrUpperIpAddress, Is.Not.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetBan_WhenBanDoesNotExist_ReturnsNull(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        await SeedExactBanDataAsync(appScope.ServiceScope, cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var result = await repository.GetBanAsync(999999, cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetBan_WhenBanIsDeleted_StillReturnsBan(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "10.0.0.1", isOriginalPost: true)
            .WithExactBan("10.0.0.1", "deleted ban reason", isDeleted: true);

        await builder.SaveAsync(cancellationToken);
        var banId = builder.LastBanId;

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var result = await repository.GetBanAsync(banId, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(banId));
        Assert.That(result.IsDeleted, Is.True);
        Assert.That(result.Reason, Is.EqualTo("deleted ban reason"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetBan_WhenBanHasCategoryAlias_ReturnsCategoryAlias(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "172.16.0.1", isOriginalPost: true)
            .WithExactBan("172.16.0.1", "category ban reason", inCategory: true);

        await builder.SaveAsync(cancellationToken);
        var banId = builder.LastBanId;

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var result = await repository.GetBanAsync(banId, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.CategoryAlias, Is.EqualTo("b"));
        Assert.That(result.CategoryId, Is.Not.Null);
    }
}
