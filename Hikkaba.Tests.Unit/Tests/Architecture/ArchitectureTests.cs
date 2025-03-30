using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BBCodeParser;
using Hikkaba.Application;
using Hikkaba.Data;
using Hikkaba.Infrastructure.Mappings;
using Hikkaba.Infrastructure.Models;
using Hikkaba.Infrastructure.Repositories;
using Hikkaba.Paging.Enums;
using Hikkaba.Shared;
using Hikkaba.Shared.Interfaces;
using Hikkaba.Web;
using NetArchTest.Rules;

namespace Hikkaba.Tests.Unit.Tests.Architecture;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
public class ArchitectureTests
{
    private static readonly HikkabaSharedAssemblyInfo HikkabaSharedAssemblyInfo = new();
    private static readonly HikkabaDataAssemblyInfo HikkabaDataAssemblyInfo = new();
    private static readonly HikkabaInfrastructureMappingsAssemblyInfo HikkabaInfrastructureMappingsAssemblyInfo = new();
    private static readonly HikkabaInfrastructureModelsAssemblyInfo HikkabaInfrastructureModelsAssemblyInfo = new();
    private static readonly HikkabaInfrastructureRepositoriesAssemblyInfo HikkabaInfrastructureRepositoriesAssemblyInfo = new();
    private static readonly HikkabaApplicationAssemblyInfo HikkabaApplicationAssemblyInfo = new();
    private static readonly HikkabaWebAssemblyInfo HikkabaWebAssemblyInfo = new();

    private static IReadOnlyList<(IAssemblyInfo AssemblyInfo, Assembly Assembly)> GetAllAssemblyData()
    {
        return new List<IAssemblyInfo>
            {
                HikkabaSharedAssemblyInfo,
                HikkabaDataAssemblyInfo,
                HikkabaInfrastructureMappingsAssemblyInfo,
                HikkabaInfrastructureModelsAssemblyInfo,
                HikkabaInfrastructureRepositoriesAssemblyInfo,
                HikkabaApplicationAssemblyInfo,
                HikkabaWebAssemblyInfo,
            }
            .Select(x => (x, x.GetType().Assembly))
            .ToList()
            .AsReadOnly();
    }

    private static IReadOnlyList<Assembly> GetAllAssemblies()
    {
        return GetAllAssemblyData()
            .Select(x => x.Assembly)
            .ToList()
            .AsReadOnly();
    }

    private static string[] GetAllAssemblyNames()
    {
        return GetAllAssemblyData()
            .Select(x => x.Assembly.GetName().Name)
            .Where(x => x != null)
            .Select(x => x!)
            .ToArray();
    }

    [Test]
    public void Interfaces_ShouldHaveNameStartingWith_I()
    {
        var result = Types
            .InAssemblies(GetAllAssemblies())
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I", StringComparison.Ordinal)
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True);
    }

    [Test]
    public void Assemblies_ShouldNotDepend_OnHigherLevelAssemblies()
    {
        var assemblyTuples = GetAllAssemblyData();

        foreach (var assemblyTuple in assemblyTuples)
        {
            var hierarchyLevel = assemblyTuple.AssemblyInfo.HierarchyLevel;

            var higherLevelAssemblies = assemblyTuples
                .Where(x => x.AssemblyInfo.HierarchyLevel > hierarchyLevel)
                .Select(x => x.Assembly.GetName().Name)
                .Where(x => x != null)
                .Select(x => x!)
                .ToArray();

            var result = Types
                .InAssembly(assemblyTuple.Assembly)
                .Should()
                .NotHaveDependencyOnAny(higherLevelAssemblies)
                .GetResult();

            Assert.That(result.IsSuccessful, Is.True, $"Assembly {assemblyTuple.Assembly.GetName().Name} should not depend on higher level assemblies: {string.Join(", ", higherLevelAssemblies)}");
        }

        Assert.That(assemblyTuples, Is.Not.Empty, "No assemblies found to check dependencies.");
    }

    [Test]
    public void InfrastructureModels_ShouldHaveNameEndingWith_Suffix()
    {
        var infrastructureModelsAssembly = typeof(HikkabaInfrastructureModelsAssemblyInfo).Assembly;

        var result = Types
            .InAssembly(infrastructureModelsAssembly)
            .That()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Model", StringComparison.Ordinal)
            .Or()
            .HaveNameEndingWith("Filter", StringComparison.Ordinal)
            .Or()
            .HaveNameEndingWith("Command", StringComparison.Ordinal)
            .Or()
            .HaveNameEndingWith("Query", StringComparison.Ordinal)
            .Or()
            .HaveNameEndingWith("Info", StringComparison.Ordinal)
            .Or()
            .HaveNameEndingWith("Container", StringComparison.Ordinal)
            .Or()
            .HaveNameEndingWith("Collection", StringComparison.Ordinal)
            .Or()
            .HaveNameEndingWith("Configuration", StringComparison.Ordinal)
            .Or()
            .HaveNameEndingWith("Extensions", StringComparison.Ordinal)
            .GetResult();

        var failingTypeNames = result.FailingTypeNames ?? [];

        Assert.That(result.IsSuccessful, Is.True, $"Some classes in {infrastructureModelsAssembly.GetName().Name} do not follow the naming convention: {string.Join(", ", failingTypeNames)}");
    }

    [Test]
    public void Libraries_ShouldNotDepend_OnAnyOtherAssembly()
    {
        var result = Types
            .InAssemblies([typeof(BbParser).Assembly, typeof(OrderByDirection).Assembly])
            .Should()
            .NotHaveDependencyOnAny(GetAllAssemblyNames())
            .GetResult();

        var failingTypeNames = result.FailingTypeNames ?? [];

        Assert.That(result.IsSuccessful, Is.True, $"Some libraries depend on other assemblies: {string.Join(", ", failingTypeNames)}");
    }
}
