using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Models;

internal sealed class AppScope : IAppFactoryScope
{
    public required IServiceScope ServiceScope { get; set; }
    public required CustomAppFactory AppFactory { get; set; }

    public void Dispose()
    {
        ServiceScope.Dispose();
        AppFactory.Dispose();
    }
}
