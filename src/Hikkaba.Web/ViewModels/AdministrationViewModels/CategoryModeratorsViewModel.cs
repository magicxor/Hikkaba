using System.Collections.Generic;

namespace Hikkaba.Web.ViewModels.AdministrationViewModels
{
    public class CategoryModeratorsViewModel
    {
        public string CategoryAlias { get; set; }
        public IList<ApplicationUserViewModel> Moderators { get; set; }
    }
}
