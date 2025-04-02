using System;
using System.Collections.Generic;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
[UseStaticMapper(typeof(AttachmentMapper))]
[UseStaticMapper(typeof(Hikkaba.Infrastructure.Mappings.IpAddressMapper))]
public static partial class PostMapper
{
    [UserMapping]
    public static string BytesToString(byte[] src) => Convert.ToHexStringLower(src);

    [MapProperty(nameof(PostDetailsModel.ThreadLocalUserHash), nameof(PostDetailsViewModel.ThreadLocalUserHash), Use = nameof(BytesToString))]
    public static partial PostDetailsViewModel ToViewModel(this PostDetailsModel model);

    [MapperIgnoreSource(nameof(PostPreviewModel.CreatedBy))]
    [MapperIgnoreSource(nameof(PostPreviewModel.ModifiedBy))]
    [MapProperty(nameof(PostPreviewModel.ThreadLocalUserHash), nameof(PostDetailsViewModel.ThreadLocalUserHash), Use = nameof(BytesToString))]
    public static partial PostDetailsViewModel ToViewModel(this PostPreviewModel model);

    public static partial IReadOnlyList<PostDetailsViewModel> ToViewModels(this IReadOnlyList<PostDetailsModel> models);
}
