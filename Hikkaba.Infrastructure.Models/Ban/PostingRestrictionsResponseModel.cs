using Hikkaba.Shared.Enums;

namespace Hikkaba.Infrastructure.Models.Ban;

public class PostingRestrictionsResponseModel
{
    public required PostingRestrictionType RestrictionType { get; set; }
    public string? RestrictionReason { get; set; }
    public DateTime? RestrictionEndsAt { get; set; }
}
