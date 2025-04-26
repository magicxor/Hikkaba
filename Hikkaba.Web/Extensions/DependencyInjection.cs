using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.RateLimiting;
using DNTCaptcha.Core;
using Hikkaba.Application.Contracts;
using Hikkaba.Application.Implementations;
using Hikkaba.Application.Telemetry.Metrics;
using Hikkaba.Data.Context;
using Hikkaba.Data.Utils;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Infrastructure.Repositories.Implementations;
using Hikkaba.Infrastructure.Repositories.Implementations.Interceptors;
using Hikkaba.Shared.Constants;
using Hikkaba.Shared.Services.Contracts;
using Hikkaba.Shared.Services.Implementations;
using Hikkaba.Web.ActionFilters;
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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MyCSharp.HttpUserAgentParser.AspNetCore.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sakura.AspNetCore.Mvc;
using TwentyTwenty.Storage;
using TwentyTwenty.Storage.Local;
using MyCSharp.HttpUserAgentParser.MemoryCache.DependencyInjection;

namespace Hikkaba.Web.Extensions;

internal static class DependencyInjection
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

            var auditColumnWriter = provider.GetRequiredService<IAuditColumnWriter>();

            options
                .UseSqlServer(connectionString, ContextConfiguration.SqlServerOptionsAction)
                .AddInterceptors(auditColumnWriter);
        });

        return services;
    }

    internal static IServiceCollection AddHikkabaDbContext(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (!webHostEnvironment.IsEnvironment(Defaults.AspNetEnvIntegrationTesting) || !string.IsNullOrEmpty(connectionString))
        {
            // "EmptyConnectionString" is used for dotnet ef tools
            return services.AddHikkabaDbContext(connectionString ?? "EmptyConnectionString");
        }

        return services;
    }

    internal static IServiceCollection AddHikkabaRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAuditColumnWriter, AuditColumnWriter>();
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
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }

    internal static IServiceCollection AddHikkabaServices(this IServiceCollection services)
    {
        // core
        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<IVersionInfoService, VersionInfoService>();

        // GeoIP
        services.AddSingleton<GeoIpAsnReader>(x => new GeoIpAsnReader("./GeoIp/GeoLite2-ASN.mmdb"));
        services.AddSingleton<GeoIpCountryReader>(x => new GeoIpCountryReader("./GeoIp/GeoLite2-Country.mmdb"));
        services.AddSingleton<IGeoIpService, GeoIpService>();

        // general
        services.AddScoped<ISeedService, SeedService>();
        services.AddScoped<IMigrationService, MigrationService>();
        services.AddScoped<IAuthMessageSender, AuthMessageSender>();
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
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();

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

        services.AddHttpUserAgentMemoryCachedParser()
            .AddHttpUserAgentParserAccessor();

        return services;
    }

    internal static IServiceCollection AddHikkabaCookieConfig(this IServiceCollection services)
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

    internal static IServiceCollection AddHikkabaDataProtection(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        var hikkabaConfig = configuration.GetSection(nameof(HikkabaConfiguration)).Get<HikkabaConfiguration>();

        if (webHostEnvironment.IsEnvironment(Defaults.AspNetEnvIntegrationTesting) || hikkabaConfig == null)
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

    internal static IServiceCollection AddHikkabaHttpServerConfig(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);

        services.AddResponseCompression();

        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken);
            };

            rateLimiterOptions.GlobalLimiter = RateLimiterFactory.CreateSlidingPerEndpointPerIpPerMinute(
                RateLimiterFactory.DefaultEndpointRateLimits);
        });

        return services;
    }

    internal static IServiceCollection ConfigureHikkabaIdentity(this IServiceCollection services)
    {
        return services
            .Configure<IdentityOptions>(o =>
            {
                o.SignIn.RequireConfirmedAccount = true;
                o.SignIn.RequireConfirmedEmail = true;
                o.SignIn.RequireConfirmedPhoneNumber = false;

                o.User.RequireUniqueEmail = true;
                o.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyz0123456789_";

                o.Password.RequireLowercase = true;
                o.Password.RequireUppercase = true;
                o.Password.RequireDigit = true;
                o.Password.RequireNonAlphanumeric = true;
                o.Password.RequiredUniqueChars = 4;
                o.Password.RequiredLength = Defaults.MinUserPasswordLength;
            });
    }

    internal static IServiceCollection AddHikkabaMvc(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        services.AddSingleton<DateTimeKindSensitiveBinderProvider>();
        services.AddSingleton<IConfigureOptions<MvcOptions>, ConfigureMvcOptions>();

        services.AddDatabaseDeveloperPageExceptionFilter();
        services.AddControllersWithViews(options =>
        {
            options.Filters.Add<StopwatchActionFilter>();
        });
        services.AddRazorPages();
        services.AddBootstrapPagerGenerator(options => options.ConfigureDefault());

        var hikkabaConfig = configuration.GetSection(nameof(HikkabaConfiguration)).Get<HikkabaConfiguration>();

        // disable captcha for integration testing
        if (webHostEnvironment.IsEnvironment(Defaults.AspNetEnvIntegrationTesting) || hikkabaConfig == null)
        {
            return services;
        }

        services.AddDNTCaptcha(options => options
            .UseCookieStorageProvider(SameSiteMode.Strict)
            .AbsoluteExpiration(minutes: 7)
            .RateLimiterPermitLimit(50)
            .WithRateLimiterRejectResponse("Rate limit exceeded")
            .WithNoise(0.015f, 0.015f, 1, 0.0f)
            .WithNonceKey("NETESCAPADES_NONCE")
            .WithEncryptionKey(hikkabaConfig.AuthCertificatePassword));

        return services;
    }

    internal static IServiceCollection AddHikkabaObservabilityTools(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
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

        if (otlpExporterUri is not null)
        {
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(Defaults.ServiceName))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                    .AddEntityFrameworkCoreInstrumentation(o =>
                    {
                        o.SetDbStatementForText = true;
                        o.SetDbStatementForStoredProcedure = true;
                    })
                    .AddHttpClientInstrumentation()
                    .AddSource("Hikkaba.*")
                    .AddOtlpExporter(options => options.Endpoint = otlpExporterUri))
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddMeter("Microsoft.EntityFrameworkCore")
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddMeter("mailkit.*")
                    .AddMeter("Hikkaba.*")
                    .AddOtlpExporter(options => options.Endpoint = otlpExporterUri));
        }

        return services;
    }

    internal static IServiceCollection AddHikkabaMetrics(this IServiceCollection services)
    {
        return services
            .AddSingleton<PostMetrics>()
            .AddSingleton<ThreadMetrics>()
            .AddSingleton<AuthMessageSenderMetrics>();
    }
}
