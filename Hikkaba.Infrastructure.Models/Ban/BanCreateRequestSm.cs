namespace Hikkaba.Infrastructure.Models.Ban;

public class BanCreateRequestSm
{
    public required DateTime? EndsAt { get; set; }

    public required string BannedIpAddress { get; set; }

    public required bool BanByNetwork { get; set; }

    public required string Reason { get; set; }

    public required long? RelatedPostId { get; set; }

    public required int CategoryId { get; set; }
}
