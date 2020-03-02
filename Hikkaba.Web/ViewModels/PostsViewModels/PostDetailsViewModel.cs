using TPrimaryKey = System.Guid;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Web.ViewModels.PostsViewModels.Attachments;

namespace Hikkaba.Web.ViewModels.PostsViewModels
{
    public class PostDetailsViewModel
    {
        public int Index { get; set; }
        public TPrimaryKey Id { get; set; }

        [Display(Name = @"Is deleted")]
        public bool IsDeleted { get; set; }

        [Display(Name = @"Creation date and time")]
        public DateTime Created { get; set; }

        [Display(Name = @"Modification date and time")]
        public DateTime? Modified { get; set; }


        [Display(Name = @"Sage")]
        public bool IsSageEnabled { get; set; }

        [Display(Name = @"Message")]
        public string Message { get; set; }

        [Display(Name = @"IP")]
        public string UserIpAddress { get; set; }

        [Display(Name = @"User-agent")]
        public string UserAgent { get; set; }

        [Display(Name = @"Audio")]
        public ICollection<AudioViewModel> Audio { get; set; }

        [Display(Name = @"Documents")]
        public ICollection<DocumentViewModel> Documents { get; set; }

        [Display(Name = @"Notices")]
        public ICollection<NoticeViewModel> Notices { get; set; }

        [Display(Name = @"Pictures")]
        public ICollection<PictureViewModel> Pictures { get; set; }

        [Display(Name = @"Video")]
        public ICollection<VideoViewModel> Video { get; set; }


        public TPrimaryKey ThreadId { get; set; }


        [Display(Name = @"Show thread-local user hashes")]
        public bool ThreadShowThreadLocalUserHash { get; set; }

        [Display(Name = @"Category alias")]
        public string CategoryAlias { get; set; }

        public TPrimaryKey CategoryId { get; set; }

        [Display(Name = @"Answers")]
        public ICollection<TPrimaryKey> Answers { get; set; }
    }
}