using System;
using Hikkaba.Common.Dto.Base;

namespace Hikkaba.Common.Dto.Attachments.Base
{
    public abstract class AttachmentDto: BaseDto
    {
        public Guid PostId { get; set; }
    }
}