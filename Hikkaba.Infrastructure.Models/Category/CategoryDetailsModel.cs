using Hikkaba.Infrastructure.Models.ApplicationUser;

namespace Hikkaba.Infrastructure.Models.Category;

public class CategoryDetailsModel
{
    public required int Id { get; set; }

    public required bool IsDeleted { get; set; }

    public required ApplicationUserPreviewModel CreatedBy { get; set; }

    public required ApplicationUserPreviewModel? ModifiedBy { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required DateTime? ModifiedAt { get; set; }

    public required string Alias { get; set; }
    public required string Name { get; set; }
    public required bool IsHidden { get; set; }
    public required int DefaultBumpLimit { get; set; }
    public required bool ShowThreadLocalUserHash { get; set; }

    public required bool ShowUserAgent { get; set; }

    public required bool ShowCountry { get; set; }
    public required int MaxThreadCount { get; set; }

    public required int BoardId { get; set; }
}
