using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Ban;

internal sealed class FindActiveBanTests : IntegrationTestBase
{
    private static async Task SeedExactBansDataAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        await new BanTestDataBuilder(scope)
            .WithDefaultAdmin()
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("176.213.241.52", "Firefox", isOriginalPost: true)
            .WithExactBan("176.213.241.52", "exact ban reason")
            .SaveAsync(cancellationToken);
    }

    private static async Task SeedRangeBansDataAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        await new BanTestDataBuilder(scope)
            .WithDefaultAdmin()
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("176.213.224.37", "Firefox", isOriginalPost: true)
            .WithRangeBan("176.213.224.40", "176.213.224.1", "176.213.224.254", "range ban reason")
            .SaveAsync(cancellationToken);
    }

    private static async Task SeedCategoryBanDataAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        await new BanTestDataBuilder(scope)
            .WithDefaultAdmin()
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("192.168.1.100", "Chrome", isOriginalPost: true)
            .WithExactBan("192.168.1.100", "category ban reason", inCategory: true)
            .SaveAsync(cancellationToken);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase("176.213.241.52", true, "exact ban reason")]
    [TestCase("176.213.241.53", false, null)]
    [TestCase("10.0.0.1", false, null)]
    public async Task FindActiveBan_WhenSearchingByExactIp_ReturnsExpectedResult(
        string ipAddress,
        bool expectedFound,
        string? expectedReason,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        await SeedExactBansDataAsync(appScope.Scope, cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IBanRepository>();
        var ip = IPAddress.Parse(ipAddress);

        // Act
        var result = await repository.FindActiveBanAsync(new ActiveBanFilter
        {
            UserIpAddress = ip.GetAddressBytes(),
        }, cancellationToken);

        // Assert
        if (expectedFound)
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Reason, Is.EqualTo(expectedReason));
            Assert.That(result.Id, Is.GreaterThan(0));
            Assert.That(result.EndsAt, Is.Not.Null);
        }
        else
        {
            Assert.That(result, Is.Null);
        }
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase("176.213.224.37", true, "range ban reason")]
    [TestCase("176.213.224.100", true, "range ban reason")]
    [TestCase("176.213.224.254", true, "range ban reason")]
    [TestCase("176.213.225.1", false, null)]
    [TestCase("10.0.0.1", false, null)]
    public async Task FindActiveBan_WhenSearchingInRange_ReturnsExpectedResult(
        string ipAddress,
        bool expectedFound,
        string? expectedReason,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        await SeedRangeBansDataAsync(appScope.Scope, cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IBanRepository>();
        var ip = IPAddress.Parse(ipAddress);

        // Act
        var result = await repository.FindActiveBanAsync(new ActiveBanFilter
        {
            UserIpAddress = ip.GetAddressBytes(),
        }, cancellationToken);

        // Assert
        if (expectedFound)
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Reason, Is.EqualTo(expectedReason));
        }
        else
        {
            Assert.That(result, Is.Null);
        }
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task FindActiveBan_WhenSearchingWithCategoryAlias_ReturnsBanForCategory(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        await SeedCategoryBanDataAsync(appScope.Scope, cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IBanRepository>();
        var ip = IPAddress.Parse("192.168.1.100");

        // Act
        var resultWithCategory = await repository.FindActiveBanAsync(new ActiveBanFilter
        {
            UserIpAddress = ip.GetAddressBytes(),
            CategoryAlias = "b",
        }, cancellationToken);

        var resultWithDifferentCategory = await repository.FindActiveBanAsync(new ActiveBanFilter
        {
            UserIpAddress = ip.GetAddressBytes(),
            CategoryAlias = "a",
        }, cancellationToken);

        var resultSystemWide = await repository.FindActiveBanAsync(new ActiveBanFilter
        {
            UserIpAddress = ip.GetAddressBytes(),
        }, cancellationToken);

        // Assert
        Assert.That(resultWithCategory, Is.Not.Null);
        Assert.That(resultWithCategory!.Reason, Is.EqualTo("category ban reason"));

        Assert.That(resultWithDifferentCategory, Is.Null);
        Assert.That(resultSystemWide, Is.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task FindActiveBan_WhenBanIsDeleted_ReturnsNull(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        await new BanTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("192.168.1.50", "Firefox", isOriginalPost: true)
            .WithExactBan("192.168.1.50", "deleted ban", isDeleted: true)
            .SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IBanRepository>();
        var ip = IPAddress.Parse("192.168.1.50");

        // Act
        var result = await repository.FindActiveBanAsync(new ActiveBanFilter
        {
            UserIpAddress = ip.GetAddressBytes(),
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task FindActiveBan_WhenBanIsExpired_ReturnsNull(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        await new BanTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("192.168.1.60", "Firefox", isOriginalPost: true)
            .WithExactBan("192.168.1.60", "expired ban", isExpired: true)
            .SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IBanRepository>();
        var ip = IPAddress.Parse("192.168.1.60");

        // Act
        var result = await repository.FindActiveBanAsync(new ActiveBanFilter
        {
            UserIpAddress = ip.GetAddressBytes(),
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
    }
}
