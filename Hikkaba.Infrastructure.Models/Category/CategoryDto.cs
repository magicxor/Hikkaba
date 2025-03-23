using Hikkaba.Infrastructure.Models.ApplicationUser;

namespace Hikkaba.Infrastructure.Models.Category;

public class CategoryDto
{
    public required int Id { get; set; }

    public required bool IsDeleted { get; set; }

    public required ApplicationUserPreviewRm CreatedBy { get; set; }

    public required ApplicationUserPreviewRm? ModifiedBy { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required DateTime? ModifiedAt { get; set; }

    public required string Alias { get; set; }
    public required string Name { get; set; }
    public required bool IsHidden { get; set; }
    public required int DefaultBumpLimit { get; set; }
    public required bool DefaultShowThreadLocalUserHash { get; set; }

    public required int BoardId { get; set; }
}
