using System.Net;
using System.Net.Sockets;
using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Enums;

namespace Hikkaba.Application.Implementations;

public sealed class BanService : IBanService
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

    public async Task<BanPreviewModel?> FindActiveBan(ActiveBanFilter filter, CancellationToken cancellationToken)
    {
        return await _banRepository.FindActiveBanAsync(filter, cancellationToken);
    }

    public async Task<PagedResult<BanDetailsModel>> ListBansPaginatedAsync(BanPagingFilter banFilter, CancellationToken cancellationToken)
    {
        return await _banRepository.ListBansPaginatedAsync(banFilter, cancellationToken);
    }

    public async Task<BanDetailsModel?> GetBanAsync(int banId, CancellationToken cancellationToken)
    {
        return await _banRepository.GetBanAsync(banId, cancellationToken);
    }

    public async Task<BanCreateResultModel> CreateBanAsync(BanCreateCommand banCreateCommand, CancellationToken cancellationToken)
    {
        var bannedIpAddress = IPAddress.Parse(banCreateCommand.BannedIpAddress);
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
            BanInAllCategories = banCreateCommand.BanInAllCategories,
            CountryIsoCode = bannedIpAddressInfo.CountryIsoCode,
            AutonomousSystemNumber = bannedIpAddressInfo.AutonomousSystemNumber,
            AutonomousSystemOrganization = bannedIpAddressInfo.AutonomousSystemOrganization,
            AdditionalAction = banCreateCommand.AdditionalAction,
            Reason = banCreateCommand.Reason,
            RelatedPostId = banCreateCommand.RelatedPostId,
            RelatedThreadId = banCreateCommand.RelatedThreadId,
            CategoryAlias = banCreateCommand.CategoryAlias,
        }, cancellationToken);
    }

    public async Task SetBanDeletedAsync(int banId, bool isDeleted, CancellationToken cancellationToken)
    {
        await _banRepository.SetBanDeletedAsync(banId, isDeleted, cancellationToken);
    }
}
