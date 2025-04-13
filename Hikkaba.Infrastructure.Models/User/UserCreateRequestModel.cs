namespace Hikkaba.Infrastructure.Models.User;

public sealed class UserCreateRequestModel
{
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
}
