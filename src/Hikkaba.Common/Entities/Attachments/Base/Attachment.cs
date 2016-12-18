using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Common.Entities.Base;

namespace Hikkaba.Common.Entities.Attachments.Base
{
    public abstract class Attachment: BaseEntity
    {
        [Required]
        public virtual Post Post { get; set; }
    }
}
