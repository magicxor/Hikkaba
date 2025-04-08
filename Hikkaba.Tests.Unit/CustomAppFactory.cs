using System;
using System.Configuration;
using System.Data.Common;
using Hikkaba.Shared.Constants;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Tests.Unit.Mocks;
using Hikkaba.Web.Services.Contracts;
using Hikkaba.Web.Services.Implementations;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Config;
using NLog.Web;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Hikkaba.Tests.Unit;

internal sealed class CustomAppFactory
    : WebApplicationFactory<Web.Program>
{
    private readonly string _currentAction;

    public CustomAppFactory(string currentAction)
    {
        _currentAction = currentAction;
        LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .ConfigureServices(services =>
            {
                // we will use in-memory cache for tests
                services.RemoveAll<IDistributedCache>();
                services.RemoveAll<IDataProtectionProvider>();

                // we will override these options
                services.RemoveAll<HikkabaConfiguration>();
                services.RemoveAll<SeedConfiguration>();
                services.RemoveAll<IOptions<HikkabaConfiguration>>();
                services.RemoveAll<IOptions<SeedConfiguration>>();
                services.RemoveAll<IOptionsMonitor<HikkabaConfiguration>>();
                services.RemoveAll<IOptionsMonitor<SeedConfiguration>>();
                services.RemoveAll<MemoryDistributedCacheOptions>();

                services.RemoveAll<IdentityOptions>();
                services.RemoveAll<KeyManagementOptions>();

                services.RemoveAll<IUrlHelper>();
                services.RemoveAll<IUrlHelperFactory>();
                services.RemoveAll<IUrlHelperFactoryWrapper>();
                services.RemoveAll<IActionContextAccessor>();

                // we will use TestContainers + Respawner for DB, Redis, RabbitMQ
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<DbConnection>();
                services.RemoveAll<ApplicationDbContext>();

                services.RemoveAll<TimeProvider>();

                services
                    .Configure<ConnectionStringsSection>(o =>
                    {
                        o.ConnectionStrings.Add(new ConnectionStringSettings("DefaultConnection", string.Empty, "Microsoft.EntityFrameworkCore.InMemory"));
                    })
                    .Configure<HikkabaConfiguration>(o =>
                    {
                        o.AuthCertificateBase64 = "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\n";
                        o.AuthCertificatePassword = "f84dddc8-e695-463a-bde9-0666d28be69b";
                        o.MaintenanceKey = "sdiw3p49jiouhjuiOY@&*hoiiopHUIwh";
                        o.TripCodeSalt = "34mi2ouijnruioehiwerg3847";
                    })
                    .Configure<SeedConfiguration>(o =>
                    {
                        o.AdministratorEmail = "admin@example.com";
                        o.AdministratorPassword = "6fd71994-b5ca-4b17-aa4f-513a42dab5ad";
                    })
                    .Configure<MemoryDistributedCacheOptions>(_ => { });

                // fake Redis
                services.AddDistributedMemoryCache();

                services.AddDbContext<ApplicationDbContext>((provider, options) =>
                {
                    var webHostEnvironment = provider.GetRequiredService<IWebHostEnvironment>();
                    if (webHostEnvironment.IsDevelopment() || webHostEnvironment.IsEnvironment(Defaults.AspNetEnvIntegrationTesting))
                    {
                        options.EnableSensitiveDataLogging();
                    }

                    options
                        .UseInMemoryDatabase("HikkabaInMemoryDb")
                        .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                        .LogTo((eventId, logLevel) => logLevel >= LogLevel.Trace,
                            eventData =>
                            {
                                TestLogUtils.WriteProgressMessage(eventData.ToString());
                                TestLogUtils.WriteConsoleMessage(eventData.ToString());
                            });
                });

                services.AddDataProtection(options => options
                        .ApplicationDiscriminator = "0933a769-ed14-4abd-8947-e1705489c4e0")
                    .SetApplicationName("Hikkaba-Unit-Test");

                services.AddSingleton<TimeProvider>(x => FakeTimeProviderFactory.Create());

                services.AddSingleton<IUrlHelper, FakeUrlHelper>(x => new FakeUrlHelper(_currentAction));
                services.AddSingleton<IUrlHelperFactory, FakeUrlHelperFactory>(x => new FakeUrlHelperFactory(_currentAction));
                services.AddSingleton<IUrlHelperFactoryWrapper, UrlHelperFactoryWrapper>();
                services.AddSingleton<IActionContextAccessor, FakeActionContextAccessor>();
            })
            .ConfigureLogging(logging =>
            {
                // remove other logging providers, such as remote loggers or unnecessary event logs
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Trace);
            })
            .UseNLog()
            .UseEnvironment(Defaults.AspNetEnvIntegrationTesting);
    }
}
