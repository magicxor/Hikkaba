namespace Hikkaba.Infrastructure.Models.Role;

public sealed class RoleModel
{
    public required int Id { get; set; }
    public required string? Name { get; set; }
    public required string? NormalizedName { get; set; }
}
