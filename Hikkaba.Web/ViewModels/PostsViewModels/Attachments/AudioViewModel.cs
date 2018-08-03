using System;
using Hikkaba.Models.Dto.Attachments;

namespace Hikkaba.Web.ViewModels.PostsViewModels.Attachments
{
    public class AudioViewModel : AudioDto
    {
        public Guid ThreadId { get; set; }
    }
}