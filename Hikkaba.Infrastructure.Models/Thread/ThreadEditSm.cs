using Hikkaba.Infrastructure.Models.ApplicationUser;

namespace Hikkaba.Infrastructure.Models.Thread;

public class ThreadEditSm
{
    public required long Id { get; set; }

    public required bool IsDeleted { get; set; }

    public required ApplicationUserDto CreatedBy { get; set; }

    public required ApplicationUserDto ModifiedBy { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required DateTime? ModifiedAt { get; set; }

    public required string Title { get; set; }
    public required bool IsPinned { get; set; }
    public required bool IsClosed { get; set; }
    public required int BumpLimit { get; set; }
    public required bool ShowThreadLocalUserHash { get; set; }

    public required int CategoryId { get; set; }
}
