using Hikkaba.Data.Context;
using Hikkaba.Web.Services;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hikkaba.Tests.Integration.Tests.DataModel;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
public sealed class EfMetaPendingModelChangesTests
{
    /* see
     https://www.meziantou.net/detect-missing-migrations-in-entity-framework-core.htm
     https://github.com/dotnet/efcore/issues/26348#issuecomment-1535156915
     */
    [Test]
    public void Ensure_Migrations_AreUpToDate()
    {
        using var applicationDbContext = CreateDbContext();

        // Get required services from the dbcontext
        var migrationModelDiffer = applicationDbContext.GetService<IMigrationsModelDiffer>();
        var migrationsAssembly = applicationDbContext.GetService<IMigrationsAssembly>();
        var modelRuntimeInitializer = applicationDbContext.GetService<IModelRuntimeInitializer>();
        var designTimeModel = applicationDbContext.GetService<IDesignTimeModel>();

        // Get current model
        var model = designTimeModel.Model;

        // Get the snapshot model and finalize it
        var snapshotModel = migrationsAssembly.ModelSnapshot?.Model;
        if (snapshotModel is IMutableModel mutableModel)
        {
            // Forces post-processing on the model such that it is ready for use by the runtime
            snapshotModel = mutableModel.FinalizeModel();
        }

        if (snapshotModel is not null)
        {
            // Validates and initializes the given model with runtime dependencies
            snapshotModel = modelRuntimeInitializer.Initialize(snapshotModel);
        }

        // Compute differences
        var modelDifferences = migrationModelDiffer.GetDifferences(
            source: snapshotModel?.GetRelationalModel(),
            target: model.GetRelationalModel());

        // The differences should be empty if the migrations are up-to-date
        Assert.That(modelDifferences, Is.Empty, string.Join("\n", modelDifferences.Select(d => d.ToString())));
    }

    [Test]
    public void Ensure_NoPendingModelChanges()
    {
        using var applicationDbContext = CreateDbContext();
        Assert.That(applicationDbContext.Database.HasPendingModelChanges(), Is.False, $"There are pending model changes in {nameof(ApplicationDbContext)}. Migrations are not up-to-date.");
    }

    [MustDisposeResource]
    private static ApplicationDbContext CreateDbContext()
    {
        var metaDesignTimeDbContextFactory = new MetaDesignTimeDbContextFactory();
        return metaDesignTimeDbContextFactory.CreateDbContext([]);
    }
}
