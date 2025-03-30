using System.Collections.Generic;
using Hikkaba.Infrastructure.Models.Attachments.Concrete;
using Hikkaba.Web.ViewModels.PostsViewModels.Attachments;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
[UseStaticMapper(typeof(StringBytesMapper))]
public static partial class AttachmentMapper
{
    public static partial AudioViewModel ToViewModel(this AudioModel model);

    public static partial IReadOnlyList<AudioViewModel> ToViewModels(this IReadOnlyList<AudioModel> models);

    public static partial DocumentViewModel ToViewModel(this DocumentModel model);

    public static partial IReadOnlyList<DocumentViewModel> ToViewModels(this IReadOnlyList<DocumentModel> models);

    public static partial NoticeViewModel ToViewModel(this NoticeModel model);

    public static partial IReadOnlyList<NoticeViewModel> ToViewModels(this IReadOnlyList<NoticeModel> models);

    public static partial PictureViewModel ToViewModel(this PictureModel model);

    public static partial IReadOnlyList<PictureViewModel> ToViewModels(this IReadOnlyList<PictureModel> models);

    public static partial VideoViewModel ToViewModel(this VideoModel model);

    public static partial IReadOnlyList<VideoViewModel> ToViewModels(this IReadOnlyList<VideoModel> models);
}
