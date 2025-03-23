using System.Collections.Generic;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Web.ViewModels.ThreadsViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
[UseStaticMapper(typeof(BoardMapper))]
[UseStaticMapper(typeof(CategoryMapper))]
[UseStaticMapper(typeof(PostMapper))]
public static partial class ThreadMapper
{
    public static partial ThreadAggregationViewModel ToViewModel(this ThreadAggregationSm model);

    public static partial IReadOnlyList<ThreadAggregationViewModel> ToViewModels(this IReadOnlyList<ThreadAggregationSm> models);

    [MapperIgnoreSource(nameof(ThreadPreviewSm.LastPostCreatedAt))]
    public static partial ThreadDetailsViewModel ToViewModel(this ThreadPreviewSm model);

    public static partial IReadOnlyList<ThreadDetailsViewModel> ToViewModels(this IReadOnlyList<ThreadPreviewSm> models);
}
