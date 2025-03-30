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
    public static partial ThreadDetailsViewModel ToViewModel(this ThreadDetailsRequestModel model);

    [MapperIgnoreSource(nameof(ThreadPreviewModel.LastPostCreatedAt))]
    public static partial ThreadDetailsViewModel ToViewModel(this ThreadPreviewModel model);

    public static partial IReadOnlyList<ThreadDetailsViewModel> ToViewModels(this IReadOnlyList<ThreadPreviewModel> models);
}
