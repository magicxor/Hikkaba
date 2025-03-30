namespace Hikkaba.Infrastructure.Models.Ban;

public class PostingRestrictionsRequestModel
{
    public string? CategoryAlias { get; set; }

    public long? ThreadId { get; set; }

    public byte[]? UserIpAddress { get; set; }
}
