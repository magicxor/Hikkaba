using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Infrastructure.Models.Configuration;

public sealed class SmtpClientConfiguration
{
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string DisplayName { get; set; }

    [Required]
    public required string Password { get; set; }

    [Required]
    public required string Host { get; set; }

    [Required]
    [Range(1, ushort.MaxValue)]
    public required int Port { get; set; }

    [Required]
    public required bool UseSecureConnection { get; set; }
}
