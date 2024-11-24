using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Web.ViewModels.BoardViewModels;

namespace Hikkaba.Web.ViewModels.AdministrationViewModels;

public class DashboardViewModel
{
    [Display(Name = @"Board")]
    public BoardViewModel Board { get; set; }
        
    [Display(Name = @"Moderators")]
    public IList<CategoryModeratorsViewModel> CategoriesModerators { get; set; }

    [Display(Name = @"System info")]
    public SystemInfoViewModel SystemInfo { get; set; }
}