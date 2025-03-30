using System.Globalization;
using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Constants;
using Microsoft.Extensions.Logging;
using TwentyTwenty.Storage;

namespace Hikkaba.Application.Implementations;

public class AdministrationService : IAdministrationService
{
    private readonly ILogger<AdministrationService> _logger;
    private readonly IStorageProvider _storageProvider;
    private readonly IAdministrationRepository _administrationRepository;
    private readonly IThreadRepository _threadRepository;

    public AdministrationService(
        ILogger<AdministrationService> logger,
        IStorageProvider storageProvider,
        IAdministrationRepository administrationRepository,
        IThreadRepository threadRepository)
    {
        _logger = logger;
        _storageProvider = storageProvider;
        _administrationRepository = administrationRepository;
        _threadRepository = threadRepository;
    }

    public async Task<DashboardModel> GetDashboardAsync()
    {
        return await _administrationRepository.GetDashboardAsync();
    }

    public async Task DeleteAllContentAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Wiping all database tables and media files...");
        var threadIdList = await _threadRepository.ListAllThreadIdsAsync(cancellationToken);

        _logger.LogDebug("Deleting content files (media) from {Count} threads", threadIdList.Count);
        foreach (var threadId in threadIdList)
        {
            var threadIdStr = threadId.ToString(CultureInfo.InvariantCulture);

            _logger.LogDebug("Deleting {ThreadId}...", threadIdStr);
            try
            {
                await _storageProvider.DeleteContainerAsync(threadIdStr);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(DeleteAllContentAsync)} error");
            }
            _logger.LogDebug("OK");

            _logger.LogDebug("Deleting {ThumbnailPostfix}...", threadIdStr + Defaults.ThumbnailPostfix);
            try
            {
                await _storageProvider.DeleteContainerAsync(threadIdStr + Defaults.ThumbnailPostfix);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(DeleteAllContentAsync)} error");
            }
            _logger.LogDebug("OK");
        }

        _logger.LogDebug("Wiping database tables...");
        await _administrationRepository.WipeDatabaseAsync();
        _logger.LogDebug("OK");

        _logger.LogDebug("Running migrations and seed...");
        await _administrationRepository.RunSeedInNewScopeAsync();
        _logger.LogDebug("OK");
    }
}
