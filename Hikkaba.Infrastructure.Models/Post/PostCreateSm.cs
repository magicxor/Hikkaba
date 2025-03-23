using Hikkaba.Infrastructure.Models.ApplicationUser;
using Hikkaba.Infrastructure.Models.Attachments;

namespace Hikkaba.Infrastructure.Models.Post;

public class PostCreateSm
{
    public required bool IsSageEnabled { get; set; }

    public required string Message { get; set; }

    public required string UserIpAddress { get; set; }

    public required string UserAgent { get; set; }

    public required IReadOnlyList<AudioDto> Audio { get; set; }

    public required IReadOnlyList<DocumentDto> Documents { get; set; }

    public required IReadOnlyList<NoticeDto> Notices { get; set; }

    public required IReadOnlyList<PictureDto> Pictures { get; set; }

    public required IReadOnlyList<VideoDto> Video { get; set; }

    public required long? ThreadId { get; set; }
}
