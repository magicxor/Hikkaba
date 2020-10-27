using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Constants;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Configuration;
using Hikkaba.Models.Dto;
using Hikkaba.Models.Dto.Administration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwentyTwenty.Storage;

namespace Hikkaba.Services
{
    public interface IAdministrationService
    {
        Task<DashboardDto> GetDashboardAsync();
        Task DeleteAllContentAsync();
    }

    public class AdministrationService : IAdministrationService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IServiceScopeFactory _scopeFactory;

        public AdministrationService(
            ILogger<AdministrationService> logger,
            ApplicationDbContext context,
            IStorageProvider storageProvider,
            IMapper mapper,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _scopeFactory = scopeFactory;
            _storageProvider = storageProvider;
        }

        public async Task<DashboardDto> GetDashboardAsync()
        {
            var dashboardItems = await _context.Categories
                .OrderBy(category => category.Alias)
                .Select(
                    category =>
                    new
                    {
                        Category = category,
                        Moderators = category.Moderators
                            .OrderBy(categoryToModerator => categoryToModerator.ApplicationUser.UserName)
                            .Select(categoryToModerator => categoryToModerator.ApplicationUser),
                    })
                 .ToListAsync();

            var dashboardItemsDto = dashboardItems
                 .Select(categoryModerators => new CategoryModeratorsDto
                {
                    Category = _mapper.Map<CategoryDto>(categoryModerators.Category),
                    Moderators = _mapper.Map<List<ModeratorDto>>(categoryModerators.Moderators),
                 })
                 .ToList();

            return new DashboardDto
            {
                CategoriesModerators = dashboardItemsDto,
            };
        }

        private async Task WipeDatabase()
        {
            if (new[]
            {
                "Microsoft.EntityFrameworkCore.SqlServer",
                "EntityFrameworkCore.SqlServerCompact35",
                "EntityFrameworkCore.SqlServerCompact40",
            }
            .Contains(_context.Database.ProviderName))
            {
                await _context.Database.ExecuteSqlRawAsync(
@"
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
");
            }
            else
            {
                await _context.Database.EnsureDeletedAsync();
            }
        }

        private async Task RunSeedInNewScopeAsync()
        {
            // new scope to reset EF cache
            using(var scope = _scopeFactory.CreateScope())
            {
                var applicationDbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var userMgr = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                var roleMgr = scope.ServiceProvider.GetService<RoleManager<ApplicationRole>>();
                var seedSettings = scope.ServiceProvider.GetService<IOptions<SeedConfiguration>>();

                await _context.Database.MigrateAsync();
                await DbSeeder.SeedAsync(applicationDbContext, userMgr, roleMgr, seedSettings);
            }
        }

        public async Task DeleteAllContentAsync()
        {
            _logger.LogInformation("Wiping all database tables and media files...");
            var threadIdList = await _context.Threads.Select(thread => thread.Id).ToListAsync();

            _logger.LogDebug($"Deleting content files (media) from {threadIdList.Count} threads");
            foreach (var threadId in threadIdList)
            {
                _logger.LogDebug($"Processing of {threadId} container...");
                try
                {
                    await _storageProvider.DeleteContainerAsync(threadId.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{nameof(DeleteAllContentAsync)} error");
                }
                _logger.LogDebug("OK");

                _logger.LogDebug($"Processing of {threadId + Defaults.ThumbnailPostfix} container...");
                try
                {
                    await _storageProvider.DeleteContainerAsync(threadId + Defaults.ThumbnailPostfix);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{nameof(DeleteAllContentAsync)} error");
                }
                _logger.LogDebug("OK");
            }

            _logger.LogDebug("Wiping database tables...");
            await WipeDatabase();
            _logger.LogDebug("OK");

            _logger.LogDebug("Running migrations and seed...");
            await RunSeedInNewScopeAsync();
            _logger.LogDebug("OK");
        }
    }
}
