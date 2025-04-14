namespace Hikkaba.Infrastructure.Models.User;

public sealed class CategoryModeratorModel
{
    public required int Id { get; set; }
    public required DateTime? LastLogin { get; set; }
    public required string? Email { get; set; }
    public required string? UserName { get; set; }
    public required bool IsCategoryModerator { get; set; }
}
