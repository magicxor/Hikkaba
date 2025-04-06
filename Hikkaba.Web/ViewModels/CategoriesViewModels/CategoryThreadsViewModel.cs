using System.ComponentModel.DataAnnotations;
using Hikkaba.Paging.Models;

using Hikkaba.Web.ViewModels.ThreadsViewModels;

namespace Hikkaba.Web.ViewModels.CategoriesViewModels;

public class CategoryThreadsViewModel
{
    [Display(Name = @"Category")]
    public required CategoryDetailsViewModel Category { get; set; }

    [Display(Name = @"Threads")]
    public required PagedResult<ThreadDetailsViewModel> Threads { get; set; }
}
