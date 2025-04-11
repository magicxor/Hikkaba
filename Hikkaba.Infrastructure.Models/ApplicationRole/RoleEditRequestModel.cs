namespace Hikkaba.Infrastructure.Models.ApplicationRole;

public sealed class RoleEditRequestModel
{
    public required int RoleId { get; set; }
    public required string RoleName { get; set; }
}
