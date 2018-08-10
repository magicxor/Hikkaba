using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Services.Base.Generic;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Hikkaba.Web.ViewModels.PostsViewModels;

namespace Hikkaba.Web.ViewModels.HomeViewModels
{
    public class HomeIndexViewModel
    {
        [Display(Name = @"Categories")]
        public IList<CategoryViewModel> Categories { get; set; }

        [Display(Name = @"Latest posts")]
        public BasePagedList<PostDetailsViewModel> Posts { get; set; }
    }
}
