using System.Collections.Generic;
using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Infrastructure.Models.ApplicationUser;
using Hikkaba.Web.ViewModels.AdministrationViewModels;
using Hikkaba.Web.ViewModels.UserViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
[UseStaticMapper(typeof(ApplicationUserMapper))]
[UseStaticMapper(typeof(CategoryMapper))]
internal static partial class CategoryModeratorsMapper
{
    public static partial CategoryModeratorsViewModel ToViewModel(this CategoryModeratorsModel model);

    public static partial UserTinyViewModel ToViewModel(this CategoryModeratorModel model);

    public static partial IReadOnlyList<CategoryModeratorsViewModel> ToViewModels(this IReadOnlyList<CategoryModeratorsModel> models);

    public static partial IReadOnlyList<UserTinyViewModel> ToViewModels(this IReadOnlyList<CategoryModeratorModel> models);
}
