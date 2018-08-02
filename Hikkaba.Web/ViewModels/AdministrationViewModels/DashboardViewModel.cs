using System.Collections.Generic;
using Hikkaba.Web.ViewModels.BoardViewModels;

namespace Hikkaba.Web.ViewModels.AdministrationViewModels
{
    public class DashboardViewModel
    {
        public BoardViewModel Board { get; set; }
        public IList<CategoryModeratorsViewModel> CategoriesModerators { get; set; }
    }
}
