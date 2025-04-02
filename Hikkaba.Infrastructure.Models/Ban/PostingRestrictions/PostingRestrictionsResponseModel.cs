using Hikkaba.Shared.Enums;

namespace Hikkaba.Infrastructure.Models.Ban;

public abstract class PostingRestrictionsResponseModel
{
    public required PostingRestrictionType RestrictionType { get; set; }
}
