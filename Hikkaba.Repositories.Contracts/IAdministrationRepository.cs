using Hikkaba.Infrastructure.Models.Administration;

namespace Hikkaba.Repositories.Contracts;

public interface IAdministrationRepository
{
    Task<DashboardRm> GetDashboardAsync();
    Task WipeDatabaseAsync();
    Task RunSeedInNewScopeAsync();
}
