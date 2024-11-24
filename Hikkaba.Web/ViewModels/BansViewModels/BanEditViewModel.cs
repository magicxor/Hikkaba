using TPrimaryKey = System.Guid;
using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.BansViewModels;

public class BanEditViewModel: BanCreateViewModel
{
    [Display(Name = @"Id")]
    public TPrimaryKey Id { get; set; }
}