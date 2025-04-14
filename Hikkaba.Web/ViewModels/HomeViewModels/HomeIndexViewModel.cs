using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Paging.Models;

using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Hikkaba.Web.ViewModels.PostsViewModels;

namespace Hikkaba.Web.ViewModels.HomeViewModels;

public sealed class HomeIndexViewModel
{
    [Display(Name = @"Categories")]
    public required IReadOnlyList<CategoryDetailsViewModel> Categories { get; set; }

    [Display(Name = @"Latest posts")]
    public required PagedResult<PostDetailsViewModel> Posts { get; set; }
}
