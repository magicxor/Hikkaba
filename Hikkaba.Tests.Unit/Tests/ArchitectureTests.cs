using System;
using System.Reflection;
using Hikkaba.Repositories.Contracts.Ref;
using Hikkaba.Services.Contracts.Ref;
using NetArchTest.Rules;

namespace Hikkaba.Tests.Unit.Tests;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
public class ArchitectureTests
{
    private static readonly Assembly HikkabaRepositoriesContractsAssembly = typeof(HikkabaRepositoriesContractsRef).Assembly;
    private static readonly Assembly HikkabaServicesContractsAssembly = typeof(HikkabaServicesContractsRef).Assembly;

    [Test]
    public void Interfaces_ShouldHaveNameStartingWith_I()
    {
        var result = Types
            .InAssemblies([HikkabaRepositoriesContractsAssembly, HikkabaServicesContractsAssembly])
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I", StringComparison.Ordinal)
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True);
    }
}
