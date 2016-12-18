using System;
using Hikkaba.Common.Dto.Attachments.Base;

namespace Hikkaba.Common.Dto.Attachments
{
    public class NoticeDto : AttachmentDto
    {
        public string Text { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; }
    }
}