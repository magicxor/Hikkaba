using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Models.Ban.PostingRestrictions;
using Hikkaba.Paging.Models;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface IBanRepository
{
    Task<BanSlimModel?> FindActiveBanAsync(ActiveBanFilter filter, CancellationToken cancellationToken);
    Task<PostingRestrictionsResponseModel> GetPostingRestrictionStatusAsync(PostingRestrictionsRequestModel requestModel, CancellationToken cancellationToken);
    Task<PagedResult<BanDetailsModel>> ListBansPaginatedAsync(BanPagingFilter filter, CancellationToken cancellationToken);
    Task<BanDetailsModel?> GetBanAsync(int banId, CancellationToken cancellationToken);
    Task<BanCreateResultModel> CreateBanAsync(BanCreateRequestModel requestModel, CancellationToken cancellationToken);
    Task SetBanDeletedAsync(int banId, bool isDeleted, CancellationToken cancellationToken);
}
