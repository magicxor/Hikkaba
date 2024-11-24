using Hikkaba.Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.SearchViewModels;

public class SearchRequestViewModel
{
    [Required]
    [MinLength(Defaults.MinSearchTermLength)]
    [MaxLength(Defaults.MaxSearchTermLength)]
    public string Query { get; set; }
}