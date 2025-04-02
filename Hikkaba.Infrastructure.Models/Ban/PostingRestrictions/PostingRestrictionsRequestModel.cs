namespace Hikkaba.Infrastructure.Models.Ban.PostingRestrictions;

public class PostingRestrictionsRequestModel
{
    public required string CategoryAlias { get; set; }

    public required long? ThreadId { get; set; }

    public required byte[]? UserIpAddress { get; set; }
}
