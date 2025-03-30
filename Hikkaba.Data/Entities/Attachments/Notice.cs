using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Shared.Constants;
using Hikkaba.Data.Entities.Attachments.Base;
using AttachmentType = Hikkaba.Shared.Enums.AttachmentType;

namespace Hikkaba.Data.Entities.Attachments;

public class Notice : Attachment
{
    [Required]
    [MaxLength(Defaults.MaxNoticeLength)]
    public required string Text { get; set; }

    [Required]
    public required DateTime CreatedAt { get; set; }

    // FK id
    [ForeignKey(nameof(CreatedBy))]
    public int CreatedById { get; set; }

    // FK models
    [Required]
    public virtual ApplicationUser CreatedBy
    {
        get => _createdBy
               ?? throw new InvalidOperationException("Uninitialized property: " + nameof(CreatedBy));
        set => _createdBy = value;
    }

    private ApplicationUser? _createdBy;

    public Notice()
    {
        AttachmentType = AttachmentType.Notice;
    }
}
