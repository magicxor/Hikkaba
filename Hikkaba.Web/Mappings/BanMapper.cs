using System.Collections.Generic;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Web.ViewModels.BansViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
[UseStaticMapper(typeof(Repositories.Mappings.IpAddressMapper))]
public static partial class BanMapper
{
    public static partial BanPreviewModel ToViewModel(this BanPreviewRm model);

    [MapperIgnoreSource(nameof(BanViewRm.CategoryId))]
    [MapperIgnoreSource(nameof(BanViewRm.CreatedById))]
    [MapperIgnoreSource(nameof(BanViewRm.ModifiedById))]
    public static partial BanViewModel ToViewModel(this BanViewRm model);

    public static partial IReadOnlyList<BanViewModel> ToViewModels(this IReadOnlyList<BanViewRm> models);
}
