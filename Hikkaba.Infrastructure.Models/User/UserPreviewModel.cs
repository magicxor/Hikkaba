namespace Hikkaba.Infrastructure.Models.User;

public sealed class UserPreviewModel
{
    public required int Id { get; set; }
    public required string? UserName { get; set; }
    public required string? Email { get; set; }
    public required DateTime? LastLoginAt { get; set; }
}
