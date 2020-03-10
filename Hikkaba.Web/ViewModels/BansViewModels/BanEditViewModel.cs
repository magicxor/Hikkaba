using TPrimaryKey = System.Guid;
using System;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Hikkaba.Common.Constants;

namespace Hikkaba.Web.ViewModels.BansViewModels
{
    public class BanEditViewModel
    {
        [Display(Name = @"Id")]
        public TPrimaryKey Id { get; set; }

        [Display(Name = @"Start")]
        public DateTime Start { get; set; }

        [Display(Name = @"End")]
        public DateTime End { get; set; }

        [Display(Name = @"Lower IP address")]
        [MaxLength(Defaults.MaxIpAddressLength)]
        public string LowerIpAddress { get; set; }

        [Display(Name = @"Upper IP address")]
        [MaxLength(Defaults.MaxIpAddressLength)]
        public string UpperIpAddress { get; set; }

        [Display(Name = @"Reason")]
        [MaxLength(Defaults.MaxReasonLength)]
        public string Reason { get; set; }

        public PostDetailsViewModel RelatedPost { get; set; }
        public CategoryViewModel Category { get; set; }
    }
}
