using Hikkaba.Application.Implementations;
using Hikkaba.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Hikkaba.Tests.Unit.Tests.Services;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
internal sealed class SystemInfoServiceTests
{
    [Test]
    public void SystemInfoService_ShouldReturnCorrectDatabaseProvider()
    {
        var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("HikkabaDbContextTest")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        using var context = new ApplicationDbContext(contextOptions);
        const string databaseProviderName = "Microsoft.EntityFrameworkCore.InMemory";
        var systemInfoService = new SystemInfoService(context);
        var systemInfo = systemInfoService.GetSystemInfo();
        Assert.That(systemInfo.DatabaseProvider, Is.EqualTo(databaseProviderName));
    }
}
