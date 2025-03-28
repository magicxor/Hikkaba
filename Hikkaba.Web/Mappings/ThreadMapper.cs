using System.Collections.Generic;
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
    public static partial ThreadAggregationViewModel ToViewModel(this ThreadAggregationRm model);

    public static partial ThreadDetailsViewModel ToViewModel(this ThreadDetailsRm model);

    public static partial IReadOnlyList<ThreadAggregationViewModel> ToViewModels(this IReadOnlyList<ThreadAggregationRm> models);

    [MapperIgnoreSource(nameof(ThreadPreviewRm.LastPostCreatedAt))]
    public static partial ThreadDetailsViewModel ToViewModel(this ThreadPreviewRm model);

    public static partial IReadOnlyList<ThreadDetailsViewModel> ToViewModels(this IReadOnlyList<ThreadPreviewRm> models);
}
