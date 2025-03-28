using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Paging.Models;

namespace Hikkaba.Repositories.Contracts;

public interface IBanRepository
{
    Task<BanPreviewRm?> FindActiveBan(long? threadId, string? categoryAlias, string userIpAddress);
    Task<BanPreviewRm?> FindActiveBan(long? threadId, string? categoryAlias, byte[] userIpAddress);
    Task<PostingRestrictionStatusRm> GetPostingRestrictionStatusAsync(PostingRestrictionStatusRequestRm restrictionStatusRequestRm);
    Task<PagedResult<BanViewRm>> ListBansPaginatedAsync(BanPagingFilter banFilter);
    Task<BanViewRm?> GetBanAsync(int banId);
    Task<int> CreateBanAsync(BanCreateRequestRm banCreateRequest);
    Task SetBanDeletedAsync(int banId, bool isDeleted);
}
