namespace Hikkaba.Infrastructure.Models.Ban;

public class BanEditRequestRm
{
    public int Id { get; set; }

    public required DateTime? EndsAt { get; set; }

    public required byte[] BannedIpAddress { get; set; }

    public byte[]? BannedCidrLowerIpAddress { get; set; }

    public byte[]? BannedCidrUpperIpAddress { get; set; }

    public required string Reason { get; set; }

    public required long? RelatedPostId { get; set; }
    public required int CategoryId { get; set; }
}
