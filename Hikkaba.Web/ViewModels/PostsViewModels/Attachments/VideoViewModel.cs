using Hikkaba.Models.Dto.Attachments;
using TPrimaryKey = System.Guid;

namespace Hikkaba.Web.ViewModels.PostsViewModels.Attachments
{
    public class VideoViewModel : VideoDto
    {
        public TPrimaryKey ThreadId { get; set; }
    }
}