using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hikkaba.Web.ViewModels.ManageViewModels
{
    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }

        public ICollection<SelectListItem> Providers { get; set; }
    }
}
