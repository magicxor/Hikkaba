using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hikkaba.Data.Entities;

[Table("PostToReply")]
public class PostToReply
{
    [Key]
    public int Id { get; set; }

    // FK id
    [ForeignKey(nameof(Post))]
    public long PostId { get; set; }

    [ForeignKey(nameof(Reply))]
    public long ReplyId { get; set; }

    // FK models
    [Required]
    public Post Post
    {
        get => _post
               ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Post));
        set => _post = value;
    }

    private Post? _post;

    [Required]
    public Post Reply
    {
        get => _reply
               ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Reply));
        set => _reply = value;
    }

    private Post? _reply;
}
