using System;
using System.Collections.Generic;
using Hikkaba.Web.ViewModels.RoleViewModels;

namespace Hikkaba.Web.ViewModels.UserViewModels;

public sealed class UserDetailsViewModel
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
    public required IReadOnlyList<RoleSlimViewModel> UserRoles { get; set; }
}
