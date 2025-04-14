using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.BansViewModels;

public sealed class BanEditViewModel : BanCreateViewModel
{
    [Display(Name = @"Id")]
    public int Id { get; set; }
}
