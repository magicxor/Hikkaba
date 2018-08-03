using System;
using Hikkaba.Models.Dto.Attachments;

namespace Hikkaba.Web.ViewModels.PostsViewModels.Attachments
{
    public class NoticeViewModel : NoticeDto
    {
        public Guid ThreadId { get; set; }
    }
}