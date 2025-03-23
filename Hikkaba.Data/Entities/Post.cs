using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using Hikkaba.Common.Constants;
using Hikkaba.Data.Entities.Attachments.Base;

namespace Hikkaba.Data.Entities;

[Table("Posts")]
public class Post
{
    [Key]
    public long Id { get; set; }

    [Required]
    public bool IsDeleted { get; set; }

    [Required]
    public required DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    [Required]
    public bool IsSageEnabled { get; set; }

    [MaxLength(Defaults.MaxMessageLength)]
    public required string MessageText { get; set; }

    [MaxLength(Defaults.MaxMessageHtmlLength)]
    public required string MessageHtml { get; set; }

    [Required]
    [Column(TypeName = "varbinary(16)")]
    [MaxLength(Defaults.MaxIpAddressBytesLength)]
    public required byte[] UserIpAddress { get; set; }

    [Required]
    [MaxLength(Defaults.MaxUserAgentLength)]
    public required string UserAgent { get; set; }

    // FK id
    [ForeignKey(nameof(Thread))]
    public long ThreadId { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public int? CreatedById { get; set; }

    [ForeignKey(nameof(ModifiedBy))]
    public int? ModifiedById { get; set; }

    // FK models
    [Required]
    public virtual Thread Thread
    {
        get => _thread
               ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Thread));
        set => _thread = value;
    }

    private Thread? _thread;

    public virtual ApplicationUser? CreatedBy { get; set; }

    public virtual ApplicationUser? ModifiedBy { get; set; }

    // Relations
    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    [InverseProperty(nameof(PostToReply.Post))]
    public virtual ICollection<PostToReply> ParentPosts { get; set; } = new List<PostToReply>();

    [InverseProperty(nameof(PostToReply.Reply))]
    public virtual ICollection<PostToReply> Replies { get; set; } = new List<PostToReply>();
}
