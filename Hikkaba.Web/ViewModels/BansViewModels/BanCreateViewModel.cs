using System;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Hikkaba.Shared.Constants;

namespace Hikkaba.Web.ViewModels.BansViewModels;

public class BanCreateViewModel
{
    [Display(Name = @"Start")]
    public required DateTime Start { get; set; }

    [Display(Name = @"End")]
    public required DateTime End { get; set; }

    [Display(Name = @"Lower IP address")]
    [MaxLength(Defaults.MaxIpAddressStringLength)]
    public required string LowerIpAddress { get; set; }

    [Display(Name = @"Upper IP address")]
    [MaxLength(Defaults.MaxIpAddressStringLength)]
    public required string UpperIpAddress { get; set; }

    [Display(Name = @"Reason")]
    [MaxLength(Defaults.MaxReasonLength)]
    public required string Reason { get; set; }

    public required PostDetailsViewModel RelatedPost { get; set; }
    public required CategoryDetailsViewModel Category { get; set; }
}
