using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Category;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Repositories.Mappings;

[Mapper]
[UseStaticMapper(typeof(ApplicationUserMapper))]
public static partial class CategoryMapper
{
    [MapProperty(nameof(Category.Id), nameof(CategoryDashboardViewRm.Id))]
    [MapProperty(nameof(Category.IsDeleted), nameof(CategoryDashboardViewRm.IsDeleted))]
    [MapProperty(nameof(Category.CreatedAt), nameof(CategoryDashboardViewRm.CreatedAt))]
    [MapProperty(nameof(Category.ModifiedAt), nameof(CategoryDashboardViewRm.ModifiedAt))]
    [MapProperty(nameof(Category.Alias), nameof(CategoryDashboardViewRm.Alias))]
    [MapProperty(nameof(Category.Name), nameof(CategoryDashboardViewRm.Name))]
    [MapProperty(nameof(Category.IsHidden), nameof(CategoryDashboardViewRm.IsHidden))]
    [MapProperty(nameof(Category.DefaultBumpLimit), nameof(CategoryDashboardViewRm.DefaultBumpLimit))]
    [MapProperty(nameof(Category.DefaultShowThreadLocalUserHash), nameof(CategoryDashboardViewRm.DefaultShowThreadLocalUserHash))]
    [MapProperty(nameof(Category.BoardId), nameof(CategoryDashboardViewRm.BoardId))]
    [MapperIgnoreSource(nameof(Category.CreatedById))]
    [MapperIgnoreSource(nameof(Category.ModifiedById))]
    [MapperIgnoreSource(nameof(Category.Board))]
    [MapperIgnoreSource(nameof(Category.Threads))]
    [MapperIgnoreSource(nameof(Category.Moderators))]
    public static partial CategoryDashboardViewRm ToDashboardViewRm(this Category entity);
}
