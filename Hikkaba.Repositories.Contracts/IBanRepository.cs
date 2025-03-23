using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Paging.Models;

namespace Hikkaba.Repositories.Contracts;

public interface IBanRepository
{
    Task<BanPreviewRm?> FindActiveBan(long? threadId, string? categoryAlias, string userIpAddress);
    Task<PagedResult<BanRm>> ListBansPaginatedAsync(BanPagingFilter banFilter);
    Task<BanRm?> GetBanAsync(int banId);
    Task<int> CreateBanAsync(BanCreateRequestRm banCreateRequest);
    Task SetBanDeletedAsync(int banId, bool isDeleted);
}
