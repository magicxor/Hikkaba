using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.ApplicationUser;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Infrastructure.Mappings;

[Mapper]
public static partial class ApplicationUserMapper
{
    [MapperIgnoreSource(nameof(ApplicationUser.NormalizedUserName))]
    [MapperIgnoreSource(nameof(ApplicationUser.NormalizedEmail))]
    [MapperIgnoreSource(nameof(ApplicationUser.EmailConfirmed))]
    [MapperIgnoreSource(nameof(ApplicationUser.PasswordHash))]
    [MapperIgnoreSource(nameof(ApplicationUser.SecurityStamp))]
    [MapperIgnoreSource(nameof(ApplicationUser.ConcurrencyStamp))]
    [MapperIgnoreSource(nameof(ApplicationUser.PhoneNumber))]
    [MapperIgnoreSource(nameof(ApplicationUser.PhoneNumberConfirmed))]
    [MapperIgnoreSource(nameof(ApplicationUser.TwoFactorEnabled))]
    [MapperIgnoreSource(nameof(ApplicationUser.LockoutEnd))]
    [MapperIgnoreSource(nameof(ApplicationUser.LockoutEnabled))]
    [MapperIgnoreSource(nameof(ApplicationUser.AccessFailedCount))]
    [MapperIgnoreSource(nameof(ApplicationUser.IsDeleted))]
    [MapperIgnoreSource(nameof(ApplicationUser.CreatedAt))]
    [MapperIgnoreSource(nameof(ApplicationUser.CreatedBans))]
    [MapperIgnoreSource(nameof(ApplicationUser.CreatedCategories))]
    [MapperIgnoreSource(nameof(ApplicationUser.CreatedPosts))]
    [MapperIgnoreSource(nameof(ApplicationUser.CreatedNotices))]
    [MapperIgnoreSource(nameof(ApplicationUser.ModifiedBans))]
    [MapperIgnoreSource(nameof(ApplicationUser.ModifiedCategories))]
    [MapperIgnoreSource(nameof(ApplicationUser.ModifiedPosts))]
    [MapperIgnoreSource(nameof(ApplicationUser.ModerationCategories))]
    public static partial UserPreviewModel ToPreview(this ApplicationUser entity);

    public static partial IReadOnlyList<UserPreviewModel> ToPreviews(this IReadOnlyList<ApplicationUser> entities);
}
