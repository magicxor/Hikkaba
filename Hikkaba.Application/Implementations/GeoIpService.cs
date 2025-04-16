using System.Diagnostics;
using System.Net;
using Hikkaba.Application.Contracts;
using Hikkaba.Application.Telemetry;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Application.Implementations;

public sealed class GeoIpService : IGeoIpService
{
    private readonly ILogger<GeoIpService> _logger;
    private readonly GeoIpAsnReader _geoIpAsnReader;
    private readonly GeoIpCountryReader _geoIpCountryReader;

    public GeoIpService(
        ILogger<GeoIpService> logger,
        GeoIpAsnReader geoIpAsnReader,
        GeoIpCountryReader geoIpCountryReader)
    {
        _logger = logger;
        _geoIpAsnReader = geoIpAsnReader;
        _geoIpCountryReader = geoIpCountryReader;
    }

    public IpAddressInfoModel GetIpAddressInfo(IPAddress ipAddress)
    {
        using var getIpAddressInfoActivity = ApplicationTelemetry.GeoIpServiceSource.StartActivity();
        var result = new IpAddressInfoModel();

        try
        {
            using var getAsnActivity = ApplicationTelemetry.GeoIpServiceSource.StartActivity(
                "GetAsn",
                ActivityKind.Internal,
                null,
                tags: [new("AddressFamily", Enum.GetName(ipAddress.AddressFamily))]);

            if (_geoIpAsnReader.TryAsn(ipAddress, out var asnResponse))
            {
                result.AutonomousSystemOrganization = asnResponse?.AutonomousSystemOrganization;
                result.AutonomousSystemNumber = asnResponse?.AutonomousSystemNumber;
                result.NetworkIpAddress = asnResponse?.Network?.NetworkAddress;
                result.NetworkPrefixLength = asnResponse?.Network?.PrefixLength;

                if (result is { NetworkIpAddress: { } networkIpAddress, NetworkPrefixLength: { } networkPrefixLength })
                {
                    IpRangeCalculator.GetHostRange(networkIpAddress, networkPrefixLength, out var firstHost, out var lastHost);
                    result.LowerIpAddress = firstHost;
                    result.UpperIpAddress = lastHost;
                }

                getAsnActivity?.SetStatus(ActivityStatusCode.Ok);
            }
            else
            {
                getAsnActivity?.SetStatus(ActivityStatusCode.Error, "Can't get ASN information for IP address");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(
                LogEventIds.ErrorGettingAsn,
                e,
                "Error while getting ASN information for IP address {IpAddress}",
                ipAddress);
        }

        try
        {
            using var getCountryActivity = ApplicationTelemetry.GeoIpServiceSource.StartActivity(
                "GetCountry",
                ActivityKind.Internal,
                null,
                tags: [new("AddressFamily", Enum.GetName(ipAddress.AddressFamily))]);

            if (_geoIpCountryReader.TryCountry(ipAddress, out var countryResponse))
            {
                result.CountryIsoCode = countryResponse?.Country.IsoCode;

                getCountryActivity?.SetTag("CountryIsoCode", result.CountryIsoCode);
                getCountryActivity?.SetStatus(ActivityStatusCode.Ok);
            }
            else
            {
                getCountryActivity?.SetStatus(ActivityStatusCode.Error, "Can't get country information for IP address");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(
                LogEventIds.ErrorGettingCountry,
                e,
                "Error while getting country information for IP address {IpAddress}",
                ipAddress);
        }

        return result;
    }
}
