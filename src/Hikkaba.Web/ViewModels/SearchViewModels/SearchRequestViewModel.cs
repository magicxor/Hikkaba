using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hikkaba.Web.ViewModels.SearchViewModels
{
    public class SearchRequestViewModel
    {
        [MinLength(3)]
        [Required]
        public string Query { get; set; }
    }
}
