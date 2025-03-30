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

    [Display(Name = @"Name")]
    public required string Name { get; set; }

    [Display(Name = @"Is hidden")]
    public required bool IsHidden { get; set; }

    [Display(Name = @"Default bump limit")]
    public required int DefaultBumpLimit { get; set; } = Defaults.MinBumpLimit;

    [Display(Name = @"Show thread-local user hash by default")]
    public required bool DefaultShowThreadLocalUserHash { get; set; }
}
