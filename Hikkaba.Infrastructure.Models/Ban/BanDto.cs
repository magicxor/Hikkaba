using Hikkaba.Common.Enums;
using Hikkaba.Infrastructure.Models.ApplicationUser;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Infrastructure.Models.Post;

namespace Hikkaba.Infrastructure.Models.Ban;

public class BanDto
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

    public long? RelatedPostId { get; set; }

    public int? CategoryId { get; set; }

    public required ApplicationUserPreviewRm CreatedBy { get; set; }

    public required ApplicationUserPreviewRm ModifiedBy { get; set; }

    public required PostDto RelatedPost { get; set; }
    public required CategoryDto Category { get; set; }
}
