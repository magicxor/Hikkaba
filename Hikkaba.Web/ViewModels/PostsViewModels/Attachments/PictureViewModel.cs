using Hikkaba.Models.Dto.Attachments;
using TPrimaryKey = System.Guid;

namespace Hikkaba.Web.ViewModels.PostsViewModels.Attachments
{
    public class PictureViewModel : PictureDto
    {
        public TPrimaryKey ThreadId { get; set; }
    }
}