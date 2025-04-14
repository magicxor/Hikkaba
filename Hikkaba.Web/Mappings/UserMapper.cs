using System.Collections.Generic;
using Hikkaba.Infrastructure.Models.User;
using Hikkaba.Web.ViewModels.AdministrationViewModels;
using Hikkaba.Web.ViewModels.UserViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
[UseStaticMapper(typeof(RoleMapper))]
internal static partial class UserMapper
{
    public static partial UserDetailsViewModel ToViewModel(this UserDetailsModel model);

    public static partial IReadOnlyList<UserDetailsViewModel> ToViewModels(this IReadOnlyList<UserDetailsModel> models);

    [MapperIgnoreSource(nameof(UserCreateViewModel.AllExistingRoles))]
    public static partial UserCreateRequestModel ToCreateModel(this UserCreateViewModel model);

    [MapperIgnoreSource(nameof(UserEditViewModel.AllExistingRoles))]
    public static partial UserEditRequestModel ToEditModel(this UserEditViewModel model);

    [MapperIgnoreSource(nameof(UserDetailsModel.IsDeleted))]
    [MapperIgnoreSource(nameof(UserDetailsModel.AccessFailedCount))]
    [MapperIgnoreSource(nameof(UserDetailsModel.EmailConfirmed))]
    [MapperIgnoreSource(nameof(UserDetailsModel.LastLogin))]
    [MapperIgnoreSource(nameof(UserDetailsModel.LockoutEnabled))]
    [MapperIgnoreSource(nameof(UserDetailsModel.PhoneNumber))]
    [MapperIgnoreSource(nameof(UserDetailsModel.PhoneNumberConfirmed))]
    [MapperIgnoreTarget(nameof(UserEditViewModel.AllExistingRoles))]
    [MapProperty(nameof(UserDetailsModel.UserRoles), nameof(UserEditViewModel.UserRoleIds))]
    public static partial UserEditViewModel ToEditViewModel(this UserDetailsModel model);
}
