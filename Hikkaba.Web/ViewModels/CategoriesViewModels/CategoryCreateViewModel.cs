using System.ComponentModel.DataAnnotations;
using Hikkaba.Shared.Constants;

namespace Hikkaba.Web.ViewModels.CategoriesViewModels;

public class CategoryCreateViewModel
{
    [Required]
    [RegularExpression("^[A-Za-z]+$")]
    [MinLength(Defaults.MinCategoryAliasLength)]
    [MaxLength(Defaults.MaxCategoryAliasLength)]
    [Display(Name = @"Alias")]
    public required string Alias { get; set; }

    [Required]
    [Display(Name = @"Name")]
    [MaxLength(Defaults.MaxCategoryAndBoardNameLength)]
    public required string Name { get; set; }

    [Required]
    [Display(Name = @"Is hidden")]
    public required bool IsHidden { get; set; }

    [Required]
    [Display(Name = @"Default bump limit")]
    [Range(10, 10000)]
    public required int DefaultBumpLimit { get; set; } = Defaults.MinBumpLimit;

    [Required]
    [Display(Name = @"Show thread-local user hash by default")]
    public required bool ShowThreadLocalUserHash { get; set; }

    [Required]
    [Display(Name = @"Show country")]
    public required bool ShowCountry { get; set; }

    [Required]
    [Display(Name = @"Show OS")]
    public required bool ShowOs { get; init; }

    [Required]
    [Display(Name = @"Show browser")]
    public required bool ShowBrowser { get; init; }

    [Required]
    [Display(Name = @"Max thread count")]
    [Range(10, 10000)]
    public required int MaxThreadCount { get; set; }
}
