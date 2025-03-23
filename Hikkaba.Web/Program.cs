using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Hikkaba.Web;

public class Program
{
    private const string NlogFileName = "nlog.config";
    private const string EnvPrefix = "Hikkaba_";

    public static async Task Main(string[] args)
    {
        // NLog: set up the logger first to catch all errors
        LogManager.Setup(s => s.LoadConfigurationFromXml(NlogFileName));

        try
        {
            await CreateHostBuilder(args).Build().RunAsync();
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

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddEnvironmentVariables(EnvPrefix);
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Trace);
            })
            .UseNLog()
            .UseDefaultServiceProvider((context, options) => {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
