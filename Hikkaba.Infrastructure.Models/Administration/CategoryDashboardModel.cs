using Hikkaba.Infrastructure.Models.ApplicationUser;

namespace Hikkaba.Infrastructure.Models.Administration;

public sealed class CategoryDashboardModel
{
    public required int Id { get; set; }

    public required bool IsDeleted { get; set; }

    public required UserPreviewModel CreatedBy { get; set; }

    public required UserPreviewModel? ModifiedBy { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required DateTime? ModifiedAt { get; set; }

    public required string Alias { get; set; }
    public required string Name { get; set; }
    public required bool IsHidden { get; set; }
    public required int DefaultBumpLimit { get; set; }
    public required bool ShowThreadLocalUserHash { get; set; }

    public required bool ShowCountry { get; set; }

    public required bool ShowOs { get; init; }

    public required bool ShowBrowser { get; init; }

    public required int MaxThreadCount { get; set; }

    public required int BoardId { get; set; }
}
