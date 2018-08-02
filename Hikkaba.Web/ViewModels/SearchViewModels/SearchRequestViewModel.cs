using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.SearchViewModels
{
    public class SearchRequestViewModel
    {
        [MinLength(3)]
        [Required]
        public string Query { get; set; }
    }
}
