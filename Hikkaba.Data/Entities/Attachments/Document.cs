using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Data.Entities.Attachments.Base;

namespace Hikkaba.Data.Entities.Attachments
{
    [Table("Documents")]
    public class Document: FileAttachment
    {
    }
}
