using System;
using Hikkaba.Models.Dto.Attachments;

namespace Hikkaba.Web.ViewModels.PostsViewModels.Attachments
{
    public class PictureViewModel : PictureDto
    {
        public Guid ThreadId { get; set; }
    }
}