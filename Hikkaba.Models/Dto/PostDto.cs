using System.Collections.Generic;
using Hikkaba.Models.Dto.Attachments;
using Hikkaba.Models.Dto.Base.Current;
using TPrimaryKey = System.Guid;

namespace Hikkaba.Models.Dto
{
    public class PostDto : BaseMutableDto
    {
        public bool IsSageEnabled { get; set; }
        public string Message { get; set; }
        public string UserIpAddress { get; set; }
        public string UserAgent { get; set; }
        public ICollection<AudioDto> Audio { get; set; }
        public ICollection<DocumentDto> Documents { get; set; }
        public ICollection<NoticeDto> Notices { get; set; }
        public ICollection<PictureDto> Pictures { get; set; }
        public ICollection<VideoDto> Video { get; set; }

        public TPrimaryKey ThreadId { get; set; }
    }
}