namespace Hikkaba.Infrastructure.Models.Ban.PostingRestrictions;

public sealed class PostingRestrictionsRequestModel
{
    public required string CategoryAlias { get; set; }

    public required long? ThreadId { get; set; }

    public required byte[]? UserIpAddress { get; set; }
}
