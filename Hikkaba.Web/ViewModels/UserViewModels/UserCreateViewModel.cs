using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.UserViewModels;

public class UserCreateViewModel
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string UserName { get; set; }

    [Required]
    public required string Password { get; set; }
}
