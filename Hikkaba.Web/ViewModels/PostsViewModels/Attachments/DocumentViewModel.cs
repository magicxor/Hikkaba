using Hikkaba.Models.Dto.Attachments;
using TPrimaryKey = System.Guid;

namespace Hikkaba.Web.ViewModels.PostsViewModels.Attachments
{
    public class DocumentViewModel : DocumentDto
    {
        public TPrimaryKey ThreadId { get; set; }
    }
}