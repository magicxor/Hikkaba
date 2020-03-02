using TPrimaryKey = System.Guid;
using Hikkaba.Models.Dto.Base.Current;

namespace Hikkaba.Models.Dto.Attachments.Base
{
    public abstract class AttachmentDto: BaseDto
    {
        public TPrimaryKey PostId { get; set; }
    }
}