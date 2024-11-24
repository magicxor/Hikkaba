using TPrimaryKey = System.Guid;
using System;

namespace Hikkaba.Web.ViewModels.AdministrationViewModels;

public class ApplicationUserViewModel
{
    public TPrimaryKey Id { get; set; }
    public bool IsDeleted { get; set; }
    public int AccessFailedCount { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
}