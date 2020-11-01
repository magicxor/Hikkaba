using TPrimaryKey = System.Guid;
using System.IO;
using AutoMapper;
using DNTCaptcha.Core;
using Hikkaba.Common.Constants;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Mapping;
using Hikkaba.Models.Configuration;
using Hikkaba.Services;
using Hikkaba.Services.Ref;
using Hikkaba.Web.Binding.Providers;
using Hikkaba.Web.Mapping;
using Hikkaba.Web.Services;
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
using Hikkaba.Data.Services;
using Hikkaba.Models.Extensions;
using Hikkaba.Web.Middleware;
using Hikkaba.Web.Utils;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.CookiePolicy;
using Hikkaba.Web.Extensions;

namespace Hikkaba.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

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
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
                options.HttpOnly = HttpOnlyPolicy.Always;
            });

            services.AddDbContext<ApplicationDbContext>((provider, options) =>
            {
                var webHostEnvironment = provider.GetRequiredService<IWebHostEnvironment>();
                if (webHostEnvironment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                }
                options
                    .UseLazyLoadingProxies()
                    .UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, TPrimaryKey>>()
                .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, TPrimaryKey>>();

            services.AddOptions();
            services.Configure<HikkabaConfiguration>(_configuration.GetSection(nameof(HikkabaConfiguration)));
            services.Configure<SeedConfiguration>(_configuration.GetSection(nameof(SeedConfiguration)));

            services.AddSingleton<DateTimeKindSensitiveBinderProvider>();
            services.AddSingleton<IConfigureOptions<MvcOptions>, ConfigureMvcOptions>();

            services.AddHealthChecks();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var hikkabaConfiguration = _configuration.GetSection(nameof(HikkabaConfiguration)).Get<HikkabaConfiguration>();
            if (hikkabaConfiguration != null)
            {
                services.AddDataProtection(options =>
                {
                    options.ApplicationDiscriminator = "4036e12c07fa7f8fb6f58a70c90ee85b52c15be531acf7bd0d480d1ca7f9ea5d";
                })
               .SetApplicationName("Hikkaba")
               .ProtectKeysWithCertificate(CertificateUtils.LoadCertificate(hikkabaConfiguration))
               .PersistKeysToFileSystem(new DirectoryInfo("/home/hikkaba/keys"));
            }

            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddBootstrapPagerGenerator(options =>
            {
                options.ConfigureDefault();
            });

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddScoped<IUrlHelperFactoryWrapper, UrlHelperFactoryWrapper>();

            // Captcha
            if (_webHostEnvironment.EnvironmentName != Defaults.AspNetEnvIntegrationTesting)
            {
                services.AddDNTCaptcha(options => options.UseSessionStorageProvider());
            }
            else
            {
                services.AddDNTCaptchaMock();
            }

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
            services.AddScoped<IAuthenticatedUserService, AuthenticatedUserService>();

            // AutoMapper
            var mapperConfiguration = new MapperConfiguration(expression =>
            {
                expression.DisableConstructorMapping();
                expression.AddProfile<MapProfile>();
                expression.AddProfile<MvcMapProfile>();
            });
            mapperConfiguration.AssertConfigurationIsValid();
            mapperConfiguration.CompileMappings();
            services.AddSingleton<IMapper>(new Mapper(mapperConfiguration));

            // Hikkaba stuff
            services.Scan(scan => scan
                // We start out with all types in the assembly of ITransientService
                .FromAssemblyOf<HikkabaServiceRef>()
                // AddClasses starts out with all public, non-abstract types in this assembly.
                // These types are then filtered by the delegate passed to the method.
                .AddClasses()
                // Whe then specify what type we want to register these classes as.
                // In this case, we wan to register the types as all of its implemented interfaces.
                // So if a type implements 3 interfaces; A, B, C, we'd end up with three separate registrations.
                .AsImplementedInterfaces()
                // And lastly, we specify the lifetime of these registrations.
                .WithScopedLifetime());

            services.AddResponseCompression();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<HikkabaConfiguration> settings)
        {
            var cacheMaxAgeSeconds = settings.Value.GetCacheMaxAgeSecondsOrDefault();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
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
                }
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
}
