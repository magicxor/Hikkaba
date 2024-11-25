using System.Threading.Tasks;
using Hikkaba.Models.Dto.Administration;

namespace Hikkaba.Services.Contracts;

public interface IAdministrationService
{
    Task<DashboardDto> GetDashboardAsync();
    Task DeleteAllContentAsync();
}