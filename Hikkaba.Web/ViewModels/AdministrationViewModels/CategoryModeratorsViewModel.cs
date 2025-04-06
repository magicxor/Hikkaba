using System.Collections.Generic;
using Hikkaba.Web.ViewModels.CategoriesViewModels;

namespace Hikkaba.Web.ViewModels.AdministrationViewModels;

public class CategoryModeratorsViewModel
{
    public required CategoryDetailsViewModel Category { get; set; }
    public required IReadOnlyList<ApplicationUserViewModel> Moderators { get; set; }
}
