using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Data.Entities.Base.Current;

namespace Hikkaba.Data.Entities.Attachments.Base;

[Table("Attachments")]
public abstract class Attachment: BaseEntity
{
    [Required]
    public virtual Post Post { get; set; }
}