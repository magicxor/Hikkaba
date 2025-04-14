using Hikkaba.Infrastructure.Models.User;

namespace Hikkaba.Infrastructure.Models.Thread;

public sealed class ThreadEditRequestModel
{
    public required long Id { get; set; }

    public required bool IsDeleted { get; set; }

    public required UserDetailsModel CreatedBy { get; set; }

    public required UserDetailsModel ModifiedBy { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required DateTime? ModifiedAt { get; set; }

    public required string Title { get; set; }
    public required bool IsPinned { get; set; }
    public required bool IsClosed { get; set; }
    public required int BumpLimit { get; set; }
    public required bool ShowThreadLocalUserHash { get; set; }

    public required int CategoryId { get; set; }
}
