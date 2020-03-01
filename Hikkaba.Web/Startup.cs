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
using Hikkaba.Services.Storage;
using Hikkaba.Web.Binding.Providers;
using Hikkaba.Web.Mapping;
using Hikkaba.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
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
using TPrimaryKey = System.Guid;
using Microsoft.Extensions.Options;
using Hikkaba.Web.Models;

namespace Hikkaba.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseLazyLoadingProxies()
                    .UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, ApplicationRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, TPrimaryKey>>()
                .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, TPrimaryKey>>();

            services.AddOptions();
            services.Configure<HikkabaConfiguration>(Configuration.GetSection(typeof(HikkabaConfiguration).Name));
            services.Configure<SeedConfiguration>(Configuration.GetSection(typeof(SeedConfiguration).Name));

            //services.AddCors(options =>
            //{
            //    options.AddPolicy("CorsPolicy",
            //        builder => builder
            //            .AllowAnyOrigin()
            //            .AllowAnyMethod()
            //            .AllowAnyHeader()
            //            .AllowCredentials());
            //}); // todo: AddCors and UseCors

            services.AddScoped<DateTimeKindSensitiveBinderProvider>();
            services.AddSingleton<IConfigureOptions<MvcOptions>, ConfigureMvcOptions>();

            services.AddMvc();
            services.AddControllersWithViews();
            services.AddRazorPages();

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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
                app.UseExceptionHandler("/Error/Details");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();

            Directory.CreateDirectory(Path.Combine(env.WebRootPath, Defaults.AttachmentsStorageDirectoryName));
            app.UseStaticFiles();

            app.UseRouting();

            app.UseHttpsRedirection();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
