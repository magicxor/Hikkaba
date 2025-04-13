namespace Hikkaba.Infrastructure.Models.User;

public sealed class UserEditRequestModel
{
    public required int Id { get; set; }
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public required DateTime? LockoutEndDate { get; set; }
    public required bool TwoFactorEnabled { get; set; }
}
