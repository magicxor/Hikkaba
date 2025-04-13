using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Paging.Models;

namespace Hikkaba.Application.Contracts;

public interface IBanService
{
    Task<BanSlimModel?> FindActiveBan(ActiveBanFilter filter, CancellationToken cancellationToken);
    Task<PagedResult<BanDetailsModel>> ListBansPaginatedAsync(BanPagingFilter filter, CancellationToken cancellationToken);
    Task<BanDetailsModel?> GetBanAsync(int banId, CancellationToken cancellationToken);
    Task<BanCreateResultModel> CreateBanAsync(BanCreateCommand banCreateCommand, CancellationToken cancellationToken);
    Task SetBanDeletedAsync(int banId, bool isDeleted, CancellationToken cancellationToken);
}
