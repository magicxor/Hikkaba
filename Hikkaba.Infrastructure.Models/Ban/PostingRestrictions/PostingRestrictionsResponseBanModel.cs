namespace Hikkaba.Infrastructure.Models.Ban;

public class PostingRestrictionsResponseBanModel : PostingRestrictionsResponseModel
{
    public required string RestrictionReason { get; set; }
    public required DateTime? RestrictionEndsAt { get; set; }
}
