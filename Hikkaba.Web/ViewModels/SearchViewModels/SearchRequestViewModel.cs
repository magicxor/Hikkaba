using Hikkaba.Shared.Constants;
using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.SearchViewModels;

public class SearchRequestViewModel
{
    [Required]
    [MinLength(Defaults.MinSearchTermLength)]
    [MaxLength(Defaults.MaxSearchTermLength)]
    public required string Query { get; set; }
}
