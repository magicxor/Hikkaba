using System.ComponentModel.DataAnnotations;
using Hikkaba.Paging.Models;

namespace Hikkaba.Web.ViewModels.BansViewModels;

public sealed class BanIndexViewModel
{
    [Display(Name = @"Bans")]
    public required PagedResult<BanViewModel> Bans { get; set; }
}
