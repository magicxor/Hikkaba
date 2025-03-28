namespace Hikkaba.Infrastructure.Models.Ban;

public class PostingRestrictionStatusRequestRm
{
    public string? CategoryAlias { get; set; }

    public long? ThreadId { get; set; }

    public byte[]? UserIpAddress { get; set; }
}
