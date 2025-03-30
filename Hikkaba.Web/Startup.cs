using System.IO;
using DNTCaptcha.Core;
using Hikkaba.Shared.Constants;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Web.Binding.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sakura.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Hikkaba.Web.Models;
using TwentyTwenty.Storage.Local;
using TwentyTwenty.Storage;
using Microsoft.AspNetCore.DataProtection;
using System;
using Hikkaba.Shared.Enums;
using Hikkaba.Shared.Exceptions;
using Hikkaba.Shared.Services.Contracts;
using Hikkaba.Shared.Services.Implementations;
using Hikkaba.Data.Utils;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Infrastructure.Models.Extensions;
using Hikkaba.Application.Contracts;
using Hikkaba.Application.Implementations;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Infrastructure.Repositories.Implementations;
using Hikkaba.Web.Middleware;
using Hikkaba.Web.Services.Contracts;
using Hikkaba.Web.Services.Implementations;
using Hikkaba.Web.Utils;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web;

public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;

    private static readonly Action<ILogger, string, Exception?> LogMessage =
        LoggerMessage.Define<string>(LogLevel.Debug, LogEventIds.DbQuery, "DB query: {Message}");

    public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        _configuration = configuration;
        _webHostEnvironment = webHostEnvironment;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.MinimumSameSitePolicy = SameSiteMode.Strict;
            options.HttpOnly = HttpOnlyPolicy.Always;
        });

        services.AddDbContext<ApplicationDbContext>((provider, options) =>
        {
            var logger = provider.GetRequiredService<ILogger<ApplicationDbContext>>();
            var webHostEnvironment = provider.GetRequiredService<IWebHostEnvironment>();
            if (webHostEnvironment.IsDevelopment() || webHostEnvironment.IsEnvironment(Defaults.AspNetEnvIntegrationTesting))
            {
                options.EnableSensitiveDataLogging();
            }

            options
                .UseSqlServer(_configuration.GetConnectionString("DefaultConnection"), ContextConfiguration.SqlServerOptionsAction)
                .LogTo(msg => LogMessage(logger, msg, null));
        });

        services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, int>>()
            .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, int>>();

        services.AddOptionsWithValidateOnStart<HikkabaConfiguration>()
            .Bind(_configuration.GetSection(nameof(HikkabaConfiguration)))
            .ValidateDataAnnotations();

        services.AddOptionsWithValidateOnStart<SeedConfiguration>()
            .Bind(_configuration.GetSection(nameof(SeedConfiguration)))
            .ValidateDataAnnotations();

        services.AddSingleton<DateTimeKindSensitiveBinderProvider>();
        services.AddSingleton<IConfigureOptions<MvcOptions>, ConfigureMvcOptions>();
        services.AddSingleton<GeoIpAsnReader>(x => new GeoIpAsnReader("./GeoIp/GeoLite2-ASN.mmdb"));
        services.AddSingleton<GeoIpCountryReader>(x => new GeoIpCountryReader("./GeoIp/GeoLite2-Country.mmdb"));
        services.AddSingleton<IGeoIpService, GeoIpService>();

        services.AddHealthChecks();
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(Defaults.UserIdleTimeoutMinutes);
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        var hikkabaConfiguration = _configuration.GetSection(nameof(HikkabaConfiguration)).Get<HikkabaConfiguration>();
        if (hikkabaConfiguration != null)
        {
            if (!Directory.Exists(hikkabaConfiguration.KeysetDirectoryPath))
            {
                Directory.CreateDirectory(hikkabaConfiguration.KeysetDirectoryPath);
            }

            services.AddDataProtection(options =>
                {
                    options.ApplicationDiscriminator = "4036e12c07fa7f8fb6f58a70c90ee85b52c15be531acf7bd0d480d1ca7f9ea5d";
                })
                .SetApplicationName("Hikkaba")
                .ProtectKeysWithCertificate(CertificateUtils.LoadCertificate(hikkabaConfiguration))
                .PersistKeysToDbContext<ApplicationDbContext>();

            // Captcha
            services.AddDNTCaptcha(options => options
                .UseSessionStorageProvider()
                .WithEncryptionKey(hikkabaConfiguration?.AuthCertificatePassword ?? throw new HikkabaConfigException("AuthCertificatePassword is not set")));
        }

        services.AddControllersWithViews();
        services.AddRazorPages();

        services.AddBootstrapPagerGenerator(options =>
        {
            options.ConfigureDefault();
        });

        // add services
        services.AddScoped<IEmailSender, AuthMessageSender>();
        services.AddScoped<ISmsSender, AuthMessageSender>();
        services.AddScoped<IUrlHelperFactoryWrapper, UrlHelperFactoryWrapper>();
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
        services.AddScoped<IThreadLocalUserHashGenerator, ThreadLocalUserHashGenerator>();
        services.AddScoped<IThreadService, ThreadService>();
        services.AddScoped<IThumbnailGenerator, ThumbnailGenerator>();
        services.AddScoped<IAttachmentService, AttachmentService>();

        // add repositories
        services.AddScoped<IAdministrationRepository, AdministrationRepository>();
        services.AddScoped<IBanRepository, BanRepository>();
        services.AddScoped<IBoardRepository, BoardRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IRolesRepository, RolesRepository>();
        services.AddScoped<ISeedManager, SeedManager>();
        services.AddScoped<IThreadRepository, ThreadRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();

        // File storage
        services.AddScoped<FileExtensionContentTypeProvider>();
        services.AddScoped<IStorageProvider>(s =>
        {
            var webHostEnvironment = s.GetRequiredService<IWebHostEnvironment>();
            var path = Path.Combine(webHostEnvironment.WebRootPath, Defaults.AttachmentsStorageDirectoryName);
            Directory.CreateDirectory(path);
            return new LocalStorageProvider(path);
        });
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.AddScoped<IMessagePostProcessor, MessagePostProcessor>();
        services.AddScoped<IUserContext, UserContext>();

        services.AddResponseCompression();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<HikkabaConfiguration> settings)
    {
        var cacheMaxAgeSeconds = settings.Value.GetCacheMaxAgeSecondsOrDefault();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseStatusCodePagesWithReExecute("/Error/Details", "?statusCode={0}");
            app.UseExceptionHandler("/Error/Exception");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseResponseCompression();
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.CacheControl] = $"public,max-age={cacheMaxAgeSeconds}";
            },
        });

        app.UseSession();
        app.UseRouting();

        app.UseHttpsRedirection();
        app.UseCookiePolicy();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<SetAuthenticatedUserMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/Health");
            endpoints.MapRazorPages();
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            endpoints.MapFallbackToController("PageNotFound", "Error");
        });
    }
}
