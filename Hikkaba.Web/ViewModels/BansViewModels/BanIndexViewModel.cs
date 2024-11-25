using System.ComponentModel.DataAnnotations;
using Hikkaba.Services.Implementations.Generic;

namespace Hikkaba.Web.ViewModels.BansViewModels;

public class BanIndexViewModel
{
    [Display(Name = @"Bans")]
    public BasePagedList<BanDetailsViewModel> Bans { get; set; }
}