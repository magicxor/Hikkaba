using System;
using System.Collections.Generic;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
[UseStaticMapper(typeof(AttachmentMapper))]
[UseStaticMapper(typeof(Hikkaba.Infrastructure.Mappings.IpAddressMapper))]
internal static partial class PostMapper
{
    [UserMapping]
    public static string BytesToString(byte[] src) => Convert.ToHexStringLower(src);

    [MapProperty(nameof(PostDetailsModel.ThreadLocalUserHash), nameof(PostDetailsViewModel.ThreadLocalUserHash), Use = nameof(BytesToString))]
    [MapperIgnoreTarget(nameof(PostDetailsViewModel.ShowCategoryAlias))]
    public static partial PostDetailsViewModel ToViewModel(this PostDetailsModel model);

    public static partial IReadOnlyList<PostDetailsViewModel> ToViewModels(this IReadOnlyList<PostDetailsModel> models);
}
