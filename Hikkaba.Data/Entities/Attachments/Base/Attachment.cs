using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Common.Enums;

namespace Hikkaba.Data.Entities.Attachments.Base;

[Table("Attachments")]
public abstract class Attachment
{
    [Key]
    public long Id { get; set; }

    public AttachmentType AttachmentType { get; set; }

    // FK id
    [ForeignKey(nameof(Post))]
    public long PostId { get; set; }

    // FK models
    [Required]
    public virtual Post Post
    {
        get => _post
               ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Post));
        set => _post = value;
    }

    private Post? _post;
}
