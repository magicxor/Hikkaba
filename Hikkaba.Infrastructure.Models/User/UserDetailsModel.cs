namespace Hikkaba.Infrastructure.Models.User;

public sealed class UserDetailsModel
{
    public required int Id { get; set; }
    public required bool IsDeleted { get; set; }
    public required int AccessFailedCount { get; set; }
    public required bool EmailConfirmed { get; set; }
    public required DateTime? LastLogin { get; set; }
    public required bool LockoutEnabled { get; set; }
    public required DateTimeOffset? LockoutEnd { get; set; }
    public required string? Email { get; set; }
    public required string? UserName { get; set; }
    public required string? PhoneNumber { get; set; }
    public required bool PhoneNumberConfirmed { get; set; }
    public required bool TwoFactorEnabled { get; set; }
}
