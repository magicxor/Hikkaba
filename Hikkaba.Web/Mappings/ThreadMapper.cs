using System.Collections.Generic;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Web.ViewModels.ThreadsViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
[UseStaticMapper(typeof(CategoryMapper))]
[UseStaticMapper(typeof(PostMapper))]
internal static partial class ThreadMapper
{
    public static partial ThreadDetailsViewModel ToViewModel(this ThreadDetailsRequestModel model);

    [MapperIgnoreSource(nameof(ThreadPreviewModel.LastBumpAt))]
    public static partial ThreadDetailsViewModel ToViewModel(this ThreadPreviewModel model);

    public static partial IReadOnlyList<ThreadDetailsViewModel> ToViewModels(this IReadOnlyList<ThreadPreviewModel> models);

    [MapperIgnoreSource(nameof(ThreadDetailsRequestModel.IsDeleted))]
    [MapperIgnoreSource(nameof(ThreadDetailsRequestModel.CreatedAt))]
    [MapperIgnoreSource(nameof(ThreadDetailsRequestModel.ModifiedAt))]
    [MapperIgnoreSource(nameof(ThreadDetailsRequestModel.IsPinned))]
    [MapperIgnoreSource(nameof(ThreadDetailsRequestModel.IsClosed))]
    [MapperIgnoreSource(nameof(ThreadDetailsRequestModel.IsCyclic))]
    [MapperIgnoreSource(nameof(ThreadDetailsRequestModel.ShowThreadLocalUserHash))]
    [MapperIgnoreSource(nameof(ThreadDetailsRequestModel.CategoryId))]
    [MapperIgnoreSource(nameof(ThreadDetailsRequestModel.CategoryName))]
    [MapperIgnoreSource(nameof(ThreadDetailsRequestModel.PostCount))]
    [MapperIgnoreSource(nameof(ThreadDetailsRequestModel.Posts))]
    public static partial ThreadEditViewModel ToEditViewModel(this ThreadDetailsRequestModel model);

    [MapperIgnoreSource(nameof(ThreadEditViewModel.CategoryAlias))]
    public static partial ThreadEditRequestModel ToEditRequestModel(this ThreadEditViewModel model);
}
