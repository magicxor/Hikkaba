using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Common.Entities.Attachments.Base;

namespace Hikkaba.Common.Entities.Attachments
{
    [Table("Documents")]
    public class Document: FileAttachment
    {
    }
}
