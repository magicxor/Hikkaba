using Hikkaba.Data.Context;
using Hikkaba.Tests.Integration.Constants;
using Hikkaba.Tests.Integration.Utils;
using Hikkaba.Web.Services;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hikkaba.Tests.Integration.Tests.DataModel;

[TestFixture]
[Parallelizable(scope: ParallelScope.Fixtures)]
public sealed class EfMetaMigrationsTests
{
    [CancelAfter(TestInfraDefaults.TestTimeout)]
    [Test]
    public async Task Ensure_AllMigrations_CanBeApplied(CancellationToken cancellationToken)
    {
        // Arrange
        await using var dbMgr = TestDbUtils.CreateNewRandomMetaManager();
        var connectionString = await dbMgr.CreateRespawnedDbConnectionStringAsync();
        await using var dbContext = CreateDbContext(connectionString);

        // Ensure the database is created without any migrations
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);

        var migrator = dbContext.GetService<IMigrator>();
        var migrationsAssembly = dbContext.GetService<IMigrationsAssembly>();
        var migrations = migrationsAssembly.Migrations.Keys.ToList();

        // Act & Assert: Apply migrations one by one
        foreach (var migration in migrations)
        {
            Assert.DoesNotThrowAsync(
                async () => await migrator.MigrateAsync(migration, cancellationToken),
                $"Failed to apply migration: {migration}");
        }
    }

    [CancelAfter(TestInfraDefaults.TestTimeout)]
    [Test]
    public async Task Ensure_AllMigrations_CanBeRolledBack(CancellationToken cancellationToken)
    {
        // Arrange
        await using var dbMgr = TestDbUtils.CreateNewRandomMetaManager();
        var connectionString = await dbMgr.CreateRespawnedDbConnectionStringAsync();
        await using var dbContext = CreateDbContext(connectionString);

        // First, apply all migrations
        await dbContext.Database.MigrateAsync(cancellationToken);

        var migrator = dbContext.GetService<IMigrator>();
        var migrationsAssembly = dbContext.GetService<IMigrationsAssembly>();
        var migrations = migrationsAssembly.Migrations.Keys.Reverse().ToList();

        // Act & Assert: Rollback migrations one by one (in reverse order)
        for (var i = 0; i < migrations.Count; i++)
        {
            // To rollback to a specific migration, we migrate to the previous one
            // For the first migration, we migrate to "0" which means before any migration
            var targetMigration = i == migrations.Count - 1 ? "0" : migrations[i + 1];

            Assert.DoesNotThrowAsync(
                async () => await migrator.MigrateAsync(targetMigration, cancellationToken),
                $"Failed to rollback migration: {migrations[i]}");
        }
    }

    [MustDisposeResource]
    private static ApplicationDbContext CreateDbContext(string connectionString)
    {
        var metaDesignTimeDbContextFactory = new MetaDesignTimeDbContextFactory(connectionString);
        return metaDesignTimeDbContextFactory.CreateDbContext([]);
    }
}
