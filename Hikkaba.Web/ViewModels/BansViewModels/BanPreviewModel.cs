using System;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Hikkaba.Common.Constants;

namespace Hikkaba.Web.ViewModels.BansViewModels;

public class BanPreviewModel
{
    [Display(Name = @"Id")]
    public required int Id { get; set; }

    [Display(Name = @"End")]
    [DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm")]
    public required DateTime? EndsAt { get; set; }

    [Display(Name = @"Reason")]
    [MaxLength(Defaults.MaxReasonLength)]
    public required string Reason { get; set; }
}
