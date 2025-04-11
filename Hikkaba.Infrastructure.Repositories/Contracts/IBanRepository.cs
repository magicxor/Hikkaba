using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Models.Ban.PostingRestrictions;
using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface IBanRepository
{
    Task<BanPreviewModel?> FindActiveBanAsync(ActiveBanFilter filter, CancellationToken cancellationToken);
    Task<PostingRestrictionsResponseModel> GetPostingRestrictionStatusAsync(PostingRestrictionsRequestModel restrictionsRequestModel, CancellationToken cancellationToken);
    Task<PagedResult<BanDetailsModel>> ListBansPaginatedAsync(BanPagingFilter banFilter, CancellationToken cancellationToken);
    Task<BanDetailsModel?> GetBanAsync(int banId, CancellationToken cancellationToken);
    Task<BanCreateResultModel> CreateBanAsync(BanCreateRequestModel banCreateRequest, CancellationToken cancellationToken);
    Task SetBanDeletedAsync(int banId, bool isDeleted, CancellationToken cancellationToken);
}
