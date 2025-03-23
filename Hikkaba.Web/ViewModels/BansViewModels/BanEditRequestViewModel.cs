using System;

namespace Hikkaba.Web.ViewModels.BansViewModels;

#nullable enable

public class BanEditRequestViewModel
{
    public int Id { get; set; }

    public required DateTime? EndsAt { get; set; }

    public required string BannedIpAddress { get; set; }

    public string? BannedCidrLowerIpAddress { get; set; }

    public string? BannedCidrUpperIpAddress { get; set; }

    public required string Reason { get; set; }

    public required long? RelatedPostId { get; set; }
    public required int CategoryId { get; set; }
}
