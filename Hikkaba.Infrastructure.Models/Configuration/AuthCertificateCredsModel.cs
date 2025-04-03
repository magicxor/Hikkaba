namespace Hikkaba.Infrastructure.Models.Configuration;

public class AuthCertificateCredsModel
{
    public required string AuthCertificateBase64 { get; set; }

    public required string AuthCertificatePassword { get; set; }
}
