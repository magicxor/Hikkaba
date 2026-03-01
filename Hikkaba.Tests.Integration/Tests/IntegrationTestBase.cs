using System.Diagnostics.CodeAnalysis;
using Hikkaba.Data.Context;
using Hikkaba.Tests.Integration.Exceptions;
using Hikkaba.Tests.Integration.Models;
using Hikkaba.Tests.Integration.Services;
using Hikkaba.Tests.Integration.Utils;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests;

[TestFixture]
[Parallelizable(scope: ParallelScope.Fixtures)]
internal abstract class IntegrationTestBase
{
    [SuppressMessage("Structure", "NUnit1032:An IDisposable field/property should be Disposed in a TearDown method", Justification = "Object from pool")]
    private RespawnableContextManager<ApplicationDbContext>? _dbManager;

    [OneTimeSetUp]
    public void BaseOneTimeSetUp()
    {
        _dbManager = TestDbUtils.LeaseMetaManagerFromPool();

        TestLogUtils.WriteProgressMessage("Base.OneTimeSetUp");
    }

    [OneTimeTearDown]
    public void BaseOneTimeTearDown()
    {
        TestDbUtils.ReturnMetaManagerToPool(_dbManager ?? throw new IntegrationTestException("MetaManager is null in OneTimeTearDown"));

        TestLogUtils.WriteProgressMessage("Base.OneTimeTearDown");
    }

    private async Task MigrateAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
    }

    [MustDisposeResource]
    protected async Task<AppScope> CreateAppScopeAsync(CancellationToken cancellationToken)
    {
        var dbConnectionString = await _dbManager!.CreateRespawnedDbConnectionStringAsync();
        var customAppFactory = new CustomApiFactory(dbConnectionString);

        var scope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await MigrateAsync(applicationDbContext, cancellationToken);

        return new AppScope
        {
            ApplicationDbContext = applicationDbContext,
            ServiceScope = scope,
            AppFactory = customAppFactory,
        };
    }
}
