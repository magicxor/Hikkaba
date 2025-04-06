using Hikkaba.Shared.Constants;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Infrastructure.Models.Extensions;
using Hikkaba.Web.Extensions;
using Hikkaba.Web.Middleware;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Hikkaba.Web;

internal class Startup
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
        services.AddOptionsWithValidateOnStart<HikkabaConfiguration>()
            .Bind(_configuration.GetSection(nameof(HikkabaConfiguration)))
            .ValidateDataAnnotations();

        services.AddOptionsWithValidateOnStart<SeedConfiguration>()
            .Bind(_configuration.GetSection(nameof(SeedConfiguration)))
            .ValidateDataAnnotations();

        services.AddHikkabaDbContext(_configuration, _webHostEnvironment);

        services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, int>>()
            .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, int>>();

        services.AddHikkabaDataProtection(_configuration, _webHostEnvironment)
            .AddHikkabaHttpServerConfig()
            .AddHikkabaRepositories()
            .AddHikkabaServices()
            .AddHikkabaCookieConfig()
            .AddHikkabaMvc(_configuration, _webHostEnvironment)
            .AddHikkabaObservabilityTools(_configuration, _webHostEnvironment)
            .AddHikkabaMetrics();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<HikkabaConfiguration> settings)
    {
        var cacheMaxAgeSeconds = settings.Value.GetCacheMaxAgeSecondsOrDefault();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStatusCodePagesWithReExecute("/Error/Details", "?statusCode={0}");
        app.UseExceptionHandler("/Error/Exception");

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
        app.UseRateLimiter();

        app.UseCookiePolicy();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<SetAuthenticatedUserMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            if (!env.IsEnvironment(Defaults.AspNetEnvIntegrationTesting))
            {
                endpoints.MapHealthChecks("/health");
            }

            endpoints.MapRazorPages();
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            endpoints.MapFallbackToController("PageNotFound", "Error");
        });
    }
}
