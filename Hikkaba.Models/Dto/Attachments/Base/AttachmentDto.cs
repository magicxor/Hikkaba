using System;
using Hikkaba.Models.Dto.Base.Current;

namespace Hikkaba.Models.Dto.Attachments.Base
{
    public abstract class AttachmentDto: BaseDto
    {
        public Guid PostId { get; set; }
    }
}