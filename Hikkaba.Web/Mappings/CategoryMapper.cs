using System.Collections.Generic;
using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
internal static partial class CategoryMapper
{
    [MapperIgnoreSource(nameof(CategoryDashboardModel.CreatedBy))]
    [MapperIgnoreSource(nameof(CategoryDashboardModel.ModifiedBy))]
    public static partial CategoryDetailsViewModel ToViewModel(this CategoryDashboardModel model);

    [MapperIgnoreSource(nameof(CategoryDetailsModel.CreatedBy))]
    [MapperIgnoreSource(nameof(CategoryDetailsModel.ModifiedBy))]
    public static partial CategoryDetailsViewModel ToViewModel(this CategoryDetailsModel model);

    [MapperIgnoreSource(nameof(CategoryDetailsModel.CreatedBy))]
    [MapperIgnoreSource(nameof(CategoryDetailsModel.ModifiedBy))]
    [MapperIgnoreSource(nameof(CategoryDetailsModel.BoardId))]
    [MapperIgnoreSource(nameof(CategoryDetailsModel.CreatedAt))]
    [MapperIgnoreSource(nameof(CategoryDetailsModel.ModifiedAt))]
    [MapperIgnoreSource(nameof(CategoryDetailsModel.IsDeleted))]
    public static partial CategoryEditViewModel ToEditViewModel(this CategoryDetailsModel model);

    public static partial IReadOnlyList<CategoryDetailsViewModel> ToViewModels(this IReadOnlyList<CategoryDashboardModel> models);

    public static partial IReadOnlyList<CategoryDetailsViewModel> ToViewModels(this IReadOnlyList<CategoryDetailsModel> models);

    public static partial CategoryCreateRequestModel ToModel(this CategoryCreateViewModel requestModel);

    public static partial CategoryEditRequestModel ToModel(this CategoryEditViewModel requestModel);
}
