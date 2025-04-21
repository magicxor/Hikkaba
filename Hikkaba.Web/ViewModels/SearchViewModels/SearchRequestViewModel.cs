using Hikkaba.Shared.Constants;
using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.SearchViewModels;

public sealed class SearchRequestViewModel
{
    private const string SearchQueryRequiredErrorMessage = "Enter a search query.";

    [MaxLength(Defaults.MaxCategoryAliasLength)]
    public string? CategoryAlias { get; set; }

    [Required(ErrorMessage = SearchQueryRequiredErrorMessage)]
    [RegularExpression(@".*[^\s*""].*", ErrorMessage = SearchQueryRequiredErrorMessage)]
    [MinLength(Defaults.MinSearchTermLength)]
    [MaxLength(Defaults.MaxSearchTermLength)]
    public required string Query { get; set; }
}
