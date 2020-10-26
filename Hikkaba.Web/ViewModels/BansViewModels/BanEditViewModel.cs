using TPrimaryKey = System.Guid;
using System;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Hikkaba.Common.Constants;

namespace Hikkaba.Web.ViewModels.BansViewModels
{
    public class BanEditViewModel: BanCreateViewModel
    {
        [Display(Name = @"Id")]
        public TPrimaryKey Id { get; set; }
    }
}
