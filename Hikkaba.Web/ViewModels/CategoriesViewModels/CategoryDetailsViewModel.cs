using System;
using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.CategoriesViewModels;

public sealed class CategoryDetailsViewModel
{
    [Display(Name = @"Id")]
    public required int Id { get; set; }

    [Display(Name = @"Is deleted")]
    public required bool IsDeleted { get; set; }

    [Display(Name = @"Created at")]
    public required DateTime CreatedAt { get; set; }

    [Display(Name = @"Modified at")]
    public required DateTime? ModifiedAt { get; set; }

    [Display(Name = @"Alias")]
    public required string Alias { get; set; }

    [Display(Name = @"Name")]
    public required string Name { get; set; }

    [Display(Name = @"Is hidden")]
    public required bool IsHidden { get; set; }

    [Display(Name = @"Default bump limit")]
    public required int DefaultBumpLimit { get; set; }

    [Display(Name = @"Show hash")]
    public required bool ShowThreadLocalUserHash { get; set; }

    [Display(Name = @"Show country")]
    public required bool ShowCountry { get; set; }

    [Display(Name = @"Show OS")]
    public required bool ShowOs { get; init; }

    [Display(Name = @"Show browser")]
    public required bool ShowBrowser { get; init; }

    [Display(Name = @"Max thread count")]
    public required int MaxThreadCount { get; set; }
}
