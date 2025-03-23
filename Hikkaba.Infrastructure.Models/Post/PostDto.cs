using Hikkaba.Infrastructure.Models.ApplicationUser;
using Hikkaba.Infrastructure.Models.Attachments;

namespace Hikkaba.Infrastructure.Models.Post;

public class PostDto
{
    public required long Id { get; set; }

    public required bool IsDeleted { get; set; }

    public required ApplicationUserDto CreatedBy { get; set; }

    public required ApplicationUserDto ModifiedBy { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required DateTime? ModifiedAt { get; set; }

    public required bool IsSageEnabled { get; set; }

    public required string Message { get; set; }

    public required string UserIpAddress { get; set; }

    public required string UserAgent { get; set; }

    public required IReadOnlyList<AudioDto> Audio { get; set; }

    public required IReadOnlyList<DocumentDto> Documents { get; set; }

    public required IReadOnlyList<NoticeDto> Notices { get; set; }

    public required IReadOnlyList<PictureDto> Pictures { get; set; }

    public required IReadOnlyList<VideoDto> Video { get; set; }

    public required long ThreadId { get; set; }
}
