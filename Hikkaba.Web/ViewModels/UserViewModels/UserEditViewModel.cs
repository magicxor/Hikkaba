using System;

namespace Hikkaba.Web.ViewModels.UserViewModels;

public class UserEditViewModel
{
    public required int Id { get; set; }
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public required DateTime? LockoutEndDate { get; set; }
    public required bool TwoFactorEnabled { get; set; }
}
