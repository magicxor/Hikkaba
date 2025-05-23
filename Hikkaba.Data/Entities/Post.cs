﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Shared.Constants;
using Hikkaba.Data.Entities.Attachments;

namespace Hikkaba.Data.Entities;

[Table("Posts")]
public class Post
{
    [Key]
    public long Id { get; set; }

    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required Guid BlobContainerId { get; set; }

    [Required]
    public bool IsDeleted { get; set; }

    [Required]
    public required DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    [Required]
    public bool IsSageEnabled { get; set; }

    [MaxLength(Defaults.MaxMessageTextLength)]
    public required string MessageText { get; set; }

    [MaxLength(Defaults.MaxMessageHtmlLength)]
    public required string MessageHtml { get; set; }

    [Column(TypeName = "varbinary(16)")]
    [MaxLength(Defaults.MaxIpAddressBytesLength)]
    public required byte[]? UserIpAddress { get; set; }

    [Required]
    [MaxLength(Defaults.MaxUserAgentLength)]
    public required string UserAgent { get; set; }

    [MaxLength(Defaults.MaxCountryIsoCodeLength)]
    public string? CountryIsoCode { get; set; }

    [MaxLength(Defaults.MaxBrowserTypeLength)]
    public string? BrowserType { get; set; }

    [MaxLength(Defaults.MaxOsTypeLength)]
    public string? OsType { get; set; }

    [Required]
    [MinLength(Defaults.MaxFileHashBytesLength)]
    [MaxLength(Defaults.MaxFileHashBytesLength)]
    [Column(TypeName = "binary(32)")]
    public required byte[] ThreadLocalUserHash { get; set; }

    [Required]
    public required bool IsOriginalPost { get; set; }

    [Required]
    public bool HasOriginalPosterMark { get; set; }

    // FK id
    [ForeignKey(nameof(Thread))]
    public long ThreadId { get; set; }

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

    public virtual ApplicationUser? ModifiedBy { get; set; }

    // Relations
    public virtual ICollection<Audio> Audios { get; set; } = new List<Audio>();

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<Notice> Notices { get; set; } = new List<Notice>();

    public virtual ICollection<Picture> Pictures { get; set; } = new List<Picture>();

    public virtual ICollection<Video> Videos { get; set; } = new List<Video>();

    public virtual ICollection<Post> MentionedPosts { get; set; } = new List<Post>();

    public virtual ICollection<Post> Replies { get; set; } = new List<Post>();

    [InverseProperty(nameof(PostToReply.Post))]
    public virtual ICollection<PostToReply> RepliesToThisMentionedPost { get; set; } = new List<PostToReply>();

    [InverseProperty(nameof(PostToReply.Reply))]
    public virtual ICollection<PostToReply> MentionedPostsToThisReply { get; set; } = new List<PostToReply>();
}
