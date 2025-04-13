using Hikkaba.Infrastructure.Models.Attachments.Concrete;
using Hikkaba.Infrastructure.Models.User;

namespace Hikkaba.Infrastructure.Models.Post;

public sealed class PostEditRequestModel
{
    public required long Id { get; set; }

    public required bool IsDeleted { get; set; }

    public required UserDetailsModel CreatedBy { get; set; }

    public required UserDetailsModel ModifiedBy { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required DateTime? ModifiedAt { get; set; }

    public required bool IsSageEnabled { get; set; }

    public required string Message { get; set; }

    public required string? UserIpAddress { get; set; }

    public required string UserAgent { get; set; }

    public required IReadOnlyList<AudioModel> Audio { get; set; }

    public required IReadOnlyList<DocumentModel> Documents { get; set; }

    public required IReadOnlyList<NoticeModel> Notices { get; set; }

    public required IReadOnlyList<PictureModel> Pictures { get; set; }

    public required IReadOnlyList<VideoModel> Video { get; set; }

    public required long ThreadId { get; set; }
}
