namespace Hikkaba.Infrastructure.Models.Post;

public class PostCreateRm
{
    public required Guid BlobContainerId { get; set; }

    public required bool IsSageEnabled { get; set; }

    public required string MessageHtml { get; set; }

    public required string MessageText { get; set; }

    public required byte[]? UserIpAddress { get; set; }

    public required string UserAgent { get; set; }

    public required long ThreadId { get; set; }

    public required IReadOnlyList<long> MentionedPosts { get; set; }
}
