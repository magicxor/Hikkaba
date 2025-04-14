using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Web.ViewModels.BansViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
internal static partial class IpAddresDetailsMapper
{
    [MapperIgnoreSource(nameof(IpAddressInfoModel.NetworkIpAddress))]
    [MapperIgnoreSource(nameof(IpAddressInfoModel.NetworkPrefixLength))]
    [MapperIgnoreSource(nameof(IpAddressInfoModel.LowerIpAddress))]
    [MapperIgnoreSource(nameof(IpAddressInfoModel.UpperIpAddress))]
    public static partial IpAddressDetailsViewModel ToViewModel(this IpAddressInfoModel model);
}
