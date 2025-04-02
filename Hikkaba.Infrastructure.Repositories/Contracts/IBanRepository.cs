using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Models.Ban.PostingRestrictions;
using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface IBanRepository
{
    Task<BanPreviewModel?> FindActiveBan(long? threadId, string? categoryAlias, string userIpAddress);
    Task<BanPreviewModel?> FindActiveBan(long? threadId, string? categoryAlias, byte[] userIpAddress);
    Task<PostingRestrictionsResponseModel> GetPostingRestrictionStatusAsync(PostingRestrictionsRequestModel restrictionsRequestModel);
    Task<PagedResult<BanDetailsModel>> ListBansPaginatedAsync(BanPagingFilter banFilter);
    Task<BanDetailsModel?> GetBanAsync(int banId);
    Task<int> CreateBanAsync(BanCreateRequestModel banCreateRequest);
    Task SetBanDeletedAsync(int banId, bool isDeleted);
}
