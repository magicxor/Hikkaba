using System.Collections.Generic;
using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Web.ViewModels.AdministrationViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
[UseStaticMapper(typeof(ApplicationUserMapper))]
[UseStaticMapper(typeof(CategoryMapper))]
internal static partial class CategoryModeratorsMapper
{
    public static partial CategoryModeratorsViewModel ToViewModel(this CategoryModeratorsModel model);

    public static partial IReadOnlyList<CategoryModeratorsViewModel> ToViewModels(this IReadOnlyList<CategoryModeratorsModel> models);
}
