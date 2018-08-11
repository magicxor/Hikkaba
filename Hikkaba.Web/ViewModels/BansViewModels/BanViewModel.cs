using System;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Hikkaba.Web.ViewModels.PostsViewModels;
using TPrimaryKey = System.Guid;

namespace Hikkaba.Web.ViewModels.BansViewModels
{
    public class BanViewModel
    {
        [Display(Name = @"Id")]
        public TPrimaryKey? Id { get; set; }

        [Display(Name = @"Is deleted")]
        public bool IsDeleted { get; set; }

        [Display(Name = @"Creation date and time")]
        public DateTime Created { get; set; }

        [Display(Name = @"Modification date and time")]
        public DateTime? Modified { get; set; }

        [Display(Name = @"Start")]
        public DateTime Start { get; set; }

        [Display(Name = @"End")]
        public DateTime End { get; set; }

        [Display(Name = @"Lower IP address")]
        public string LowerIpAddress { get; set; }

        [Display(Name = @"Upper IP address")]
        public string UpperIpAddress { get; set; }

        [Display(Name = @"Reason")]
        public string Reason { get; set; }

        public PostDetailsViewModel RelatedPost { get; set; }
        public CategoryViewModel Category { get; set; }
    }
}
