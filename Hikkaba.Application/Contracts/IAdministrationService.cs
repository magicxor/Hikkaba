using Hikkaba.Infrastructure.Models.Administration;

namespace Hikkaba.Application.Contracts;

public interface IAdministrationService
{
    Task<DashboardModel> GetDashboardAsync(CancellationToken cancellationToken);
}
