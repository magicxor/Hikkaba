using Hikkaba.Common.Enums;

namespace Hikkaba.Infrastructure.Models.Ban;

public class PostingRestrictionStatusRm
{
    public required PostingRestrictionType RestrictionType { get; set; }
    public string? RestrictionReason { get; set; }
    public DateTime? RestrictionEndsAt { get; set; }
}
