using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Models.Role;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Constants;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Hikkaba.Tests.Integration.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Role;

internal sealed class ListRolesTests : IntegrationTestBase
{
    #region Basic listing tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListRoles_WhenNoRoles_ReturnsEmptyList(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Act
        var result = await repository.ListRolesAsync(new RoleFilter
        {
            OrderBy = [new OrderByItem { Field = nameof(RoleModel.NormalizedName), Direction = OrderByDirection.Asc }],
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListRoles_WhenSingleRole_ReturnsOneRole(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithRole("TestRole")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Act
        var result = await repository.ListRolesAsync(new RoleFilter
        {
            OrderBy = [new OrderByItem { Field = nameof(RoleModel.NormalizedName), Direction = OrderByDirection.Asc }],
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("TestRole"));
        Assert.That(result[0].NormalizedName, Is.EqualTo("TESTROLE"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListRoles_WhenMultipleRoles_ReturnsAllRoles(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithRole("Alpha")
            .WithRole("Beta")
            .WithRole("Gamma")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Act
        var result = await repository.ListRolesAsync(new RoleFilter
        {
            OrderBy = [new OrderByItem { Field = nameof(RoleModel.NormalizedName), Direction = OrderByDirection.Asc }],
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        var names = result.Select(r => r.Name).ToList();
        Assert.That(names, Does.Contain("Alpha"));
        Assert.That(names, Does.Contain("Beta"));
        Assert.That(names, Does.Contain("Gamma"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListRoles_WhenDefaultRoles_ReturnsAdminAndModerator(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithAdministratorRole()
            .WithModeratorRole()
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Act
        var result = await repository.ListRolesAsync(new RoleFilter
        {
            OrderBy = [new OrderByItem { Field = nameof(RoleModel.NormalizedName), Direction = OrderByDirection.Asc }],
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        var names = result.Select(r => r.Name).ToList();
        Assert.That(names, Does.Contain(Defaults.AdministratorRoleName));
        Assert.That(names, Does.Contain(Defaults.ModeratorRoleName));
    }

    #endregion

    #region Ordering tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase(nameof(RoleModel.NormalizedName), OrderByDirection.Asc)]
    [TestCase(nameof(RoleModel.NormalizedName), OrderByDirection.Desc)]
    [TestCase(nameof(RoleModel.Name), OrderByDirection.Asc)]
    [TestCase(nameof(RoleModel.Name), OrderByDirection.Desc)]
    [TestCase(nameof(RoleModel.Id), OrderByDirection.Asc)]
    [TestCase(nameof(RoleModel.Id), OrderByDirection.Desc)]
    public async Task ListRoles_WhenOrderByField_ReturnsOrderedResults(
        string fieldName,
        OrderByDirection direction,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithRole("Charlie")
            .WithRole("Alpha")
            .WithRole("Bravo")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Act
        var result = await repository.ListRolesAsync(new RoleFilter
        {
            OrderBy = [new OrderByItem { Field = fieldName, Direction = direction }],
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result, Is.OrderedBy(fieldName, direction));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListRoles_WhenOrderByNormalizedNameAsc_ReturnsAlphabeticallySortedRoles(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithRole("Zeta")
            .WithRole("Alpha")
            .WithRole("Mike")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Act
        var result = await repository.ListRolesAsync(new RoleFilter
        {
            OrderBy = [new OrderByItem { Field = nameof(RoleModel.NormalizedName), Direction = OrderByDirection.Asc }],
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result[0].Name, Is.EqualTo("Alpha"));
        Assert.That(result[1].Name, Is.EqualTo("Mike"));
        Assert.That(result[2].Name, Is.EqualTo("Zeta"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListRoles_WhenOrderByNormalizedNameDesc_ReturnsReverseSortedRoles(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithRole("Zeta")
            .WithRole("Alpha")
            .WithRole("Mike")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Act
        var result = await repository.ListRolesAsync(new RoleFilter
        {
            OrderBy = [new OrderByItem { Field = nameof(RoleModel.NormalizedName), Direction = OrderByDirection.Desc }],
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result[0].Name, Is.EqualTo("Zeta"));
        Assert.That(result[1].Name, Is.EqualTo("Mike"));
        Assert.That(result[2].Name, Is.EqualTo("Alpha"));
    }

    #endregion

    #region Role properties tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task ListRoles_ReturnsCorrectRoleProperties(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithRole("CustomRole");
        await builder.SaveAsync(cancellationToken);

        var expectedRole = builder.GetRole("CustomRole");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Act
        var result = await repository.ListRolesAsync(new RoleFilter
        {
            OrderBy = [new OrderByItem { Field = nameof(RoleModel.NormalizedName), Direction = OrderByDirection.Asc }],
        }, cancellationToken);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(expectedRole.Id));
        Assert.That(result[0].Name, Is.EqualTo("CustomRole"));
        Assert.That(result[0].NormalizedName, Is.EqualTo("CUSTOMROLE"));
    }

    #endregion
}
