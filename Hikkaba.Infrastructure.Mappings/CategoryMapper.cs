using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Infrastructure.Models.Category;
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

    [MapperIgnoreTarget(nameof(Category.Id))]
    [MapperIgnoreTarget(nameof(Category.IsDeleted))]
    [MapperIgnoreTarget(nameof(Category.CreatedAt))]
    [MapperIgnoreTarget(nameof(Category.ModifiedAt))]
    [MapperIgnoreTarget(nameof(Category.BoardId))]
    [MapperIgnoreTarget(nameof(Category.CreatedById))]
    [MapperIgnoreTarget(nameof(Category.ModifiedById))]
    [MapperIgnoreTarget(nameof(Category.Board))]
    [MapperIgnoreTarget(nameof(Category.CreatedBy))]
    [MapperIgnoreTarget(nameof(Category.ModifiedBy))]
    [MapperIgnoreTarget(nameof(Category.Threads))]
    [MapperIgnoreTarget(nameof(Category.Moderators))]
    public static partial Category ToEntity(this CategoryCreateRequestModel model);

    [MapperIgnoreTarget(nameof(Category.IsDeleted))]
    [MapperIgnoreTarget(nameof(Category.CreatedAt))]
    [MapperIgnoreTarget(nameof(Category.ModifiedAt))]
    [MapperIgnoreTarget(nameof(Category.BoardId))]
    [MapperIgnoreTarget(nameof(Category.CreatedById))]
    [MapperIgnoreTarget(nameof(Category.ModifiedById))]
    [MapperIgnoreTarget(nameof(Category.Board))]
    [MapperIgnoreTarget(nameof(Category.CreatedBy))]
    [MapperIgnoreTarget(nameof(Category.ModifiedBy))]
    [MapperIgnoreTarget(nameof(Category.Threads))]
    [MapperIgnoreTarget(nameof(Category.Moderators))]
    public static partial void ApplyUpdate([MappingTarget] this Category entity, CategoryEditRequestModel model);
}
