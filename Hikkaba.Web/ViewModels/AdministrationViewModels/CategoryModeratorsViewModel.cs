using System.Collections.Generic;
using Hikkaba.Web.ViewModels.CategoriesViewModels;

namespace Hikkaba.Web.ViewModels.AdministrationViewModels
{
    public class CategoryModeratorsViewModel
    {
        public CategoryViewModel Category { get; set; }
        public IList<ApplicationUserViewModel> Moderators { get; set; }
    }
}
