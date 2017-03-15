using System;
using System.IO;
using AutoMapper;
using Hikkaba.Common.Data;
using Hikkaba.Common.Entities;
using Hikkaba.Common.Mapping;
using Hikkaba.Service.Ref;
using Hikkaba.Web.Mapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Scrutor;
using DNTCaptcha.Core;
using Hikkaba.Common.Configuration;
using Hikkaba.Common.Constants;
using Hikkaba.Common.Storage.Implementations;
using Hikkaba.Common.Storage.Interfaces;
using Hikkaba.Service;
using Hikkaba.Web.Binding.Providers;
using Hikkaba.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using Sakura.AspNetCore.Mvc;
using TwentyTwenty.Storage;

namespace Hikkaba.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("seedconfig.json", optional: false, reloadOnChange: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets("Hikkaba");

                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext, Guid>()
                .AddDefaultTokenProviders()
                .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
                .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();

            services.AddOptions();
            services.Configure<HikkabaConfiguration>(Configuration.GetSection(typeof(HikkabaConfiguration).Name));
            services.Configure<SeedConfiguration>(Configuration.GetSection(typeof(SeedConfiguration).Name));

            services.AddMvc(options =>
            {
                options.ModelBinderProviders.Insert(0, new DateTimeKindSensitiveBinderProvider());
            });

            services.AddBootstrapPagerGenerator(options =>
            {
                options.ConfigureDefault();
            });

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            // Captcha
            services.AddDNTCaptcha();

            // File storage
            services.AddScoped<IStoragePathProvider, LocalStoragePathProvider>();
            services.AddScoped<IStorageProviderFactory, LocalStorageProviderFactory>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IMessagePostProcessor, MessagePostProcessor>();

            // AutoMapper
            services.AddAutoMapper(typeof(MapProfile), typeof(MvcMapProfile));

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, ApplicationDbContext context, IStoragePathProvider storagePathProvider)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddNLog();

            // nlog: needed for non-NETSTANDARD platforms: configure nlog.config in your project root
            env.ConfigureNLog("nlog.config");

            var logger = loggerFactory.CreateLogger<Startup>();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error/Details");
            }

            app.UseStatusCodePagesWithReExecute("/Error/{0}");
            app.UseIdentity();

            Directory.CreateDirectory(Path.Combine(env.WebRootPath, Defaults.AttachmentsStorageDirectoryName));
            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // http://coderhints.com/ef7-seed-user/
            try
            {
                using (var serviceScope = app
                                        .ApplicationServices
                                        .GetRequiredService<IServiceScopeFactory>()
                                        .CreateScope())
                {
                    serviceScope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();

                    var userManager = app.ApplicationServices.GetService<UserManager<ApplicationUser>>();
                    var roleManager = app.ApplicationServices.GetService<RoleManager<ApplicationRole>>();
                    var seedConfig = app.ApplicationServices.GetService<IOptions<SeedConfiguration>>();

                    DbSeeder.SeedAsync(context, userManager, roleManager, seedConfig).Wait();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                throw;
            }
        }
    }
}