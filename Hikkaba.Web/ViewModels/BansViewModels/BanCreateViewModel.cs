using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Hikkaba.Shared.Constants;

namespace Hikkaba.Web.ViewModels.BansViewModels;

public class BanCreateViewModel
{
    [Display(Name = @"End date and time")]
    public required DateTime? EndsAt { get; set; }

    [Display(Name = @"Ban whole network (IP address range)")]
    public required bool BanByNetwork { get; set; }

    [Display(Name = @"Ban in all categories")]
    public required bool BanInAllCategories { get; set; }

    [Display(Name = @"Reason")]
    [MaxLength(Defaults.MaxReasonLength)]
    public required string Reason { get; set; }

    public required string BannedIpAddress { get; set; }

    public required string CategoryAlias { get; set; }

    public required long RelatedThreadId { get; set; }

    public required long? RelatedPostId { get; set; }
}
