using System.Globalization;
using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Constants;
using Microsoft.Extensions.Logging;
using TwentyTwenty.Storage;

namespace Hikkaba.Application.Implementations;

public sealed class AdministrationService : IAdministrationService
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

    public async Task<DashboardModel> GetDashboardAsync(CancellationToken cancellationToken)
    {
        return await _administrationRepository.GetDashboardAsync(cancellationToken);
    }
}
