using System.Net;
using System.Net.Sockets;
using Hikkaba.Common.Enums;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Paging.Models;
using Hikkaba.Repositories.Contracts;
using Hikkaba.Services.Contracts;

namespace Hikkaba.Services.Implementations;

public class BanService : IBanService
{
    private readonly IGeoIpService _geoIpService;
    private readonly IBanRepository _banRepository;

    public BanService(
        IGeoIpService geoIpService,
        IBanRepository banRepository)
    {
        _geoIpService = geoIpService;
        _banRepository = banRepository;
    }

    public async Task<BanPreviewRm?> FindActiveBan(long? threadId, string? categoryAlias, string userIpAddress)
    {
        return await _banRepository.FindActiveBan(threadId, categoryAlias, userIpAddress);
    }

    public async Task<PagedResult<BanRm>> ListBansPaginatedAsync(BanPagingFilter banFilter)
    {
        return await _banRepository.ListBansPaginatedAsync(banFilter);
    }

    public async Task<BanRm?> GetBanAsync(int banId)
    {
        return await _banRepository.GetBanAsync(banId);
    }

    public async Task<int> CreateBanAsync(BanCreateRequestSm banCreateRequestSm)
    {
        var bannedIpAddress = IPAddress.Parse(banCreateRequestSm.BannedIpAddress)
                              ?? throw new ArgumentException("Invalid IP address");
        var bannedIpAddressType = bannedIpAddress.AddressFamily switch
        {
            AddressFamily.InterNetwork => IpAddressType.IpV4,
            AddressFamily.InterNetworkV6 => IpAddressType.IpV6,
            _ => IpAddressType.Unknown,
        };
        var bannedIpAddressInfo = _geoIpService.GetIpAddressInfo(bannedIpAddress);

        return await _banRepository.CreateBanAsync(new BanCreateRequestRm
        {
            EndsAt = banCreateRequestSm.EndsAt,
            IpAddressType = bannedIpAddressType,
            BannedIpAddress = bannedIpAddress.GetAddressBytes(),
            BannedCidrLowerIpAddress = banCreateRequestSm.BanByNetwork
                ? bannedIpAddressInfo.LowerIpAddress?.GetAddressBytes()
                : null,
            BannedCidrUpperIpAddress = banCreateRequestSm.BanByNetwork
                ? bannedIpAddressInfo.UpperIpAddress?.GetAddressBytes()
                : null,
            CountryIsoCode = bannedIpAddressInfo.CountryIsoCode,
            AutonomousSystemNumber = bannedIpAddressInfo.AutonomousSystemNumber,
            AutonomousSystemOrganization = bannedIpAddressInfo.AutonomousSystemOrganization,
            Reason = banCreateRequestSm.Reason,
            RelatedPostId = banCreateRequestSm.RelatedPostId,
            CategoryId = banCreateRequestSm.CategoryId,
        });
    }

    public async Task SetBanDeletedAsync(int banId, bool isDeleted)
    {
        await _banRepository.SetBanDeletedAsync(banId, isDeleted);
    }
}
