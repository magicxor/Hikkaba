using System;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Web;

namespace Hikkaba.Web
{
    internal class Program
    {
        private const string EnvPrefix = "Hikkaba_";

        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // seed: https://dotnetthoughts.net/seed-database-in-aspnet-core/s
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
                var seedConfig = services.GetRequiredService<IOptions<SeedConfiguration>>();
                try
                {
                    await context.Database.MigrateAsync();
                    await DbSeeder.SeedAsync(context, userManager, roleManager, seedConfig);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred seeding the DB");
                    throw;
                }
            }

            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.AddEnvironmentVariables(EnvPrefix);
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseNLog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
