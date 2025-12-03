using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Constants;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Hikkaba.Tests.Integration.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Ban;

internal sealed class ListBansTests : IntegrationTestBase
{
    #region IpAddress filter tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase("176.213.241.52", true)]
    [TestCase("b550:f112:2801:51d4:fdaf:21d8:6bbc:aaba", true)]
    [TestCase("176.213.241.53", false)]
    [TestCase("e226:df4a:8eb6:99b3:7dad:affa:5560:39d3", false)]
    public async Task ListBans_WhenSearchExact_ReturnsExpectedResult(
        string ipAddress,
        bool expectedFound,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "176.213.241.52", isOriginalPost: true)
            .WithPost("test post", "b550:f112:2801:51d4:fdaf:21d8:6bbc:aaba", "Chrome")
            .WithExactBan("176.213.241.52", "ban reason 1")
            .WithExactBan("b550:f112:2801:51d4:fdaf:21d8:6bbc:aaba", "ban reason 2")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var result = await repository.ListBansAsync(new BanPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Desc }],
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
    public async Task ListBans_WhenSearchInRange_ReturnsExpectedResult(
        string ipAddress,
        bool expectedFound,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "176.213.224.37", isOriginalPost: true)
            .WithPost("test post", "2001:4860:0000:0000:0000:0000:ffff:0", "Chrome")
            .WithRangeBan("176.213.224.40", "176.213.224.1", "176.213.224.254", "ban reason 1")
            .WithRangeBan("2001:4860:0000:0000:ffff:0000:0000:0", "2001:4860:0000:0000:0000:0000:0000:0", "2001:4860:ffff:ffff:ffff:ffff:ffff:ffff", "ban reason 2")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var result = await repository.ListBansAsync(new BanPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Post.CreatedAt), Direction = OrderByDirection.Desc }],
            IpAddress = IPAddress.Parse(ipAddress),
        }, cancellationToken);

        // Assert
        var any = result.Data.Count != 0;
        Assert.That(any, Is.EqualTo(expectedFound));
    }

    #endregion

    #region IncludeDeleted filter tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase(true, 2)]
    [TestCase(false, 1)]
    public async Task ListBans_WhenIncludeDeleted_ReturnsExpectedCount(
        bool includeDeleted,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "192.168.1.1", isOriginalPost: true)
            .WithExactBan("192.168.1.1", "active ban")
            .WithExactBan("192.168.1.2", "deleted ban", isDeleted: true)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var result = await repository.ListBansAsync(new BanPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Ban.CreatedAt), Direction = OrderByDirection.Desc }],
            IncludeDeleted = includeDeleted,
        }, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(expectedCount));
    }

    #endregion

    #region Date filters tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase(-10, -5, 1)] // CreatedNotBefore 10 days ago, CreatedNotAfter 5 days ago -> 1 ban (7 days ago)
    [TestCase(-10, -8, 0)] // CreatedNotBefore 10 days ago, CreatedNotAfter 8 days ago -> 0 bans
    [TestCase(-16, -1, 1)] // CreatedNotBefore 6 days ago, CreatedNotAfter 1 day ago -> 1 ban (7 days ago)
    [TestCase(-3, 0, 0)] // CreatedNotBefore 3 days ago, CreatedNotAfter today -> 0 bans
    public async Task ListBans_WhenFilteringByCreatedDate_ReturnsExpectedCount(
        int createdNotBeforeDays,
        int createdNotAfterDays,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);
        var utcNow = builder.TimeProvider.GetUtcNow().UtcDateTime;

        await builder
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "192.168.1.1", isOriginalPost: true)
            .WithExactBan("192.168.1.1", "ban from 7 days ago", createdAtOffset: TimeSpan.FromDays(-7))
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var result = await repository.ListBansAsync(new BanPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Ban.CreatedAt), Direction = OrderByDirection.Desc }],
            CreatedNotBefore = utcNow.AddDays(createdNotBeforeDays),
            CreatedNotAfter = utcNow.AddDays(createdNotAfterDays),
        }, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(expectedCount));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase(5, 15, 1)] // EndsNotBefore 5 days from now, EndsNotAfter 15 days from now -> 1 ban (ends in 10 days)
    [TestCase(11, 20, 0)] // EndsNotBefore 11 days from now, EndsNotAfter 20 days from now -> 0 bans
    [TestCase(1, 9, 0)] // EndsNotBefore 1 day from now, EndsNotAfter 9 days from now -> 0 bans
    [TestCase(1, 15, 1)] // EndsNotBefore 1 day from now, EndsNotAfter 15 days from now -> 1 ban
    public async Task ListBans_WhenFilteringByEndsDate_ReturnsExpectedCount(
        int endsNotBeforeDays,
        int endsNotAfterDays,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);
        var utcNow = builder.TimeProvider.GetUtcNow().UtcDateTime;

        await builder
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "192.168.1.1", isOriginalPost: true)
            .WithExactBan("192.168.1.1", "ban ends in 10 days", endsAtOffset: TimeSpan.FromDays(10))
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var result = await repository.ListBansAsync(new BanPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Ban.CreatedAt), Direction = OrderByDirection.Desc }],
            EndsNotBefore = utcNow.AddDays(endsNotBeforeDays),
            EndsNotAfter = utcNow.AddDays(endsNotAfterDays),
        }, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(expectedCount));
    }

    #endregion

    #region CountryIsoCode filter tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase("US", 1)]
    [TestCase("RU", 1)]
    [TestCase("DE", 0)]
    [TestCase(null, 2)]
    public async Task ListBans_WhenFilteringByCountryIsoCode_ReturnsExpectedCount(
        string? countryIsoCode,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "192.168.1.1", isOriginalPost: true)
            .WithExactBan("192.168.1.1", "US ban", countryIsoCode: "US")
            .WithExactBan("192.168.1.2", "RU ban", countryIsoCode: "RU")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var result = await repository.ListBansAsync(new BanPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Ban.CreatedAt), Direction = OrderByDirection.Desc }],
            CountryIsoCode = countryIsoCode,
        }, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(expectedCount));
    }

    #endregion

    #region AutonomousSystemNumber filter tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase(15169, 1)] // Google ASN
    [TestCase(32934, 1)] // Facebook ASN
    [TestCase(99999, 0)] // Non-existent ASN
    [TestCase(null, 2)]
    public async Task ListBans_WhenFilteringByAutonomousSystemNumber_ReturnsExpectedCount(
        int? autonomousSystemNumber,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "192.168.1.1", isOriginalPost: true)
            .WithExactBan("192.168.1.1", "Google ASN ban", autonomousSystemNumber: 15169)
            .WithExactBan("192.168.1.2", "Facebook ASN ban", autonomousSystemNumber: 32934)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var result = await repository.ListBansAsync(new BanPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Ban.CreatedAt), Direction = OrderByDirection.Desc }],
            AutonomousSystemNumber = autonomousSystemNumber,
        }, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(expectedCount));
    }

    #endregion

    #region AutonomousSystemOrganization filter tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase("Google", 1)]
    [TestCase("Facebook", 1)]
    [TestCase("LLC", 2)] // Both contain LLC
    [TestCase("Microsoft", 0)]
    [TestCase(null, 2)]
    public async Task ListBans_WhenFilteringByAutonomousSystemOrganization_ReturnsExpectedCount(
        string? autonomousSystemOrganization,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "192.168.1.1", isOriginalPost: true)
            .WithExactBan("192.168.1.1", "Google ban", autonomousSystemOrganization: "Google LLC")
            .WithExactBan("192.168.1.2", "Facebook ban", autonomousSystemOrganization: "Facebook LLC")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var result = await repository.ListBansAsync(new BanPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Ban.CreatedAt), Direction = OrderByDirection.Desc }],
            AutonomousSystemOrganization = autonomousSystemOrganization,
        }, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(expectedCount));
    }

    #endregion

    #region CategoryId filter tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListBans_WhenFilteringByCategoryId_ReturnsOnlyCategoryBans(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithCategory("a", "Anime")
            .WithDefaultThread()
            .WithPost("test post", "192.168.1.1", isOriginalPost: true)
            .WithExactBan("192.168.1.1", "category A ban", inCategory: true)
            .WithCategory("b", "Random")
            .WithDefaultThread()
            .WithPost("test post", "192.168.1.2", isOriginalPost: true)
            .WithExactBan("192.168.1.2", "category B ban", inCategory: true)
            .WithExactBan("192.168.1.3", "system-wide ban")
            .SaveAsync(cancellationToken);

        var categoryA = builder.GetCategory("a");
        var categoryB = builder.GetCategory("b");
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var resultCategoryA = await repository.ListBansAsync(new BanPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Ban.CreatedAt), Direction = OrderByDirection.Desc }],
            CategoryId = categoryA.Id,
        }, cancellationToken);

        var resultCategoryB = await repository.ListBansAsync(new BanPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Ban.CreatedAt), Direction = OrderByDirection.Desc }],
            CategoryId = categoryB.Id,
        }, cancellationToken);

        var resultAll = await repository.ListBansAsync(new BanPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Ban.CreatedAt), Direction = OrderByDirection.Desc }],
        }, cancellationToken);

        // Assert
        Assert.That(resultCategoryA.Data, Has.Count.EqualTo(1));
        Assert.That(resultCategoryA.Data[0].Reason, Is.EqualTo("category A ban"));

        Assert.That(resultCategoryB.Data, Has.Count.EqualTo(1));
        Assert.That(resultCategoryB.Data[0].Reason, Is.EqualTo("category B ban"));

        Assert.That(resultAll.Data, Has.Count.EqualTo(3));
    }

    #endregion

    #region RelatedPostId filter tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListBans_WhenFilteringByRelatedPostId_ReturnsOnlyRelatedBan(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("bad post", "192.168.1.1", isOriginalPost: true)
            .SaveAsync(cancellationToken);

        var relatedPostId = builder.LastPostId;

        await builder
            .WithExactBan("192.168.1.1", "ban with related post", relatedPostId: relatedPostId)
            .WithExactBan("192.168.1.2", "ban without related post")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var resultWithRelatedPost = await repository.ListBansAsync(new BanPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Ban.CreatedAt), Direction = OrderByDirection.Desc }],
            RelatedPostId = relatedPostId,
        }, cancellationToken);

        var resultAll = await repository.ListBansAsync(new BanPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Ban.CreatedAt), Direction = OrderByDirection.Desc }],
        }, cancellationToken);

        // Assert
        Assert.That(resultWithRelatedPost.Data, Has.Count.EqualTo(1));
        Assert.That(resultWithRelatedPost.Data[0].Reason, Is.EqualTo("ban with related post"));
        Assert.That(resultWithRelatedPost.Data[0].RelatedPostId, Is.EqualTo(relatedPostId));

        Assert.That(resultAll.Data, Has.Count.EqualTo(2));
    }

    #endregion

    #region Combined filters tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListBans_WhenCombiningMultipleFilters_ReturnsCorrectResults(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);
        var utcNow = builder.TimeProvider.GetUtcNow().UtcDateTime;

        await builder
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "192.168.1.1", isOriginalPost: true)
            // Ban 1: matches all filters
            .WithExactBan(
                "192.168.1.1",
                "matching ban",
                countryIsoCode: "US",
                autonomousSystemNumber: 15169,
                createdAtOffset: TimeSpan.FromDays(-5))
            // Ban 2: wrong country
            .WithExactBan(
                "192.168.1.2",
                "wrong country ban",
                countryIsoCode: "RU",
                autonomousSystemNumber: 15169,
                createdAtOffset: TimeSpan.FromDays(-5))
            // Ban 3: wrong ASN
            .WithExactBan(
                "192.168.1.3",
                "wrong ASN ban",
                countryIsoCode: "US",
                autonomousSystemNumber: 32934,
                createdAtOffset: TimeSpan.FromDays(-5))
            // Ban 4: outside date range
            .WithExactBan(
                "192.168.1.4",
                "old ban",
                countryIsoCode: "US",
                autonomousSystemNumber: 15169,
                createdAtOffset: TimeSpan.FromDays(-20))
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var result = await repository.ListBansAsync(new BanPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [new OrderByItem { Field = nameof(Hikkaba.Data.Entities.Ban.CreatedAt), Direction = OrderByDirection.Desc }],
            CountryIsoCode = "US",
            AutonomousSystemNumber = 15169,
            CreatedNotBefore = utcNow.AddDays(-10),
            CreatedNotAfter = utcNow,
        }, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(1));
        Assert.That(result.Data[0].Reason, Is.EqualTo("matching ban"));
    }

    #endregion

    #region Ordering tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase(nameof(BanDetailsModel.CreatedAt), OrderByDirection.Asc)]
    [TestCase(nameof(BanDetailsModel.CreatedAt), OrderByDirection.Desc)]
    [TestCase(nameof(BanDetailsModel.EndsAt), OrderByDirection.Asc)]
    [TestCase(nameof(BanDetailsModel.EndsAt), OrderByDirection.Desc)]
    [TestCase(nameof(BanDetailsModel.Id), OrderByDirection.Asc)]
    [TestCase(nameof(BanDetailsModel.Id), OrderByDirection.Desc)]
    [TestCase(nameof(BanDetailsModel.CountryIsoCode), OrderByDirection.Asc)]
    [TestCase(nameof(BanDetailsModel.CountryIsoCode), OrderByDirection.Desc)]
    [TestCase(nameof(BanDetailsModel.AutonomousSystemNumber), OrderByDirection.Asc)]
    [TestCase(nameof(BanDetailsModel.AutonomousSystemNumber), OrderByDirection.Desc)]
    [TestCase(nameof(BanDetailsModel.AutonomousSystemOrganization), OrderByDirection.Asc)]
    [TestCase(nameof(BanDetailsModel.AutonomousSystemOrganization), OrderByDirection.Desc)]
    [TestCase(nameof(BanDetailsModel.Reason), OrderByDirection.Asc)]
    [TestCase(nameof(BanDetailsModel.Reason), OrderByDirection.Desc)]
    public async Task ListBans_WhenOrderByField_ReturnsOrderedResults(
        string fieldName,
        OrderByDirection direction,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "192.168.1.1", isOriginalPost: true)
            .WithExactBan("192.168.1.1", "reason 1", createdAtOffset: TimeSpan.FromDays(-10), endsAtOffset: TimeSpan.FromDays(5), countryIsoCode: "DE", autonomousSystemNumber: 100, autonomousSystemOrganization: "Alpha Corp")
            .WithExactBan("192.168.1.2", "reason 2", createdAtOffset: TimeSpan.FromDays(-5), endsAtOffset: TimeSpan.FromDays(10), countryIsoCode: "RU", autonomousSystemNumber: 200, autonomousSystemOrganization: "Beta Inc")
            .WithExactBan("192.168.1.3", "reason 3", createdAtOffset: TimeSpan.FromDays(-1), endsAtOffset: TimeSpan.FromDays(15), countryIsoCode: "US", autonomousSystemNumber: 300, autonomousSystemOrganization: "Gamma LLC")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var result = await repository.ListBansAsync(new BanPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = [new OrderByItem { Field = fieldName, Direction = direction }],
        }, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(3));
        Assert.That(result.Data, Is.OrderedBy(fieldName, direction));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListBans_WhenOrderByMultipleFields_ReturnsCorrectlyOrderedResults(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope);

        await builder
            .WithUser(Defaults.AdministratorUserName, isAdmin: true)
            .WithDefaultCategory()
            .WithDefaultThread()
            .WithPost("test post", "192.168.1.1", isOriginalPost: true)
            // Same country, different ASN
            .WithExactBan("192.168.1.1", "ban 1", countryIsoCode: "US", autonomousSystemNumber: 300)
            .WithExactBan("192.168.1.2", "ban 2", countryIsoCode: "US", autonomousSystemNumber: 100)
            .WithExactBan("192.168.1.3", "ban 3", countryIsoCode: "US", autonomousSystemNumber: 200)
            // Different country
            .WithExactBan("192.168.1.4", "ban 4", countryIsoCode: "DE", autonomousSystemNumber: 150)
            .WithExactBan("192.168.1.5", "ban 5", countryIsoCode: "RU", autonomousSystemNumber: 250)
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IBanRepository>();

        // Act
        var result = await repository.ListBansAsync(new BanPagingFilter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy =
            [
                new OrderByItem { Field = nameof(BanDetailsModel.CountryIsoCode), Direction = OrderByDirection.Asc },
                new OrderByItem { Field = nameof(BanDetailsModel.AutonomousSystemNumber), Direction = OrderByDirection.Desc },
            ],
        }, cancellationToken);

        // Assert
        Assert.That(result.Data, Has.Count.EqualTo(5));
        Assert.That(
            result.Data,
            Is.Ordered
                .Ascending.By(nameof(BanDetailsModel.CountryIsoCode))
                .Then.Descending.By(nameof(BanDetailsModel.AutonomousSystemNumber)));
    }

    #endregion
}
