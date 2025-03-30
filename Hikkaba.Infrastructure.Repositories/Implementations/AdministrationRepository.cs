using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Mappings;
using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public class AdministrationRepository : IAdministrationRepository
{
    private readonly ILogger<AdministrationRepository> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IServiceScopeFactory _scopeFactory;

    private static readonly string[] SupportedDbProviders =
    [
        "Microsoft.EntityFrameworkCore.SqlServer",
    ];

    public AdministrationRepository(
        ILogger<AdministrationRepository> logger,
        ApplicationDbContext context,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _context = context;
        _scopeFactory = scopeFactory;
    }

    public async Task<DashboardModel> GetDashboardAsync()
    {
        var dashboardItems = await _context.Categories
            .Include(category => category.CreatedBy)
            .Include(category => category.ModifiedBy)
            .OrderBy(category => category.Alias)
            .Select(
                category =>
                    new
                    {
                        Category = category,
                        Moderators = category.Moderators
                            .OrderBy(categoryToModerator => categoryToModerator.Moderator.UserName)
                            .Select(categoryToModerator => categoryToModerator.Moderator)
                            .ToList(),
                    })
            .ToListAsync();

        var dashboardItemsDto = dashboardItems
            .Select(categoryModerators => new CategoryModeratorsModel
            {
                Category = categoryModerators.Category.ToDashboardModel(),
                Moderators = categoryModerators.Moderators.ToPreviews(),
            })
            .ToList();

        return new DashboardModel
        {
            CategoriesModerators = dashboardItemsDto,
        };
    }

    public async Task WipeDatabaseAsync()
    {
        if (SupportedDbProviders.Contains(_context.Database.ProviderName))
        {
            _logger.LogWarning("Wiping database...");

            await _context.Database.ExecuteSqlRawAsync(
                """
                DECLARE @sql NVARCHAR(2000);

                WHILE EXISTS ( SELECT 1
                               FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                               WHERE
                                 (CONSTRAINT_TYPE = 'FOREIGN KEY') AND
                                 (TABLE_NAME IN (
                                   SELECT TABLE_NAME
                                   FROM INFORMATION_SCHEMA.TABLES
                                   WHERE (TABLE_TYPE='BASE TABLE') AND (TABLE_NAME NOT LIKE 'sys.%')
                                 ))
                             )
                    BEGIN
                        SELECT TOP 1 @sql = 'ALTER TABLE '+TABLE_SCHEMA+'.['+TABLE_NAME+'] DROP CONSTRAINT ['+CONSTRAINT_NAME+']'
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                               WHERE
                                 (CONSTRAINT_TYPE = 'FOREIGN KEY') AND
                                 (TABLE_NAME IN (
                                   SELECT TABLE_NAME
                                   FROM INFORMATION_SCHEMA.TABLES
                                   WHERE (TABLE_TYPE='BASE TABLE') AND (TABLE_NAME NOT LIKE 'sys.%')
                                 ))
                        EXEC (@sql);
                    END;

                WHILE EXISTS ( SELECT 1
                               FROM INFORMATION_SCHEMA.TABLES
                               WHERE (TABLE_TYPE='BASE TABLE') AND (TABLE_NAME NOT LIKE 'sys.%')
                             )
                    BEGIN
                        SELECT TOP 1 @sql = 'DROP TABLE '+TABLE_SCHEMA+'.['+TABLE_NAME+']'
                        FROM INFORMATION_SCHEMA.TABLES
                        WHERE (TABLE_TYPE='BASE TABLE') AND (TABLE_NAME NOT LIKE 'sys.%');
                        EXEC (@sql);
                    END;
                """);
        }
        else
        {
            _logger.LogWarning("Deleting database...");

            await _context.Database.EnsureDeletedAsync();
        }
    }

    public async Task RunSeedInNewScopeAsync()
    {
        _logger.LogInformation("Running seed...");

        // new scope to reset EF cache
        using var scope = _scopeFactory.CreateScope();

        var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var seedSettings = scope.ServiceProvider.GetRequiredService<IOptions<SeedConfiguration>>();
        var seedManager = scope.ServiceProvider.GetRequiredService<ISeedManager>();

        if ((await applicationDbContext.Database.GetPendingMigrationsAsync()).Any())
        {
            await applicationDbContext.Database.MigrateAsync();
        }

        await seedManager.SeedAsync(applicationDbContext, userMgr, roleMgr, seedSettings);
    }
}
