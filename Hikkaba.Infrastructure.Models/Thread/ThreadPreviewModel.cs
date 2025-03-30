using Hikkaba.Infrastructure.Models.Post;

namespace Hikkaba.Infrastructure.Models.Thread;

public class ThreadPreviewModel
{
    public required long Id { get; set; }

    public required bool IsDeleted { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required DateTime? ModifiedAt { get; set; }
    public required DateTime? LastPostCreatedAt { get; set; }

    public required string Title { get; set; }
    public required bool IsPinned { get; set; }
    public required bool IsClosed { get; set; }
    public required int BumpLimit { get; set; }
    public required bool ShowThreadLocalUserHash { get; set; }

    public required int CategoryId { get; set; }
    public required string CategoryAlias { get; set; }
    public required string CategoryName { get; set; }
    public required int PostCount { get; set; }

    public required IReadOnlyList<PostDetailsModel> Posts { get; set; }
}
