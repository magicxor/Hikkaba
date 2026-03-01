using Hikkaba.Data.Context;
using Hikkaba.Data.Utils;
using Hikkaba.Web.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Web.Services;

/// <summary>
/// Use this factory ONLY for EF Core CLI tools and integration tests!
/// Do NOT use it in the application code.
/// </summary>
public class MetaDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private readonly string _connectionString;

    public MetaDesignTimeDbContextFactory()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables("Hikkaba_")
            .Build();

        this._connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    }

    public MetaDesignTimeDbContextFactory(string connectionString)
    {
        this._connectionString = connectionString;
    }

    // see:
    // https://github.com/dotnet/efcore/issues/36314#issuecomment-3120071170
    // https://github.com/dotnet/efcore/issues/35285#issuecomment-3161145762
    // https://learn.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model?view=aspnetcore-10.0#identity-and-ef-core-migrations
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "This is needed for Entity Framework CLI tools and tests")]
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var services = new ServiceCollection();

        services.AddHikkabaIdentity();

        var serviceProvider = services.BuildServiceProvider();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder
            .UseApplicationServiceProvider(serviceProvider)
            .UseSqlServer(_connectionString, ContextConfiguration.SqlServerOptionsAction);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
