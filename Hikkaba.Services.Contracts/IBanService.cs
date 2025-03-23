using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Paging.Models;

namespace Hikkaba.Services.Contracts;

public interface IBanService
{
    Task<BanPreviewRm?> FindActiveBan(long? threadId, string? categoryAlias, string userIpAddress);
    Task<PagedResult<BanRm>> ListBansPaginatedAsync(BanPagingFilter banFilter);
    Task<BanRm?> GetBanAsync(int banId);
    Task<int> CreateBanAsync(BanCreateRequestSm banCreateRequestSm);
    Task SetBanDeletedAsync(int banId, bool isDeleted);
}
