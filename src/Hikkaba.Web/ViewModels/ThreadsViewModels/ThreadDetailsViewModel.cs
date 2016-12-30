using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Common.Constants;
using Hikkaba.Web.ViewModels.PostsViewModels;

namespace Hikkaba.Web.ViewModels.ThreadsViewModels
{
    public class ThreadDetailsViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = @"Is deleted")]
        public bool IsDeleted { get; set; }

        [DisplayFormat(DataFormatString = Defaults.CsharpGlobalDateTimeFormatString)]
        [Display(Name = @"Creation date and time")]
        public DateTime Created { get; set; }

        [DisplayFormat(DataFormatString = Defaults.CsharpGlobalDateTimeFormatString)]
        [Display(Name = @"Modification date and time")]
        public DateTime? Modified { get; set; }


        [Display(Name = @"Title")]
        public string Title { get; set; }

        [Display(Name = @"Is pinned")]
        public bool IsPinned { get; set; }

        [Display(Name = @"Is closed")]
        public bool IsClosed { get; set; }

        [Display(Name = @"Bump limit")]
        public int BumpLimit { get; set; }

        [Display(Name = @"Show thread-local user hashes")]
        public bool ShowThreadLocalUserHash { get; set; }


        public Guid CategoryId { get; set; }


        [Display(Name = @"Category alias")]
        public string CategoryAlias { get; set; }

        [Display(Name = @"Category name")]
        public string CategoryName { get; set; }

        [Display(Name = @"Post count")]
        public int PostCount { get; set; }


        [Display(Name = @"Posts")]
        public IList<PostDetailsViewModel> Posts { get; set; }
    }
}