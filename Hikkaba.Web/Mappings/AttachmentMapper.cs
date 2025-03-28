using System.Collections.Generic;
using Hikkaba.Infrastructure.Models.Attachments;
using Hikkaba.Web.ViewModels.PostsViewModels.Attachments;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
[UseStaticMapper(typeof(StringBytesMapper))]
public static partial class AttachmentMapper
{
    public static partial AudioViewModel ToViewModel(this AudioViewRm model);

    public static partial IReadOnlyList<AudioViewModel> ToViewModels(this IReadOnlyList<AudioViewRm> models);

    public static partial DocumentViewModel ToViewModel(this DocumentViewRm model);

    public static partial IReadOnlyList<DocumentViewModel> ToViewModels(this IReadOnlyList<DocumentViewRm> models);

    public static partial NoticeViewModel ToViewModel(this NoticeViewRm model);

    public static partial IReadOnlyList<NoticeViewModel> ToViewModels(this IReadOnlyList<NoticeViewRm> models);

    public static partial PictureViewModel ToViewModel(this PictureViewRm model);

    public static partial IReadOnlyList<PictureViewModel> ToViewModels(this IReadOnlyList<PictureViewRm> models);

    public static partial VideoViewModel ToViewModel(this VideoViewRm model);

    public static partial IReadOnlyList<VideoViewModel> ToViewModels(this IReadOnlyList<VideoViewRm> models);
}
