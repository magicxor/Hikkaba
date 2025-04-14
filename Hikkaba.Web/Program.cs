using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Shared.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Web;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Hikkaba.Web;

[SuppressMessage("Roslynator", "RCS1102:Make class static", Justification = "Entry point requires a non-static class for testability.")]
[SuppressMessage("Major Code Smell", "S1118:Utility classes should not have public constructors", Justification = "Entry point requires a non-static class for testability.")]
[SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "Entry point requires this call")]
internal class Program
{
    private const string NlogFileName = "nlog.config";
    private const string EnvPrefix = "Hikkaba_";

    public static async Task Main(string[] args)
    {
        // NLog: set up the logger first to catch all errors
        var loggingConfiguration = new XmlLoggingConfiguration(NlogFileName);
        LogManager.Configuration = loggingConfiguration;

        try
        {
            var host = CreateHostBuilder(args, loggingConfiguration).Build();
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            // NLog: catch setup errors
            LogManager.GetCurrentClassLogger().Error(ex, "Stopped program because of exception");
            throw;
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            LogManager.Shutdown();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args, LoggingConfiguration loggingConfiguration) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, config) => config.AddEnvironmentVariables(EnvPrefix))
            .ConfigureLogging((hostBuilderContext, logging) =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddNLog(loggingConfiguration);

                var hikkabaConfig = hostBuilderContext.Configuration
                    .GetSection(nameof(HikkabaConfiguration))
                    .Get<HikkabaConfiguration>();
                var otlpExporterUri = hikkabaConfig?.OtlpExporterUri;

                if (otlpExporterUri is not null)
                {
                    logging.AddOpenTelemetry(telemetryLoggerOptions =>
                    {
                        telemetryLoggerOptions.SetResourceBuilder(ResourceBuilder.CreateDefault()
                            .AddService(Defaults.ServiceName))
                            .AddOtlpExporter(otlpExporterOptions => otlpExporterOptions.Endpoint = otlpExporterUri);
                    });
                }
            })
            .UseDefaultServiceProvider((_, options) =>
            {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            })
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
}
