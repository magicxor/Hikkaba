using Hikkaba.Models.Dto.Base.Current;
using TPrimaryKey = System.Guid;

namespace Hikkaba.Models.Dto.Attachments.Base
{
    public abstract class AttachmentDto: BaseDto
    {
        public TPrimaryKey PostId { get; set; }
    }
}