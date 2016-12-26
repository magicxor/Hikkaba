using System;
using System.Threading.Tasks;
using Hikkaba.Common.Configuration;
using Hikkaba.Common.Constants;
using Hikkaba.Common.Data;
using Hikkaba.Common.Entities;
using Hikkaba.Common.Storage;
using Hikkaba.Common.Storage.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwentyTwenty.Storage;

namespace Hikkaba.Service
{
    public interface IAdministrationService
    {
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
        private readonly IThreadService _threadService;

        public AdministrationService(
            ILogger<AdministrationService> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IOptions<SeedConfiguration> seedConfOptions,
            IStorageProviderFactory storageProviderFactory,
            IThreadService threadService)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _seedConfOptions = seedConfOptions;
            _storageProvider = storageProviderFactory.CreateStorageProvider();
            _threadService = threadService;
        }

        public async Task DeleteAllContentAsync()
        {
            _logger.LogInformation("Wiping all database tables and media files...");
            var threadsDtoList = await _threadService.ListAsync();
            
            _logger.LogDebug($"Deleting content files (media) from {threadsDtoList.Count} threads");
            foreach (var threadDto in threadsDtoList)
            {
                _logger.LogDebug($"Processing of {threadDto.Id} container...");
                try
                {
                    await _storageProvider.DeleteContainerAsync(threadDto.Id.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                _logger.LogDebug("OK");

                _logger.LogDebug($"Processing of {threadDto.Id + Defaults.ThumbnailPostfix} container...");
                try
                {
                    await _storageProvider.DeleteContainerAsync(threadDto.Id + Defaults.ThumbnailPostfix);
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

            _logger.LogDebug("Running migrations...");
            await _context.Database.MigrateAsync();
            _logger.LogDebug("OK");

            _logger.LogDebug("Seeding...");
            await DbSeeder.SeedAsync(_context, _userManager, _roleManager, _seedConfOptions);
            _logger.LogDebug("OK");
        }
    }
}