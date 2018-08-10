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
using Hikkaba.Services.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwentyTwenty.Storage;

namespace Hikkaba.Services
{
    public interface IAdministrationService
    {
        DashboardDto GetDashboard();
        Task DeleteAllContentAsync();
    }

    public class AdministrationService : IAdministrationService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IOptions<SeedConfiguration> _seedConfOptions;
        private readonly IMapper _mapper;

        public AdministrationService(
            ILogger<AdministrationService> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IOptions<SeedConfiguration> seedConfOptions,
            IStorageProviderFactory storageProviderFactory,
            IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _seedConfOptions = seedConfOptions;
            _mapper = mapper;
            _storageProvider = storageProviderFactory.CreateStorageProvider();
        }

        public DashboardDto GetDashboard()
        {
            var dashboardItems = _context.Categories
                .OrderBy(category => category.Alias)
                .Select(
                    category => 
                    new { Category = category,
                          Moderators = category.Moderators
                            .OrderBy(moderator => moderator.ApplicationUser.UserName)
                            .Select(categoryToModerator => categoryToModerator.ApplicationUser),
                    })
                 .ToList()
                 .Select(categoryModerators => new CategoryModeratorsDto
                {
                    Category = _mapper.Map<CategoryDto>(categoryModerators.Category),
                    Moderators = _mapper.Map<List<ModeratorDto>>(categoryModerators.Moderators),
                 })
                 .ToList();

            return new DashboardDto
            {
                CategoriesModerators = dashboardItems
            };
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
                    _logger.LogError(ex.ToString());
                }
                _logger.LogDebug("OK");

                _logger.LogDebug($"Processing of {threadId + Defaults.ThumbnailPostfix} container...");
                try
                {
                    await _storageProvider.DeleteContainerAsync(threadId + Defaults.ThumbnailPostfix);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                _logger.LogDebug("OK");
            }

            _logger.LogDebug("Deleting all database tables...");
            await _context.Database.ExecuteSqlCommandAsync(
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
            _logger.LogDebug("OK");

            _logger.LogDebug("Running migrations and seed...");
            await _context.Database.MigrateAsync();
            _logger.LogDebug("OK");
        }
    }
}