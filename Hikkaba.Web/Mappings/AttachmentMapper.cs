using System.Collections.Generic;
using Hikkaba.Infrastructure.Models.Attachments;
using Hikkaba.Web.ViewModels.PostsViewModels.Attachments;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
[UseStaticMapper(typeof(StringBytesMapper))]
public static partial class AttachmentMapper
{
    public static partial AudioViewModel ToViewModel(this AudioDto model);

    public static partial IReadOnlyList<AudioViewModel> ToViewModels(this IReadOnlyList<AudioDto> models);

    public static partial DocumentViewModel ToViewModel(this DocumentDto model);

    public static partial IReadOnlyList<DocumentViewModel> ToViewModels(this IReadOnlyList<DocumentDto> models);

    public static partial NoticeViewModel ToViewModel(this NoticeDto model);

    public static partial IReadOnlyList<NoticeViewModel> ToViewModels(this IReadOnlyList<NoticeDto> models);

    public static partial PictureViewModel ToViewModel(this PictureDto model);

    public static partial IReadOnlyList<PictureViewModel> ToViewModels(this IReadOnlyList<PictureDto> models);

    public static partial VideoViewModel ToViewModel(this VideoDto model);

    public static partial IReadOnlyList<VideoViewModel> ToViewModels(this IReadOnlyList<VideoDto> models);
}
