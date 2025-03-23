using System.Threading.Tasks;
using Hikkaba.Infrastructure.Models.Administration;

namespace Hikkaba.Services.Contracts;

public interface IAdministrationService
{
    Task<DashboardRm> GetDashboardAsync();
    Task DeleteAllContentAsync(CancellationToken cancellationToken);
}
