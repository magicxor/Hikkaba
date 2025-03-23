using Hikkaba.Infrastructure.Models.ApplicationUser;
using Hikkaba.Infrastructure.Models.Attachments;

namespace Hikkaba.Infrastructure.Models.Post;

public class PostPreviewRm
{
    public required int Index { get; set; }
    public required long Id { get; set; }

    public required bool IsDeleted { get; set; }

    public required ApplicationUserPreviewRm CreatedBy { get; set; }

    public required ApplicationUserPreviewRm ModifiedBy { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required DateTime? ModifiedAt { get; set; }

    public required bool IsSageEnabled { get; set; }
    public required string MessageHtml { get; set; }
    public required string UserIpAddress { get; set; }
    public required string UserAgent { get; set; }
    public required IReadOnlyList<AudioDto> Audio { get; set; }
    public required IReadOnlyList<DocumentDto> Documents { get; set; }
    public required IReadOnlyList<NoticeDto> Notices { get; set; }
    public required IReadOnlyList<PictureDto> Pictures { get; set; }
    public required IReadOnlyList<VideoDto> Video { get; set; }

    public required long ThreadId { get; set; }
    public required bool ThreadShowThreadLocalUserHash { get; set; }
    public required string CategoryAlias { get; set; }
    public required int CategoryId { get; set; }
    public required IReadOnlyList<long> Replies { get; set; }
}
