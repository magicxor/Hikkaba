using System.ComponentModel.DataAnnotations;
using Hikkaba.Data.Entities.Base.Current;

namespace Hikkaba.Data.Entities.Attachments.Base
{
    public abstract class Attachment: BaseEntity
    {
        [Required]
        public virtual Post Post { get; set; }
    }
}
