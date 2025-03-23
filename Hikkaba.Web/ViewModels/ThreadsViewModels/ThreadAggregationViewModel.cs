using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Web.ViewModels.BoardViewModels;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Hikkaba.Web.ViewModels.PostsViewModels;

namespace Hikkaba.Web.ViewModels.ThreadsViewModels;

public class ThreadAggregationViewModel
{
    [Display(Name = @"Thread")]
    public required ThreadDetailsViewModel Thread { get; set; }

    [Display(Name = @"Category")]
    public required CategoryDetailsViewModel Category { get; set; }

    [Display(Name = @"Board")]
    public required BoardViewModel Board { get; set; }

    [Display(Name = @"Posts")]
    public required IReadOnlyList<PostDetailsViewModel> Posts { get; set; }
}
