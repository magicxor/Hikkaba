using Hikkaba.Infrastructure.Models.ApplicationUser;
using Hikkaba.Infrastructure.Models.Attachments;

namespace Hikkaba.Infrastructure.Models.Post;

public class PostEditRm
{
    public required long Id { get; set; }

    public required bool IsDeleted { get; set; }

    public required ApplicationUserViewRm CreatedBy { get; set; }

    public required ApplicationUserViewRm ModifiedBy { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required DateTime? ModifiedAt { get; set; }

    public required bool IsSageEnabled { get; set; }

    public required string Message { get; set; }

    public required string? UserIpAddress { get; set; }

    public required string UserAgent { get; set; }

    public required IReadOnlyList<AudioViewRm> Audio { get; set; }

    public required IReadOnlyList<DocumentViewRm> Documents { get; set; }

    public required IReadOnlyList<NoticeViewRm> Notices { get; set; }

    public required IReadOnlyList<PictureViewRm> Pictures { get; set; }

    public required IReadOnlyList<VideoViewRm> Video { get; set; }

    public required long ThreadId { get; set; }
}
