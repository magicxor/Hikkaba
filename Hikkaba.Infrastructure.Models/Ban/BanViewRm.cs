using Hikkaba.Common.Enums;

namespace Hikkaba.Infrastructure.Models.Ban;

public class BanViewRm
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public required DateTime? EndsAt { get; set; }
    public required IpAddressType IpAddressType { get; set; }
    public required byte[] BannedIpAddress { get; set; }
    public byte[]? BannedCidrLowerIpAddress { get; set; }
    public byte[]? BannedCidrUpperIpAddress { get; set; }
    public string? CountryIsoCode { get; set; }
    public long? AutonomousSystemNumber { get; set; }
    public string? AutonomousSystemOrganization { get; set; }
    public required string Reason { get; set; }
    public string? CategoryAlias { get; set; }
    public long? RelatedThreadId { get; set; }
    public long? RelatedPostId { get; set; }
    public int? CategoryId { get; set; }
    public int CreatedById { get; set; }
    public int? ModifiedById { get; set; }
}
