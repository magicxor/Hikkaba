using System.Collections.Generic;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
[UseStaticMapper(typeof(AttachmentMapper))]
[UseStaticMapper(typeof(IpAddressMapper))]
public static partial class PostMapper
{
    public static partial PostDetailsViewModel ToViewModel(this PostInfoRm model);

    [MapperIgnoreSource(nameof(PostPreviewRm.CreatedBy))]
    [MapperIgnoreSource(nameof(PostPreviewRm.ModifiedBy))]
    public static partial PostDetailsViewModel ToViewModel(this PostPreviewRm model);

    public static partial IReadOnlyList<PostDetailsViewModel> ToViewModels(this IReadOnlyList<PostInfoRm> models);

    public static partial PostDetailsViewModel ToViewModel(this PostPreviewDto model);
}
