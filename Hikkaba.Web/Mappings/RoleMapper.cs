using System.Collections.Generic;
using System.Linq;
using Hikkaba.Infrastructure.Models.Role;
using Hikkaba.Web.ViewModels.AdministrationViewModels;
using Hikkaba.Web.ViewModels.RoleViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
internal static partial class RoleMapper
{
    [MapperIgnoreSource(nameof(RoleModel.NormalizedName))]
    public static partial RoleSlimViewModel ToViewModel(this RoleModel model);

    [UserMapping]
    public static IReadOnlyList<int> ToRoleIds(this IReadOnlyList<RoleModel> roles) => roles
        .Select(r => r.Id)
        .ToList()
        .AsReadOnly();
}
