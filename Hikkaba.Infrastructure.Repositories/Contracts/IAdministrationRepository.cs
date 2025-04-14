using Hikkaba.Infrastructure.Models.Administration;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface IAdministrationRepository
{
    Task<DashboardModel> GetDashboardAsync(CancellationToken cancellationToken);
}
