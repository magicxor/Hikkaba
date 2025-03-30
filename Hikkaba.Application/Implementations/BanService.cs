using System.Net;
using System.Net.Sockets;
using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Enums;

namespace Hikkaba.Application.Implementations;

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

    public async Task<BanPreviewModel?> FindActiveBan(long? threadId, string? categoryAlias, string userIpAddress)
    {
        return await _banRepository.FindActiveBan(threadId, categoryAlias, userIpAddress);
    }

    public async Task<PagedResult<BanDetailsModel>> ListBansPaginatedAsync(BanPagingFilter banFilter)
    {
        return await _banRepository.ListBansPaginatedAsync(banFilter);
    }

    public async Task<BanDetailsModel?> GetBanAsync(int banId)
    {
        return await _banRepository.GetBanAsync(banId);
    }

    public async Task<int> CreateBanAsync(BanCreateCommand banCreateCommand)
    {
        var bannedIpAddress = IPAddress.Parse(banCreateCommand.BannedIpAddress)
                              ?? throw new ArgumentException("Invalid IP address");
        var bannedIpAddressType = bannedIpAddress.AddressFamily switch
        {
            AddressFamily.InterNetwork => IpAddressType.IpV4,
            AddressFamily.InterNetworkV6 => IpAddressType.IpV6,
            _ => IpAddressType.Unknown,
        };
        var bannedIpAddressInfo = _geoIpService.GetIpAddressInfo(bannedIpAddress);

        return await _banRepository.CreateBanAsync(new BanCreateRequestModel
        {
            EndsAt = banCreateCommand.EndsAt,
            IpAddressType = bannedIpAddressType,
            BannedIpAddress = bannedIpAddress.GetAddressBytes(),
            BannedCidrLowerIpAddress = banCreateCommand.BanByNetwork
                ? bannedIpAddressInfo.LowerIpAddress?.GetAddressBytes()
                : null,
            BannedCidrUpperIpAddress = banCreateCommand.BanByNetwork
                ? bannedIpAddressInfo.UpperIpAddress?.GetAddressBytes()
                : null,
            CountryIsoCode = bannedIpAddressInfo.CountryIsoCode,
            AutonomousSystemNumber = bannedIpAddressInfo.AutonomousSystemNumber,
            AutonomousSystemOrganization = bannedIpAddressInfo.AutonomousSystemOrganization,
            Reason = banCreateCommand.Reason,
            RelatedPostId = banCreateCommand.RelatedPostId,
            CategoryId = banCreateCommand.CategoryId,
        });
    }

    public async Task SetBanDeletedAsync(int banId, bool isDeleted)
    {
        await _banRepository.SetBanDeletedAsync(banId, isDeleted);
    }
}
