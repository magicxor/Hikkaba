using System.ComponentModel.DataAnnotations;
using Hikkaba.Shared.Constants;

namespace Hikkaba.Web.ViewModels.ThreadsViewModels;

public sealed class ThreadEditViewModel
{
    [Required]
    public required long Id { get; set; }

    [Required]
    [Display(Name = @"Category")]
    [MaxLength(Defaults.MaxCategoryAliasLength)]
    public required string CategoryAlias { get; set; }

    [Required]
    [MinLength(Defaults.MinTitleLength)]
    [MaxLength(Defaults.MaxTitleLength)]
    [Display(Name = @"Title")]
    public required string Title { get; set; }

    [Required]
    [Display(Name = @"Bump limit")]
    [Range(Defaults.MinBumpLimit, Defaults.MaxBumpLimit)]
    public required int BumpLimit { get; set; }
}
