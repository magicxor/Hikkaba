using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Common.Dto;
using Hikkaba.Service.Base;
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
