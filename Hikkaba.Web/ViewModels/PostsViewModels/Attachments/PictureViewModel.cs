using TPrimaryKey = System.Guid;
using Hikkaba.Models.Dto.Attachments;

namespace Hikkaba.Web.ViewModels.PostsViewModels.Attachments
{
    public class PictureViewModel : PictureDto
    {
        public TPrimaryKey ThreadId { get; set; }
    }
}