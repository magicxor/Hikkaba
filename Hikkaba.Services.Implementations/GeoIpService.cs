using System.Net;
using Hikkaba.Infrastructure.Models;
using Hikkaba.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Services.Implementations;

public class GeoIpService : IGeoIpService
{
    private readonly ILogger<GeoIpService> _logger;
    private readonly GeoIpReaderAsn _geoIpReaderAsn;
    private readonly GeoIpReaderCountry _geoIpReaderCountry;

    public GeoIpService(
        ILogger<GeoIpService> logger,
        GeoIpReaderAsn geoIpReaderAsn,
        GeoIpReaderCountry geoIpReaderCountry)
    {
        _logger = logger;
        _geoIpReaderAsn = geoIpReaderAsn;
        _geoIpReaderCountry = geoIpReaderCountry;
    }

    public IpAddressInfoSm GetIpAddressInfo(IPAddress ipAddress)
    {
        var result = new IpAddressInfoSm();

        try
        {
            if (_geoIpReaderAsn.TryAsn(ipAddress, out var asnResponse))
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
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Can't get ASN information for IP address {IpAddress}", ipAddress);
        }

        try
        {
            if (_geoIpReaderCountry.TryCountry(ipAddress, out var countryResponse))
            {
                result.CountryIsoCode = countryResponse?.Country.IsoCode;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Can't get country information for IP address {IpAddress}", ipAddress);
        }

        return result;
    }
}
