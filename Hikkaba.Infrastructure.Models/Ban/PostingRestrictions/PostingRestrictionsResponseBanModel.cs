﻿namespace Hikkaba.Infrastructure.Models.Ban.PostingRestrictions;

public sealed class PostingRestrictionsResponseBanModel : PostingRestrictionsResponseModel
{
    public required string RestrictionReason { get; set; }
    public required DateTime? RestrictionEndsAt { get; set; }
}
