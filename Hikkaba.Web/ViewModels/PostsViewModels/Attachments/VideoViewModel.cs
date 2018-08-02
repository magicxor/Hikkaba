using System;
using Hikkaba.Models.Dto.Attachments;

namespace Hikkaba.Web.ViewModels.PostsViewModels.Attachments
{
    public class VideoViewModel : VideoDto
    {
        public Guid ThreadId { get; set; }
    }
}