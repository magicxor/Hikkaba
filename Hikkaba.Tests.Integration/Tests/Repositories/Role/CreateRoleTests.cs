using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Error;
using Hikkaba.Infrastructure.Models.Role;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Role;

internal sealed class CreateRoleTests : IntegrationTestBase
{
    #region Basic create tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateRole_WhenValidRequest_CreatesRoleSuccessfully(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        var request = new RoleCreateRequestModel
        {
            RoleName = "NewRole",
        };

        // Act
        var result = await repository.CreateRoleAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<RoleCreateResultSuccessModel>(), "Expected successful role creation");
        var successResult = result.AsT0;
        Assert.That(successResult.RoleId, Is.GreaterThan(0));

        // Verify role was created in DB
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var createdRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == successResult.RoleId, cancellationToken);
        Assert.That(createdRole, Is.Not.Null);
        Assert.That(createdRole!.Name, Is.EqualTo("NewRole"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateRole_WhenCreated_SetsNormalizedName(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        var request = new RoleCreateRequestModel
        {
            RoleName = "TestRole",
        };

        // Act
        var result = await repository.CreateRoleAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<RoleCreateResultSuccessModel>(), "Expected successful role creation");
        var successResult = result.AsT0;

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var createdRole = await dbContext.Roles.FirstAsync(r => r.Id == successResult.RoleId, cancellationToken);
        Assert.That(createdRole.NormalizedName, Is.EqualTo("TESTROLE"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateRole_WhenMixedCaseName_SetsCorrectNormalizedName(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        var request = new RoleCreateRequestModel
        {
            RoleName = "SuperAdminRole",
        };

        // Act
        var result = await repository.CreateRoleAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<RoleCreateResultSuccessModel>(), "Expected successful role creation");
        var successResult = result.AsT0;

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var createdRole = await dbContext.Roles.FirstAsync(r => r.Id == successResult.RoleId, cancellationToken);
        Assert.That(createdRole.Name, Is.EqualTo("SuperAdminRole"));
        Assert.That(createdRole.NormalizedName, Is.EqualTo("SUPERADMINROLE"));
    }

    #endregion

    #region Duplicate role tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateRole_WhenDuplicateName_ReturnsError(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithRole("ExistingRole")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        var request = new RoleCreateRequestModel
        {
            RoleName = "ExistingRole",
        };

        // Act
        var result = await repository.CreateRoleAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<DomainError>(), "Expected error result for duplicate role");
        var error = result.AsT1;
        Assert.That(error.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
        Assert.That(error.ErrorMessage, Does.Contain("Role creation failed"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateRole_WhenDuplicateNameDifferentCase_ReturnsError(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        await new TestDataBuilder(appScope.ServiceScope)
            .WithRole("TestRole")
            .SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        var request = new RoleCreateRequestModel
        {
            RoleName = "TESTROLE", // Same role name but different case
        };

        // Act
        var result = await repository.CreateRoleAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<DomainError>(), "Expected error result for duplicate role (case-insensitive)");
        var error = result.AsT1;
        Assert.That(error.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
    }

    #endregion

    #region Multiple roles tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateRole_WhenMultipleRolesCreated_AllHaveUniqueIds(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Act
        var result1 = await repository.CreateRoleAsync(new RoleCreateRequestModel { RoleName = "Role1" }, cancellationToken);
        var result2 = await repository.CreateRoleAsync(new RoleCreateRequestModel { RoleName = "Role2" }, cancellationToken);
        var result3 = await repository.CreateRoleAsync(new RoleCreateRequestModel { RoleName = "Role3" }, cancellationToken);

        // Assert
        Assert.That(result1.IsT0, Is.True);
        Assert.That(result2.IsT0, Is.True);
        Assert.That(result3.IsT0, Is.True);

        var roleId1 = result1.AsT0.RoleId;
        var roleId2 = result2.AsT0.RoleId;
        var roleId3 = result3.AsT0.RoleId;

        Assert.That(roleId1, Is.Not.EqualTo(roleId2));
        Assert.That(roleId2, Is.Not.EqualTo(roleId3));
        Assert.That(roleId1, Is.Not.EqualTo(roleId3));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateRole_WhenCreatedAfterExistingRoles_HasHigherId(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithRole("ExistingRole1")
            .WithRole("ExistingRole2");
        await builder.SaveAsync(cancellationToken);

        var existingRole1 = builder.GetRole("ExistingRole1");
        var existingRole2 = builder.GetRole("ExistingRole2");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        // Act
        var result = await repository.CreateRoleAsync(new RoleCreateRequestModel { RoleName = "NewRole" }, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<RoleCreateResultSuccessModel>(), "Expected successful role creation");
        var newRoleId = result.AsT0.RoleId;
        Assert.That(newRoleId, Is.GreaterThan(existingRole1.Id));
        Assert.That(newRoleId, Is.GreaterThan(existingRole2.Id));
    }

    #endregion

    #region Special characters tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase("Role With Spaces")]
    [TestCase("Role-With-Dashes")]
    [TestCase("Role_With_Underscores")]
    [TestCase("Role123")]
    public async Task CreateRole_WhenNameWithSpecialCharacters_CreatesSuccessfully(
        string roleName,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        var request = new RoleCreateRequestModel
        {
            RoleName = roleName,
        };

        // Act
        var result = await repository.CreateRoleAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<RoleCreateResultSuccessModel>(), "Expected successful role creation");

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var createdRole = await dbContext.Roles.FirstAsync(r => r.Id == result.AsT0.RoleId, cancellationToken);
        Assert.That(createdRole.Name, Is.EqualTo(roleName));
    }

    #endregion
}
