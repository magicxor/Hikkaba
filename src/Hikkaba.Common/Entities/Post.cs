using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Common.Entities.Attachments;
using Hikkaba.Common.Entities.Base;

namespace Hikkaba.Common.Entities
{
    [Table("Posts")]
    public class Post: BaseMutableEntity
    {
        [Required]
        public bool IsSageEnabled { get; set; }

        [MaxLength(8000)]
        public string Message { get; set; }

        [Required]
        public string UserIpAddress { get; set; }

        [Required]
        public string UserAgent { get; set; }

        [Required]
        public virtual Thread Thread { get; set; }

        public virtual ICollection<Audio> Audio { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<Notice> Notices { get; set; }
        public virtual ICollection<Picture> Pictures { get; set; }
        public virtual ICollection<Video> Video { get; set; }
    }
}
