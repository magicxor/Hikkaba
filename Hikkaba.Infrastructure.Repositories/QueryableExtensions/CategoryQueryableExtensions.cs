using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Infrastructure.Models.User;

namespace Hikkaba.Infrastructure.Repositories.QueryableExtensions;

public static class CategoryQueryableExtensions
{
    public static IQueryable<CategoryDetailsModel> GetDetailsModel(this IQueryable<Data.Entities.Category> query)
    {
        return query.Select(x => new CategoryDetailsModel
        {
            Id = x.Id,
            IsDeleted = x.IsDeleted,
            CreatedBy = new UserPreviewModel
            {
                Id = x.CreatedBy.Id,
                UserName = x.CreatedBy.UserName ?? string.Empty,
                Email = x.CreatedBy.Email ?? string.Empty,
                LastLoginAt = x.CreatedBy.LastLoginAt,
            },
            ModifiedBy = x.ModifiedBy == null
                ? null
                : new UserPreviewModel
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
            ShowThreadLocalUserHash = x.ShowThreadLocalUserHash,
            ShowCountry = x.ShowCountry,
            ShowOs = x.ShowOs,
            ShowBrowser = x.ShowBrowser,
            MaxThreadCount = x.MaxThreadCount,
            BoardId = x.BoardId,
        });
    }
}
