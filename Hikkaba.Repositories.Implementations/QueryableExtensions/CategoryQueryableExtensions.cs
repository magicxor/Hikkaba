using Hikkaba.Infrastructure.Models.ApplicationUser;
using Hikkaba.Infrastructure.Models.Category;

namespace Hikkaba.Repositories.Implementations.QueryableExtensions;

public static class CategoryQueryableExtensions
{
    public static IQueryable<CategoryViewRm> GetViewRm(this IQueryable<Data.Entities.Category> query)
    {
        return query.Select(x => new CategoryViewRm
        {
            Id = x.Id,
            IsDeleted = x.IsDeleted,
            CreatedBy = new ApplicationUserPreviewRm
            {
                Id = x.CreatedBy.Id,
                UserName = x.CreatedBy.UserName ?? string.Empty,
                Email = x.CreatedBy.Email ?? string.Empty,
                LastLoginAt = x.CreatedBy.LastLoginAt,
            },
            ModifiedBy = x.ModifiedBy == null
                ? null
                : new ApplicationUserPreviewRm
                {
                    Id = x.ModifiedBy.Id,
                    UserName = x.ModifiedBy.UserName ?? string.Empty,
                    Email = x.ModifiedBy.Email ?? string.Empty,
                    LastLoginAt = x.ModifiedBy.LastLoginAt,
                },
            CreatedAt = x.CreatedAt,
            ModifiedAt = x.ModifiedAt,
            Alias = x.Alias,
            Name = x.Name,
            IsHidden = x.IsHidden,
            DefaultBumpLimit = x.DefaultBumpLimit,
            DefaultShowThreadLocalUserHash = x.DefaultShowThreadLocalUserHash,
            BoardId = x.BoardId,
        });
    }
}
