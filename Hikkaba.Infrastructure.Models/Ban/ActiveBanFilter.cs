namespace Hikkaba.Infrastructure.Models.Ban;

public sealed class ActiveBanFilter
{
    public byte[]? UserIpAddress { get; set; }
    public string? CategoryAlias { get; set; }
    public long? ThreadId { get; set; }
}
