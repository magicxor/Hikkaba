using DNTCaptcha.Core;
using Hikkaba.Shared.Constants;
using Hikkaba.Data.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection;
using Hikkaba.Shared.Exceptions;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Infrastructure.Models.Extensions;
using Hikkaba.Web.Extensions;
using Hikkaba.Web.Middleware;
using Hikkaba.Web.Utils;

namespace Hikkaba.Web;

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
        services.AddOptionsWithValidateOnStart<HikkabaConfiguration>()
            .Bind(_configuration.GetSection(nameof(HikkabaConfiguration)))
            .ValidateDataAnnotations();

        services.AddOptionsWithValidateOnStart<SeedConfiguration>()
            .Bind(_configuration.GetSection(nameof(SeedConfiguration)))
            .ValidateDataAnnotations();

        services.AddHikkabaDbContext(_configuration.GetConnectionString("DefaultConnection"));

        var hikkabaConfig = _configuration.GetSection(nameof(HikkabaConfiguration)).Get<HikkabaConfiguration>()
                            ?? throw new HikkabaConfigException($"{nameof(HikkabaConfiguration)} is null");

        services.AddDataProtection(options =>
            {
                options.ApplicationDiscriminator = "4036e12c07fa7f8fb6f58a70c90ee85b52c15be531acf7bd0d480d1ca7f9ea5d";
            })
            .SetApplicationName(Defaults.ServiceName)
            .ProtectKeysWithCertificate(CertificateUtils.LoadCertificate(hikkabaConfig))
            .PersistKeysToDbContext<ApplicationDbContext>();

        if (!_webHostEnvironment.IsEnvironment(Defaults.AspNetEnvIntegrationTesting))
        {
            services.AddDNTCaptcha(options => options
                .UseSessionStorageProvider()
                .WithEncryptionKey(hikkabaConfig.AuthCertificatePassword));
        }

        services.AddHikkabaRepositories()
            .AddHikkabaServices()
            .AddHikkabaCookieConfig()
            .AddHikkabaMvc()
            .AddObservabilityTools(_webHostEnvironment);

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
            endpoints.MapHealthChecks("/health");
            endpoints.MapRazorPages();
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            endpoints.MapFallbackToController("PageNotFound", "Error");
        });
    }
}
