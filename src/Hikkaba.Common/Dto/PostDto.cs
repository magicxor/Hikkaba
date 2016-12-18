using System;
using System.Collections.Generic;
using Hikkaba.Common.Dto.Attachments;
using Hikkaba.Common.Dto.Base;

namespace Hikkaba.Common.Dto
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

        public Guid ThreadId { get; set; }
    }
}