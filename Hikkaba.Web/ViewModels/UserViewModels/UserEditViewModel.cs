using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Shared.Constants;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hikkaba.Web.ViewModels.UserViewModels;

public sealed class UserEditViewModel
{
    [Required]
    public required int Id { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(Defaults.MaxEmailLength)]
    public required string Email { get; set; }

    [Required]
    [RegularExpression(ValidationRegularExpressions.LowercaseLatinCharsDigitsUnderscore)]
    public required string UserName { get; set; }

    public required DateTime? LockoutEndDate { get; set; }

    [Required]
    public required bool TwoFactorEnabled { get; set; }

    [Required]
    public required IReadOnlyList<int> UserRoleIds { get; set; } = [];

    public IReadOnlyList<SelectListItem> AllExistingRoles { get; set; } = [];
}
