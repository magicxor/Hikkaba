using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using DNTCaptcha.Core;
using Hikkaba.Application.Contracts;
using Hikkaba.Application.Implementations;
using Hikkaba.Application.Telemetry.Metrics;
using Hikkaba.Data.Context;
using Hikkaba.Data.Utils;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Infrastructure.Repositories.Implementations;
using Hikkaba.Shared.Constants;
using Hikkaba.Shared.Exceptions;
using Hikkaba.Shared.Services.Contracts;
using Hikkaba.Shared.Services.Implementations;
using Hikkaba.Web.Binding.Providers;
using Hikkaba.Web.Metrics;
using Hikkaba.Web.Models;
using Hikkaba.Web.Services.Contracts;
using Hikkaba.Web.Services.Implementations;
using Hikkaba.Web.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sakura.AspNetCore.Mvc;
using TwentyTwenty.Storage;
using TwentyTwenty.Storage.Local;

namespace Hikkaba.Web.Extensions;

public static class DependencyInjection
{
    private static IServiceCollection AddHikkabaDbContext(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);

        services.AddDbContext<ApplicationDbContext>((provider, options) =>
        {
            var webHostEnvironment = provider.GetRequiredService<IWebHostEnvironment>();
            if (webHostEnvironment.IsDevelopment() || webHostEnvironment.IsEnvironment(Defaults.AspNetEnvIntegrationTesting))
            {
                options.EnableSensitiveDataLogging();
            }

            options.UseSqlServer(connectionString, ContextConfiguration.SqlServerOptionsAction);
        });

        return services;
    }

    public static IServiceCollection AddHikkabaDbContext(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (!webHostEnvironment.IsEnvironment(Defaults.AspNetEnvIntegrationTesting) || !string.IsNullOrEmpty(connectionString))
        {
            return services.AddHikkabaDbContext(connectionString ?? throw new HikkabaConfigException("No connection string found."));
        }

        return services;
    }

    public static IServiceCollection AddHikkabaRepositories(this IServiceCollection services)
    {
        services.AddScoped<ISeedRepository, SeedRepository>();
        services.AddScoped<IMigrationRepository, MigrationRepository>();
        services.AddScoped<IAdministrationRepository, AdministrationRepository>();
        services.AddScoped<IBanRepository, BanRepository>();
        services.AddScoped<IBoardRepository, BoardRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IThreadRepository, ThreadRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();

        return services;
    }

    public static IServiceCollection AddHikkabaServices(this IServiceCollection services)
    {
        // core
        services.AddScoped<IUserContext, UserContext>();

        // GeoIP
        services.AddSingleton<GeoIpAsnReader>(x => new GeoIpAsnReader("./GeoIp/GeoLite2-ASN.mmdb"));
        services.AddSingleton<GeoIpCountryReader>(x => new GeoIpCountryReader("./GeoIp/GeoLite2-Country.mmdb"));
        services.AddSingleton<IGeoIpService, GeoIpService>();

        // general
        services.AddScoped<ISeedService, SeedService>();
        services.AddScoped<IMigrationService, MigrationService>();
        services.AddScoped<IEmailSender, AuthMessageSender>();
        services.AddScoped<ISmsSender, AuthMessageSender>();
        services.AddScoped<IAdministrationService, AdministrationService>();
        services.AddScoped<IAttachmentCategorizer, AttachmentCategorizer>();
        services.AddScoped<IBanService, BanService>();
        services.AddScoped<IBoardService, BoardService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IGeoIpService, GeoIpService>();
        services.AddScoped<IHmacService, HmacService>();
        services.AddScoped<IHashService, HashService>();
        services.AddScoped<IIpAddressCalculator, IpAddressCalculator>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ISystemInfoService, SystemInfoService>();
        services.AddScoped<IThreadService, ThreadService>();
        services.AddScoped<IThumbnailGenerator, ThumbnailGenerator>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IBanCreationPrerequisiteService, BanCreationPrerequisiteService>();

        // file storage
        services.AddScoped<FileExtensionContentTypeProvider>();
        services.AddScoped<IStorageProvider>(s =>
        {
            var options = s.GetService<IOptions<HikkabaConfiguration>>();
            var storagePath = options?.Value.StoragePath;
            var webHostEnvironment = s.GetRequiredService<IWebHostEnvironment>();
            var fileStorageDirectory = storagePath ?? Path.Combine(webHostEnvironment.WebRootPath, Defaults.AttachmentsStorageDirectoryName);
            Directory.CreateDirectory(fileStorageDirectory);
            return new LocalStorageProvider(fileStorageDirectory);
        });

        // declared in presentation layer
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.AddScoped<IUrlHelperFactoryWrapper, UrlHelperFactoryWrapper>();
        services.AddScoped<IMessagePostProcessor, MessagePostProcessor>();

        return services;
    }

    public static IServiceCollection AddHikkabaCookieConfig(this IServiceCollection services)
    {
        services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.MinimumSameSitePolicy = SameSiteMode.Strict;
            options.HttpOnly = HttpOnlyPolicy.Always;
        });

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(Defaults.UserIdleTimeoutMinutes);
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        return services;
    }

    public static IServiceCollection AddHikkabaDataProtection(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        if (webHostEnvironment.IsEnvironment(Defaults.AspNetEnvIntegrationTesting))
        {
            services.AddDataProtection(options =>
                {
                    options.ApplicationDiscriminator = "48765fc1-4c52-4987-bc20-e0b1d8bd760f";
                })
                .SetApplicationName(Defaults.ServiceName)
                .UseEphemeralDataProtectionProvider();
        }
        else
        {
            var hikkabaConfig = configuration.GetSection(nameof(HikkabaConfiguration)).Get<HikkabaConfiguration>()
                                ?? throw new HikkabaConfigException($"{nameof(HikkabaConfiguration)} is null");

            services.AddDataProtection(options =>
                {
                    options.ApplicationDiscriminator = "4036e12c07fa7f8fb6f58a70c90ee85b52c15be531acf7bd0d480d1ca7f9ea5d";
                })
                .SetApplicationName(Defaults.ServiceName)
                .ProtectKeysWithCertificate(CertificateUtils.LoadCertificate(hikkabaConfig))
                .PersistKeysToDbContext<ApplicationDbContext>();
        }

        return services;
    }

    public static IServiceCollection AddHikkabaMvc(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        services.AddSingleton<DateTimeKindSensitiveBinderProvider>();
        services.AddSingleton<IConfigureOptions<MvcOptions>, ConfigureMvcOptions>();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        services.AddControllersWithViews();
        services.AddRazorPages();
        services.AddBootstrapPagerGenerator(options =>
        {
            options.ConfigureDefault();
        });

        // disable captcha for integration testing
        if (webHostEnvironment.IsEnvironment(Defaults.AspNetEnvIntegrationTesting))
        {
            return services;
        }

        var hikkabaConfig = configuration.GetSection(nameof(HikkabaConfiguration)).Get<HikkabaConfiguration>()
                            ?? throw new HikkabaConfigException($"{nameof(HikkabaConfiguration)} is null");

        services.AddDNTCaptcha(options => options
            .UseCookieStorageProvider(SameSiteMode.Strict)
            .AbsoluteExpiration(minutes: 7)
            .RateLimiterPermitLimit(20)
            .WithRateLimiterRejectResponse("Rate limit exceeded")
            .WithNoise(0.015f, 0.015f, 1, 0.0f)
            .WithNonceKey("NETESCAPADES_NONCE")
            .WithEncryptionKey(hikkabaConfig.AuthCertificatePassword));

        return services;
    }

    public static IServiceCollection AddHikkabaObservabilityTools(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        // disable observability tools for integration testing
        if (webHostEnvironment.IsEnvironment(Defaults.AspNetEnvIntegrationTesting))
        {
            return services;
        }

        // add fake listener for better development experience
        if (webHostEnvironment.IsDevelopment())
        {
            DiagnosticListener.AllListeners.Subscribe(new DiagnosticObserver());
        }

        services.AddHealthChecks();

        var hikkabaConfig = configuration.GetSection(nameof(HikkabaConfiguration)).Get<HikkabaConfiguration>();
        var otlpExporterUri = hikkabaConfig?.OtlpExporterUri;

        if (!string.IsNullOrEmpty(otlpExporterUri))
        {
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(Defaults.ServiceName))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options => options.Endpoint = new Uri(otlpExporterUri)))
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddOtlpExporter(options => options.Endpoint = new Uri(otlpExporterUri)));
        }

        return services;
    }

    public static IServiceCollection AddHikkabaMetrics(this IServiceCollection services)
    {
        return services
            .AddSingleton<PostMetrics>()
            .AddSingleton<ThreadMetrics>();
    }
}
