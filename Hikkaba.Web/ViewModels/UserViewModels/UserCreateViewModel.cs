using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Shared.Constants;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hikkaba.Web.ViewModels.UserViewModels;

public sealed class UserCreateViewModel
{
    [Required]
    [EmailAddress]
    [MaxLength(Defaults.MaxEmailLength)]
    public required string Email { get; set; }

    [Required]
    [RegularExpression(ValidationRegularExpressions.LowercaseLatinCharsDigitsUnderscore)]
    public required string UserName { get; set; }

    [Required]
    [MinLength(Defaults.MinUserPasswordLength)]
    [MaxLength(Defaults.MaxUserPasswordLength)]
    public required string Password { get; set; }

    [Required]
    public required IReadOnlyList<int> UserRoleIds { get; set; }

    public IReadOnlyList<SelectListItem> AllExistingRoles { get; set; } = [];
}
