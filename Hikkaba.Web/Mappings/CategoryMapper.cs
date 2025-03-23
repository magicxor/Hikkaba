using System.Collections.Generic;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
public static partial class CategoryMapper
{
    [MapperIgnoreSource(nameof(CategoryDashboardViewRm.CreatedBy))]
    [MapperIgnoreSource(nameof(CategoryDashboardViewRm.ModifiedBy))]
    public static partial CategoryDetailsViewModel ToViewModel(this CategoryDashboardViewRm model);

    public static partial IReadOnlyList<CategoryDetailsViewModel> ToViewModels(this IReadOnlyList<CategoryDashboardViewRm> models);

    [MapperIgnoreSource(nameof(CategoryDto.CreatedBy))]
    [MapperIgnoreSource(nameof(CategoryDto.ModifiedBy))]
    public static partial CategoryDetailsViewModel ToViewModel(this CategoryDto model);

    public static partial IReadOnlyList<CategoryDetailsViewModel> ToViewModels(this IReadOnlyList<CategoryDto> models);
}
