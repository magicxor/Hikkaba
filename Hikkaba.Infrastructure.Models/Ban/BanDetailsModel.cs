using Hikkaba.Shared.Enums;

namespace Hikkaba.Infrastructure.Models.Ban;

public class BanDetailsModel
{
    public required int Id { get; set; }
    public required bool IsDeleted { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime? ModifiedAt { get; set; }
    public required DateTime? EndsAt { get; set; }
    public required IpAddressType IpAddressType { get; set; }
    public required byte[] BannedIpAddress { get; set; }
    public required byte[]? BannedCidrLowerIpAddress { get; set; }
    public required byte[]? BannedCidrUpperIpAddress { get; set; }
    public required string? CountryIsoCode { get; set; }
    public required long? AutonomousSystemNumber { get; set; }
    public required string? AutonomousSystemOrganization { get; set; }
    public required string Reason { get; set; }
    public required string? CategoryAlias { get; set; }
    public required long? RelatedThreadId { get; set; }
    public required long? RelatedPostId { get; set; }
    public required int? CategoryId { get; set; }
    public required int CreatedById { get; set; }
    public required int? ModifiedById { get; set; }
}
