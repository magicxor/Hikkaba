using System;
using System.Collections.Generic;
using System.Linq;

namespace Hikkaba.Shared.Utils;

public static class EnvironmentHelper
{
    public const string ProductionEnvironmentName = "Production";
    public const string StagingEnvironmentName = "Staging";
    public const string DevelopmentEnvironmentName = "Development";
    public const string IntegrationTestsEnvironmentName = "IntegrationTests";
    public const string FallbackEnvironmentName = DevelopmentEnvironmentName;

    public static readonly IReadOnlyList<string> NonDevelopmentEnvironments = [StagingEnvironmentName, ProductionEnvironmentName];

    public static string GetEnvironmentName()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? FallbackEnvironmentName;
    }

    public static bool IsDevelopmentEnv()
    {
        var currentEnvironment = GetEnvironmentName();
        return IsDevelopmentEnv(currentEnvironment);
    }

    public static bool IsDevelopmentEnv(string currentEnvironment)
    {
        return !NonDevelopmentEnvironments.Any(env =>
            env.Equals(currentEnvironment, StringComparison.OrdinalIgnoreCase));
    }
}
