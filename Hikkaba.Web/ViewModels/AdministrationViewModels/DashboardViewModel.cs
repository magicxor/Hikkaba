using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Web.ViewModels.BoardViewModels;

namespace Hikkaba.Web.ViewModels.AdministrationViewModels;

public class DashboardViewModel
{
    [Display(Name = @"Board")]
    public required BoardViewModel Board { get; set; }

    [Display(Name = @"System info")]
    public required SystemInfoViewModel SystemInfo { get; set; }
}
