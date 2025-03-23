using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(Hikkaba.Web.Areas.Identity.IdentityHostingStartup))]
namespace Hikkaba.Web.Areas.Identity;

public class IdentityHostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) => {
        });
    }
}
