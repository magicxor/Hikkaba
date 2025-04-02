using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Administration;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Infrastructure.Mappings;

[Mapper]
[UseStaticMapper(typeof(ApplicationUserMapper))]
public static partial class CategoryMapper
{
    [MapProperty(nameof(Category.Id), nameof(CategoryDashboardModel.Id))]
    [MapProperty(nameof(Category.IsDeleted), nameof(CategoryDashboardModel.IsDeleted))]
    [MapProperty(nameof(Category.CreatedAt), nameof(CategoryDashboardModel.CreatedAt))]
    [MapProperty(nameof(Category.ModifiedAt), nameof(CategoryDashboardModel.ModifiedAt))]
    [MapProperty(nameof(Category.Alias), nameof(CategoryDashboardModel.Alias))]
    [MapProperty(nameof(Category.Name), nameof(CategoryDashboardModel.Name))]
    [MapProperty(nameof(Category.IsHidden), nameof(CategoryDashboardModel.IsHidden))]
    [MapProperty(nameof(Category.DefaultBumpLimit), nameof(CategoryDashboardModel.DefaultBumpLimit))]
    [MapProperty(nameof(Category.ShowThreadLocalUserHash), nameof(CategoryDashboardModel.ShowThreadLocalUserHash))]
    [MapProperty(nameof(Category.BoardId), nameof(CategoryDashboardModel.BoardId))]
    [MapperIgnoreSource(nameof(Category.CreatedById))]
    [MapperIgnoreSource(nameof(Category.ModifiedById))]
    [MapperIgnoreSource(nameof(Category.Board))]
    [MapperIgnoreSource(nameof(Category.Threads))]
    [MapperIgnoreSource(nameof(Category.Moderators))]
    public static partial CategoryDashboardModel ToDashboardModel(this Category entity);
}
