using System.ComponentModel.DataAnnotations;
using Hikkaba.Services.Implementations.Generic;
using Hikkaba.Web.ViewModels.ThreadsViewModels;

namespace Hikkaba.Web.ViewModels.CategoriesViewModels;

public class CategoryThreadsViewModel
{
    [Display(Name = @"Category")]
    public CategoryDetailsViewModel Category { get; set; }

    [Display(Name = @"Threads")]
    public BasePagedList<ThreadDetailsViewModel> Threads { get; set; }
}