using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Tests.Integration.Constants;
using Hikkaba.Tests.Integration.Extensions;
using Hikkaba.Tests.Integration.Services;
using Hikkaba.Tests.Integration.Utils;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Data;

[TestFixture]
[Parallelizable(scope: ParallelScope.Fixtures)]
public sealed class EfMigrationsTests
{
    private RespawnableContextManager<ApplicationDbContext>? _contextManager;

    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        _contextManager = await TestDbUtils.CreateNewRandomDbContextManagerAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await _contextManager.StopIfNotNullAsync();
    }

    [MustDisposeResource]
    private async Task<CustomAppFactory> CreateAppFactoryAsync()
    {
        var connectionString = await _contextManager!.CreateRespawnedDbConnectionStringAsync();
        return new CustomAppFactory(connectionString);
    }

    /* see
     https://www.meziantou.net/detect-missing-migrations-in-entity-framework-core.htm
     https://github.com/dotnet/efcore/issues/26348#issuecomment-1535156915
     */
    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EnsureMigrationsAreUpToDate(CancellationToken cancellationToken)
    {
        await using var customAppFactory = await CreateAppFactoryAsync();
        using var scope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Get required services from the dbcontext
        var migrationModelDiffer = dbContext.GetService<IMigrationsModelDiffer>();
        var migrationsAssembly = dbContext.GetService<IMigrationsAssembly>();
        var modelRuntimeInitializer = dbContext.GetService<IModelRuntimeInitializer>();
        var designTimeModel = dbContext.GetService<IDesignTimeModel>();

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
        Assert.That(modelDifferences, Is.Empty);
    }
}
