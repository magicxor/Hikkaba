using System.ComponentModel.DataAnnotations;
using Hikkaba.Services.Base.Generic;
using Hikkaba.Web.ViewModels.ThreadsViewModels;

namespace Hikkaba.Web.ViewModels.CategoriesViewModels
{
    public class CategoryDetailsViewModel
    {
        [Display(Name = @"Category")]
        public CategoryViewModel Category { get; set; }

        [Display(Name = @"Threads")]
        public BasePagedList<ThreadDetailsViewModel> Threads { get; set; }
    }
}
