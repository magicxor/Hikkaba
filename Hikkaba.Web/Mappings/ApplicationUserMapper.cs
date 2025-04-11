using Hikkaba.Infrastructure.Models.ApplicationUser;
using Hikkaba.Web.ViewModels.AdministrationViewModels;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Web.Mappings;

[Mapper]
internal static partial class ApplicationUserMapper
{
    public static partial ApplicationUserViewModel ToViewModel(this UserPreviewModel model);
}
