using System;

namespace Hikkaba.Web.ViewModels.AdministrationViewModels;

public class ApplicationUserViewModel
{
    public required int Id { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required DateTime? LastLoginAt { get; set; }
}
