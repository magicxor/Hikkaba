using Hikkaba.Models.Dto.Attachments.Base;
using TPrimaryKey = System.Guid;

namespace Hikkaba.Models.Dto.Attachments
{
    public class NoticeDto : AttachmentDto
    {
        public string Text { get; set; }
        public TPrimaryKey AuthorId { get; set; }
        public string AuthorName { get; set; }
    }
}