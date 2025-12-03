using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Hikkaba.Tests.Integration.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Ban;

internal sealed class SetBanDeletedTests : IntegrationTestBase
{
    private static async Task<(int banId, int adminId)> SeedBanDataAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        var builder = new TestDataBuilder(scope)
            .WithDefaultAdmin()
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "192.168.1.100", isOriginalPost: true)
            .WithExactBan("192.168.1.100", "test ban reason");

        await builder.SaveAsync(cancellationToken);
        return (builder.LastBanId, builder.Admin.Id);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetBanDeleted_WhenSettingToDeleted_MarksBanAsDeleted(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var (banId, adminId) = await SeedBanDataAsync(appScope.ServiceScope, cancellationToken);
        UserContextUtils.SetupUserContext(appScope.ServiceScope, adminId);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Verify ban is not deleted initially
        var banBefore = await repository.GetBanAsync(banId, cancellationToken);
        Assert.That(banBefore, Is.Not.Null);
        Assert.That(banBefore!.IsDeleted, Is.False);

        // Act
        await repository.SetBanDeletedAsync(banId, true, cancellationToken);

        // Assert
        var banAfter = await repository.GetBanAsync(banId, cancellationToken);
        Assert.That(banAfter, Is.Not.Null);
        Assert.That(banAfter!.IsDeleted, Is.True);
        Assert.That(banAfter.ModifiedAt, Is.Not.Null);
        Assert.That(banAfter.ModifiedById, Is.EqualTo(adminId));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetBanDeleted_WhenRestoringDeletedBan_MarksBanAsNotDeleted(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "10.0.0.1", isOriginalPost: true)
            .WithExactBan("10.0.0.1", "deleted ban", isDeleted: true);

        await builder.SaveAsync(cancellationToken);
        UserContextUtils.SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Verify ban is deleted initially
        var banBefore = await repository.GetBanAsync(builder.LastBanId, cancellationToken);
        Assert.That(banBefore, Is.Not.Null);
        Assert.That(banBefore!.IsDeleted, Is.True);

        // Act
        await repository.SetBanDeletedAsync(builder.LastBanId, false, cancellationToken);

        // Assert
        var banAfter = await repository.GetBanAsync(builder.LastBanId, cancellationToken);
        Assert.That(banAfter, Is.Not.Null);
        Assert.That(banAfter!.IsDeleted, Is.False);
        Assert.That(banAfter.ModifiedAt, Is.Not.Null);
        Assert.That(banAfter.ModifiedById, Is.EqualTo(builder.Admin.Id));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetBanDeleted_WhenBanAlreadyDeleted_UpdatesModifiedAtAndModifiedBy(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var (banId, adminId) = await SeedBanDataAsync(appScope.ServiceScope, cancellationToken);
        UserContextUtils.SetupUserContext(appScope.ServiceScope, adminId);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Delete the ban first
        await repository.SetBanDeletedAsync(banId, true, cancellationToken);

        // Wait a tiny bit to ensure time difference
        await Task.Delay(10, cancellationToken);

        // Act - delete again
        await repository.SetBanDeletedAsync(banId, true, cancellationToken);

        // Assert
        var banAfterSecondDelete = await repository.GetBanAsync(banId, cancellationToken);
        Assert.That(banAfterSecondDelete, Is.Not.Null);
        Assert.That(banAfterSecondDelete!.IsDeleted, Is.True);

        // ModifiedAt should be updated even if IsDeleted status doesn't change
        Assert.That(banAfterSecondDelete.ModifiedAt, Is.Not.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetBanDeleted_WhenDeleted_BanNotFoundByFindActiveBan(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var (banId, adminId) = await SeedBanDataAsync(appScope.ServiceScope, cancellationToken);
        UserContextUtils.SetupUserContext(appScope.ServiceScope, adminId);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();
        var ip = IPAddress.Parse("192.168.1.100");

        // Verify ban is found before deletion
        var activeBanBefore = await repository.FindActiveBanAsync(new ActiveBanFilter
        {
            UserIpAddress = ip.GetAddressBytes(),
        }, cancellationToken);
        Assert.That(activeBanBefore, Is.Not.Null);

        // Act
        await repository.SetBanDeletedAsync(banId, true, cancellationToken);

        // Assert - ban should not be found after deletion
        var activeBanAfter = await repository.FindActiveBanAsync(new ActiveBanFilter
        {
            UserIpAddress = ip.GetAddressBytes(),
        }, cancellationToken);

        Assert.That(activeBanAfter, Is.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task SetBanDeleted_WhenRestored_BanFoundByFindActiveBan(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "172.16.0.1", isOriginalPost: true)
            .WithExactBan("172.16.0.1", "ban to restore", isDeleted: true);

        await builder.SaveAsync(cancellationToken);
        UserContextUtils.SetupUserContext(appScope.ServiceScope, builder.Admin.Id);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();
        var ip = IPAddress.Parse("172.16.0.1");

        // Verify ban is NOT found before restoration
        var activeBanBefore = await repository.FindActiveBanAsync(new ActiveBanFilter
        {
            UserIpAddress = ip.GetAddressBytes(),
        }, cancellationToken);
        Assert.That(activeBanBefore, Is.Null);

        // Act
        await repository.SetBanDeletedAsync(builder.LastBanId, false, cancellationToken);

        // Assert - ban should be found after restoration
        var activeBanAfter = await repository.FindActiveBanAsync(new ActiveBanFilter
        {
            UserIpAddress = ip.GetAddressBytes(),
        }, cancellationToken);

        Assert.That(activeBanAfter, Is.Not.Null);
        Assert.That(activeBanAfter!.Reason, Is.EqualTo("ban to restore"));
    }
}
