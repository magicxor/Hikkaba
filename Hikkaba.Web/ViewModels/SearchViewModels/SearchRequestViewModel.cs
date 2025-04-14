using Hikkaba.Shared.Constants;
using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.SearchViewModels;

public sealed class SearchRequestViewModel
{
    [MaxLength(Defaults.MaxCategoryAliasLength)]
    public string? CategoryAlias { get; set; }

    [Required]
    [MinLength(Defaults.MinSearchTermLength)]
    [MaxLength(Defaults.MaxSearchTermLength)]
    public required string Query { get; set; }
}
