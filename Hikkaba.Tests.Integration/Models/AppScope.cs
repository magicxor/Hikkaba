using Hikkaba.Data.Context;
using Hikkaba.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Models;

internal sealed class AppScope : IDisposable
{
    public required ApplicationDbContext ApplicationDbContext { get; set; }

    public required IServiceScope ServiceScope { get; set; }
    public required WebApplicationFactory<Program> AppFactory { get; set; }

    public void Dispose()
    {
        ServiceScope.Dispose();
        AppFactory.Dispose();
    }
}
