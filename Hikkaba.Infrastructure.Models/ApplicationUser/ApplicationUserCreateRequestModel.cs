namespace Hikkaba.Infrastructure.Models.ApplicationUser;

public class ApplicationUserCreateRequestModel
{
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
}
