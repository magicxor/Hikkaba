using Hikkaba.Infrastructure.Models.Post;

namespace Hikkaba.Infrastructure.Models.Ban;

public class BanCreationPrerequisitesModel
{
    public PostDetailsModel? Post { get; init; }
    public IpAddressInfoModel? IpAddressInfo { get; init; }
    public int? ActiveBanId { get; init; }
    public required BanCreationPrerequisiteStatus Status { get; init; }
}
