using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Hikkaba.Shared.Utils;
using NLog;
using NLog.Config;
using NLog.Filters;
using NLog.Targets;

namespace Hikkaba.Tests.Integration;

/// <summary>
/// Module initializer that runs before any test code executes.
/// This ensures NLog is configured with a minimal test configuration
/// </summary>
internal static class ModuleInitializer
{
    private static readonly Lock Lock = new();
    private static LoggingConfiguration? _testConfiguration;

    [ModuleInitializer]
    internal static void Initialize()
    {
        // Set environment name before anything else
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentHelper.IntegrationTestsEnvironmentName);

        // Apply the test configuration immediately
        ApplyTestConfiguration();
    }

    /// <summary>
    /// Creates and returns the test NLog configuration.
    /// </summary>
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The NLog configuration and targets are meant to live for the duration of the tests and are properly disposed by NLog when the process exits.")]
    private static LoggingConfiguration GetTestConfiguration()
    {
        lock (Lock)
        {
            if (_testConfiguration != null)
            {
                return _testConfiguration;
            }

            var loggingConfiguration = new LoggingConfiguration();

            // Add a null target for the "integrationTest" marker that Program.cs checks for
            var nullTarget = new NullTarget("integrationTest");
            loggingConfiguration.AddTarget(nullTarget);

            // Add a console target for actual logging during tests
            var consoleTarget = new ConsoleTarget("testConsole")
            {
                Layout = "${longdate} ${level:uppercase=true} ${logger} ${message} ${exception:format=tostring}",
            };
            loggingConfiguration.AddTarget(consoleTarget);

            // Only log warnings and above to console during tests
            var consoleLoggingRule = new LoggingRule("*", LogLevel.Warn, LogLevel.Fatal, consoleTarget)
            {
                FilterDefaultAction = FilterResult.Log,
            };
            consoleLoggingRule.Filters.Add(new ConditionBasedFilter
            {
                Condition = "equals(logger,'Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware') and contains(message,'Static files may be unavailable.') and level = LogLevel.Warn",
                Action = FilterResult.Ignore,
            });

            loggingConfiguration.AddRule(consoleLoggingRule);

            _testConfiguration = loggingConfiguration;
            return _testConfiguration;
        }
    }

    /// <summary>
    /// Applies the test NLog configuration. Call this after any operation that might
    /// have overwritten the NLog configuration (e.g., UseNLog()).
    /// </summary>
    internal static void ApplyTestConfiguration()
    {
        var config = GetTestConfiguration();

        // Avoid re-setting the same config object — the Configuration setter
        // calls Flush() + Close() + re-Initialize on targets while holding _syncRoot,
        // which causes heavy contention when 12 parallel threads all do this simultaneously.
        if (ReferenceEquals(LogManager.Configuration, config))
        {
            return;
        }

        LogManager.Configuration = config;
        LogManager.ReconfigExistingLoggers();
    }
}
