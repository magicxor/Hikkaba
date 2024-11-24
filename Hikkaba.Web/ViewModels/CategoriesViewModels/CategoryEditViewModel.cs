using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.CategoriesViewModels;

public class CategoryEditViewModel: CategoryCreateViewModel
{
    [Display(Name = @"Id")]
    public TPrimaryKey Id { get; set; }
}
