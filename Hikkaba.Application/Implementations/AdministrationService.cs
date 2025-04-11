using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Infrastructure.Repositories.Contracts;

namespace Hikkaba.Application.Implementations;

public sealed class AdministrationService : IAdministrationService
{
    private readonly IAdministrationRepository _administrationRepository;

    public AdministrationService(
        IAdministrationRepository administrationRepository)
    {
        _administrationRepository = administrationRepository;
    }

    public async Task<DashboardModel> GetDashboardAsync(CancellationToken cancellationToken)
    {
        return await _administrationRepository.GetDashboardAsync(cancellationToken);
    }
}
