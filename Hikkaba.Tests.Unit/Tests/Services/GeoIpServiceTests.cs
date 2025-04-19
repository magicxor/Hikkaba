using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Hikkaba.Application.Implementations;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hikkaba.Tests.Unit.Tests.Services;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
internal sealed class GeoIpServiceTests
{
    private static readonly string Asn = Path.Combine("Files", "GeoLite2-ASN.mmdb");
    private static readonly string Country = Path.Combine("Files", "GeoLite2-Country.mmdb");
    private static readonly List<string> Locales = ["en"];

    [Test]
    public void GeoIpAsnReader_ShouldReturnBuildDate1()
    {
        using var geoIpAsnReader = new GeoIpAsnReader(Asn);
        var buildDate = geoIpAsnReader.Metadata.BuildDate;
        Assert.That(buildDate, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void GeoIpCountryReader_ShouldReturnBuildDate1()
    {
        using var geoIpCountryReader = new GeoIpCountryReader(Country);
        var buildDate = geoIpCountryReader.Metadata.BuildDate;
        Assert.That(buildDate, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void GeoIpAsnReader_ShouldReturnBuildDate2()
    {
        using var geoIpAsnReader = new GeoIpAsnReader(Asn, Locales);
        var buildDate = geoIpAsnReader.Metadata.BuildDate;
        Assert.That(buildDate, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void GeoIpCountryReader_ShouldReturnBuildDate2()
    {
        using var geoIpCountryReader = new GeoIpCountryReader(Country, Locales);
        var buildDate = geoIpCountryReader.Metadata.BuildDate;
        Assert.That(buildDate, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void GeoIpAsnReader_ShouldReturnBuildDate3()
    {
        using var fs = new FileStream(Asn, FileMode.Open, FileAccess.Read);
        using var geoIpAsnReader = new GeoIpAsnReader(fs);
        var buildDate = geoIpAsnReader.Metadata.BuildDate;
        Assert.That(buildDate, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void GeoIpCountryReader_ShouldReturnBuildDate3()
    {
        using var fs = new FileStream(Country, FileMode.Open, FileAccess.Read);
        using var geoIpCountryReader = new GeoIpCountryReader(fs);
        var buildDate = geoIpCountryReader.Metadata.BuildDate;
        Assert.That(buildDate, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void GeoIpAsnReader_ShouldReturnBuildDate4()
    {
        using var fs = new FileStream(Asn, FileMode.Open, FileAccess.Read);
        using var geoIpAsnReader = new GeoIpAsnReader(fs, Locales);
        var buildDate = geoIpAsnReader.Metadata.BuildDate;
        Assert.That(buildDate, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void GeoIpCountryReader_ShouldReturnBuildDate4()
    {
        using var fs = new FileStream(Country, FileMode.Open, FileAccess.Read);
        using var geoIpCountryReader = new GeoIpCountryReader(fs, Locales);
        var buildDate = geoIpCountryReader.Metadata.BuildDate;
        Assert.That(buildDate, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void GeoIpService()
    {
        using var geoIpAsnReader = new GeoIpAsnReader(Asn);
        using var geoIpCountryReader = new GeoIpCountryReader(Country);
        var geoIpService = new GeoIpService(NullLogger<GeoIpService>.Instance, geoIpAsnReader, geoIpCountryReader);
        var ip = IPAddress.Parse("52.94.236.248");
        var info = geoIpService.GetIpAddressInfo(ip);
        Assert.That(info, Is.Not.Null);
        Assert.That(info.AutonomousSystemOrganization, Contains.Substring("AMAZON"));
    }
}
