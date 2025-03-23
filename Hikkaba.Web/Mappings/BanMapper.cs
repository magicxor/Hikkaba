using System.Collections.Generic;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Web.ViewModels.BansViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
[UseStaticMapper(typeof(IpAddressMapper))]
public static partial class BanMapper
{
    public static partial BanPreviewModel ToViewModel(this BanPreviewRm model);

    [MapperIgnoreSource(nameof(BanRm.CategoryId))]
    [MapperIgnoreSource(nameof(BanRm.CreatedById))]
    [MapperIgnoreSource(nameof(BanRm.ModifiedById))]
    public static partial BanViewModel ToViewModel(this BanRm model);

    public static partial IReadOnlyList<BanViewModel> ToViewModels(this IReadOnlyList<BanRm> models);
}
