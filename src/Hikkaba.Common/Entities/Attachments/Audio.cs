using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Common.Entities.Attachments.Base;

namespace Hikkaba.Common.Entities.Attachments
{
    [Table("Audio")]
    public class Audio: FileAttachment
    {
    }
}
