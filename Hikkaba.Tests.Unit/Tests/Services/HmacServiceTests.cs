using System;
using Hikkaba.Application.Implementations;
using Hikkaba.Infrastructure.Models.Configuration;
using Microsoft.Extensions.Options;

namespace Hikkaba.Tests.Unit.Tests.Services;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
internal sealed class HmacServiceTests
{
    [TestCase("QQQ##12098456xYz@dIihns", "QQQ!!gMynliJmDDT+")]
    [TestCase("ZzZ##12098456xYz@dIihns", "ZzZ!!m1ZQDXCNymrO")]
    [TestCase("Y0uR_N@m3##@@@edfsw984ehiuOH@", "Y0uR_N@m3!!p54+jnfrEQwL")]
    public void GetTripCode_ShouldReturnCorrectTripCode(string inputTripCode, string expectedTripCode)
    {
        var options = Options.Create(new HikkabaConfiguration
        {
            CacheMaxAgeSeconds = 0,
            CacheCategoriesExpirationSeconds = 0,
            ThumbnailsMaxWidth = 0,
            ThumbnailsMaxHeight = 0,
            MaxAttachmentsCountPerPost = 0,
            MaxAttachmentsBytesPerPost = 0,
            AuthCertificateBase64 = string.Empty,
            AuthCertificatePassword = string.Empty,
            MaxPostsFromIpWithin5Minutes = 0,
            OtlpExporterUri = new Uri("http://localhost"),
            MaintenanceKey = string.Empty,
            TripCodeSalt = "831cc049-c706-44e9-829c-cc5ac0602c5e",
            StoragePath = string.Empty,
        });
        var hmacService = new HmacService(options);
        var renderedTripCode = hmacService.GetTripCode(inputTripCode);

        Assert.That(renderedTripCode, Is.EqualTo(expectedTripCode));
    }
}
