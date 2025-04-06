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

    public static partial IReadOnlyList<CategoryDetailsViewModel> ToViewModels(this IReadOnlyList<CategoryDashboardModel> models);

    public static partial IReadOnlyList<CategoryDetailsViewModel> ToViewModels(this IReadOnlyList<CategoryDetailsModel> models);
}
