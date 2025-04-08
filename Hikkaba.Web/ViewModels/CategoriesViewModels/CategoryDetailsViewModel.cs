using System;
using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.CategoriesViewModels;

public class CategoryDetailsViewModel
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

    [Display(Name = @"Show thread-local user hash")]
    public required bool ShowThreadLocalUserHash { get; set; }

    [Display(Name = @"Show user country")]
    public required bool ShowCountry { get; set; }

    [Display(Name = @"Show user OS")]
    public required bool ShowOs { get; init; }

    [Display(Name = @"Show user browser")]
    public required bool ShowBrowser { get; init; }

    [Display(Name = @"Max thread count in category")]
    public required int MaxThreadCount { get; set; }

    [Display(Name = @"Board id")]
    public required int BoardId { get; set; }
}
