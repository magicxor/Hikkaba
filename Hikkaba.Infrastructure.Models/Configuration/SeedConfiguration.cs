using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Infrastructure.Models.Configuration;

public sealed class SeedConfiguration
{
    [Required]
    [EmailAddress]
    public required string AdministratorEmail { get; set; }

    [Required]
    [MinLength(8)]
    public required string AdministratorPassword { get; set; }
}
