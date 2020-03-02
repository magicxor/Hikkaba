using TPrimaryKey = System.Guid;
using Hikkaba.Models.Dto.Attachments;

namespace Hikkaba.Web.ViewModels.PostsViewModels.Attachments
{
    public class DocumentViewModel : DocumentDto
    {
        public TPrimaryKey ThreadId { get; set; }
    }
}