using System.Collections.Generic;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
[UseStaticMapper(typeof(AttachmentMapper))]
[UseStaticMapper(typeof(Repositories.Mappings.IpAddressMapper))]
public static partial class PostMapper
{
    public static partial PostDetailsViewModel ToViewModel(this PostViewRm model);

    [MapperIgnoreSource(nameof(PostPreviewRm.CreatedBy))]
    [MapperIgnoreSource(nameof(PostPreviewRm.ModifiedBy))]
    public static partial PostDetailsViewModel ToViewModel(this PostPreviewRm model);

    public static partial IReadOnlyList<PostDetailsViewModel> ToViewModels(this IReadOnlyList<PostViewRm> models);
}
