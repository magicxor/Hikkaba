using System;
using System.Security.Cryptography.X509Certificates;
using Hikkaba.Infrastructure.Models.Configuration;

namespace Hikkaba.Web.Utils;

internal static class CertificateUtils
{
    public static X509Certificate2 LoadCertificate(HikkabaConfiguration configuration)
    {
        var certBase64 = configuration.AuthCertificateBase64;
        var certPass = configuration.AuthCertificatePassword;
        var certBytes = Convert.FromBase64String(certBase64);
        return X509CertificateLoader.LoadPkcs12(certBytes, certPass, X509KeyStorageFlags.EphemeralKeySet);
    }
}
