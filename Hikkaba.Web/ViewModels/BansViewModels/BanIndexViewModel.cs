using System.ComponentModel.DataAnnotations;
using Hikkaba.Services.Base.Generic;

namespace Hikkaba.Web.ViewModels.BansViewModels
{
    public class BanIndexViewModel
    {
        [Display(Name = @"Bans")]
        public BasePagedList<BanDetailsViewModel> Bans { get; set; }
    }
}