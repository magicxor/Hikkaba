using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Paging.Models;

namespace Hikkaba.Application.Contracts;

public interface IBanService
{
    Task<BanPreviewModel?> FindActiveBan(long? threadId, string? categoryAlias, string userIpAddress);
    Task<PagedResult<BanDetailsModel>> ListBansPaginatedAsync(BanPagingFilter banFilter);
    Task<BanDetailsModel?> GetBanAsync(int banId);
    Task<int> CreateBanAsync(BanCreateCommand banCreateCommand);
    Task SetBanDeletedAsync(int banId, bool isDeleted);
}
