using Hikkaba.Shared.Enums;

namespace Hikkaba.Infrastructure.Models.Ban;

public class BanCreateRequestModel
{
    public required DateTime? EndsAt { get; set; }

    public required IpAddressType IpAddressType { get; set; }

    public required byte[] BannedIpAddress { get; set; }

    public byte[]? BannedCidrLowerIpAddress { get; set; }

    public byte[]? BannedCidrUpperIpAddress { get; set; }

    public string? CountryIsoCode { get; set; }

    public long? AutonomousSystemNumber { get; set; }

    public string? AutonomousSystemOrganization { get; set; }

    public required string Reason { get; set; }

    public long? RelatedPostId { get; set; }

    public int? CategoryId { get; set; }
}
