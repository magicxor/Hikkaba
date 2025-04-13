using System.Collections.Generic;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Web.ViewModels.BansViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
[UseStaticMapper(typeof(Hikkaba.Infrastructure.Mappings.IpAddressMapper))]
internal static partial class BanMapper
{
    [MapperIgnoreSource(nameof(BanDetailsModel.CategoryId))]
    [MapperIgnoreSource(nameof(BanDetailsModel.CreatedById))]
    [MapperIgnoreSource(nameof(BanDetailsModel.ModifiedById))]
    public static partial BanViewModel ToViewModel(this BanDetailsModel model);

    public static partial IReadOnlyList<BanViewModel> ToViewModels(this IReadOnlyList<BanDetailsModel> models);
}
