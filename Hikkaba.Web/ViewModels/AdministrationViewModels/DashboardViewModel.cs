using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.AdministrationViewModels;

public sealed class DashboardViewModel
{
    [Display(Name = @"System info")]
    public required SystemInfoViewModel SystemInfo { get; set; }
}
