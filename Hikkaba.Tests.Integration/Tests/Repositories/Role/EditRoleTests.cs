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
using OneOf.Types;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Role;

internal sealed class EditRoleTests : IntegrationTestBase
{
    #region Basic edit tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditRole_WhenValidRequest_UpdatesRoleSuccessfully(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithRole("OldName");
        await builder.SaveAsync(cancellationToken);

        var role = builder.GetRole("OldName");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        var request = new RoleEditRequestModel
        {
            RoleId = role.Id,
            RoleName = "NewName",
        };

        // Act
        var result = await repository.EditRoleAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<Success>(), "Expected successful role update");

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedRole = await dbContext.Roles.FirstAsync(r => r.Id == role.Id, cancellationToken);
        Assert.That(updatedRole.Name, Is.EqualTo("NewName"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditRole_WhenUpdated_UpdatesNormalizedName(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithRole("OldName");
        await builder.SaveAsync(cancellationToken);

        var role = builder.GetRole("OldName");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        var request = new RoleEditRequestModel
        {
            RoleId = role.Id,
            RoleName = "NewRoleName",
        };

        // Act
        var result = await repository.EditRoleAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<Success>(), "Expected successful role update");

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedRole = await dbContext.Roles.FirstAsync(r => r.Id == role.Id, cancellationToken);
        Assert.That(updatedRole.NormalizedName, Is.EqualTo("NEWROLENAME"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditRole_WhenSameNameProvided_SucceedsWithoutChange(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithRole("SameName");
        await builder.SaveAsync(cancellationToken);

        var role = builder.GetRole("SameName");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        var request = new RoleEditRequestModel
        {
            RoleId = role.Id,
            RoleName = "SameName",
        };

        // Act
        var result = await repository.EditRoleAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<Success>(), "Expected successful role update");

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedRole = await dbContext.Roles.FirstAsync(r => r.Id == role.Id, cancellationToken);
        Assert.That(updatedRole.Name, Is.EqualTo("SameName"));
    }

    #endregion

    #region Not found tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditRole_WhenRoleNotFound_ReturnsNotFoundError(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        var request = new RoleEditRequestModel
        {
            RoleId = 999999, // Non-existent role ID
            RoleName = "NewName",
        };

        // Act
        var result = await repository.EditRoleAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<DomainError>(), "Expected error result for non-existent role");
        var error = result.AsT1;
        Assert.That(error.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
        Assert.That(error.ErrorMessage, Is.EqualTo("Role not found."));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditRole_WhenNegativeRoleId_ReturnsNotFoundError(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        var request = new RoleEditRequestModel
        {
            RoleId = -1,
            RoleName = "NewName",
        };

        // Act
        var result = await repository.EditRoleAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<DomainError>());
        var error = result.AsT1;
        Assert.That(error.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
    }

    #endregion

    #region Duplicate name tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditRole_WhenRenamingToExistingName_ReturnsError(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithRole("Role1")
            .WithRole("Role2");
        await builder.SaveAsync(cancellationToken);

        var role1 = builder.GetRole("Role1");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        var request = new RoleEditRequestModel
        {
            RoleId = role1.Id,
            RoleName = "Role2", // Try to rename to existing role name
        };

        // Act
        var result = await repository.EditRoleAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<DomainError>(), "Expected error result for duplicate role name");
        var error = result.AsT1;
        Assert.That(error.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
        Assert.That(error.ErrorMessage, Does.Contain("Role update failed"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditRole_WhenRenamingToExistingNameDifferentCase_ReturnsError(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithRole("Role1")
            .WithRole("ExistingRole");
        await builder.SaveAsync(cancellationToken);

        var role1 = builder.GetRole("Role1");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        var request = new RoleEditRequestModel
        {
            RoleId = role1.Id,
            RoleName = "EXISTINGROLE", // Same as existing but different case
        };

        // Act
        var result = await repository.EditRoleAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<DomainError>(), "Expected error result for duplicate role name (case-insensitive)");
    }

    #endregion

    #region Case change tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditRole_WhenChangingCase_UpdatesSuccessfully(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithRole("lowercase");
        await builder.SaveAsync(cancellationToken);

        var role = builder.GetRole("lowercase");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();

        var request = new RoleEditRequestModel
        {
            RoleId = role.Id,
            RoleName = "LOWERCASE",
        };

        // Act
        var result = await repository.EditRoleAsync(request, cancellationToken);

        // Assert
        Assert.That(result.Value, Is.TypeOf<Success>(), "Expected successful role update");

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedRole = await dbContext.Roles.FirstAsync(r => r.Id == role.Id, cancellationToken);
        Assert.That(updatedRole.Name, Is.EqualTo("LOWERCASE"));
        Assert.That(updatedRole.NormalizedName, Is.EqualTo("LOWERCASE"));
    }

    #endregion

    #region Multiple edits tests

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditRole_WhenEditedMultipleTimes_AppliesAllChanges(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithRole("InitialName");
        await builder.SaveAsync(cancellationToken);

        var role = builder.GetRole("InitialName");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act & Assert - First edit
        var result1 = await repository.EditRoleAsync(new RoleEditRequestModel
        {
            RoleId = role.Id,
            RoleName = "SecondName",
        }, cancellationToken);

        Assert.That(result1.IsT0, Is.True);
        dbContext.ChangeTracker.Clear();
        var afterFirst = await dbContext.Roles.FirstAsync(r => r.Id == role.Id, cancellationToken);
        Assert.That(afterFirst.Name, Is.EqualTo("SecondName"));

        // Act & Assert - Second edit
        var result2 = await repository.EditRoleAsync(new RoleEditRequestModel
        {
            RoleId = role.Id,
            RoleName = "ThirdName",
        }, cancellationToken);

        Assert.That(result2.IsT0, Is.True);
        dbContext.ChangeTracker.Clear();
        var afterSecond = await dbContext.Roles.FirstAsync(r => r.Id == role.Id, cancellationToken);
        Assert.That(afterSecond.Name, Is.EqualTo("ThirdName"));

        // Act & Assert - Third edit
        var result3 = await repository.EditRoleAsync(new RoleEditRequestModel
        {
            RoleId = role.Id,
            RoleName = "FinalName",
        }, cancellationToken);

        Assert.That(result3.IsT0, Is.True);
        dbContext.ChangeTracker.Clear();
        var afterThird = await dbContext.Roles.FirstAsync(r => r.Id == role.Id, cancellationToken);
        Assert.That(afterThird.Name, Is.EqualTo("FinalName"));
        Assert.That(afterThird.NormalizedName, Is.EqualTo("FINALNAME"));
    }

    #endregion
}
