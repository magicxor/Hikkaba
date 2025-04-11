using System.Net;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Models.Ban.PostingRestrictions;
using Hikkaba.Infrastructure.Models.Error;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Infrastructure.Repositories.Telemetry;
using Hikkaba.Paging.Extensions;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Constants;
using Hikkaba.Shared.Enums;
using Hikkaba.Shared.Extensions;
using Hikkaba.Shared.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public sealed class BanRepository : IBanRepository
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly TimeProvider _timeProvider;
    private readonly IUserContext _userContext;

    public BanRepository(
        ApplicationDbContext applicationDbContext,
        TimeProvider timeProvider,
        IUserContext userContext)
    {
        _applicationDbContext = applicationDbContext;
        _timeProvider = timeProvider;
        _userContext = userContext;
    }

    public async Task<BanPreviewModel?> FindActiveBanAsync(
        ActiveBanFilter filter,
        CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.BanSource.StartActivity();

        ArgumentNullException.ThrowIfNull(filter.UserIpAddress);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var userIp = filter.UserIpAddress;
        var categoryAlias = filter.CategoryAlias;
        var threadId = filter.ThreadId;

        var query = _applicationDbContext.Bans.TagWithCallSite();

        if (filter is { CategoryAlias: null, ThreadId: null })
        {
            /* search for system-wide bans */
            query = query.Where(ban => ban.Category == null);
        }
        else if (filter is { CategoryAlias: not null, ThreadId: null })
        {
            /* search for system-wide bans AND category bans */
            query = query.Where(ban => ban.Category == null
                                       || ban.Category.Alias == categoryAlias);
        }
        else if (filter is { CategoryAlias: null, ThreadId: not null })
        {
            /* search for system-wide bans AND category bans (lookup category by thread) */
            query = query.Where(ban => ban.Category == null
                                       || ban.Category.Threads.Any(thread => thread.Id == threadId));
        }
        else if (filter is { CategoryAlias: not null, ThreadId: not null })
        {
            /* search for system-wide bans AND category bans (either directly or lookup category by thread) */
            query = query.Where(ban => ban.Category == null
                                       || ban.Category.Alias == categoryAlias
                                       || ban.Category.Threads.Any(thread => thread.Id == threadId));
        }

        var activeBan = await query
            .Where(
                ban => !ban.IsDeleted
                       && ban.EndsAt >= utcNow
                       && (ban.BannedIpAddress == userIp
                           || (ban.BannedCidrLowerIpAddress != null
                               && ban.BannedCidrUpperIpAddress != null
                               && ban.BannedCidrLowerIpAddress.Compare(userIp) <= 0
                               && ban.BannedCidrUpperIpAddress.Compare(userIp) >= 0)))
            .OrderByDescending(ban => ban.EndsAt)
            .Select(ban => new BanPreviewModel
            {
                Id = ban.Id,
                EndsAt = ban.EndsAt,
                Reason = ban.Reason,
            })
            .FirstOrDefaultAsync(cancellationToken);

        return activeBan;
    }

    public async Task<PostingRestrictionsResponseModel> GetPostingRestrictionStatusAsync(
        PostingRestrictionsRequestModel requestModel,
        CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.BanSource.StartActivity();

        var userIp = requestModel.UserIpAddress;
        if (userIp is null)
        {
            return new PostingRestrictionsResponseFailureModel
            {
                RestrictionType = PostingRestrictionType.IpAddressNotFound,
            };
        }

        var category = await _applicationDbContext.Categories
            .TagWithCallSite()
            .Where(c => !c.IsDeleted && c.Alias == requestModel.CategoryAlias)
            .OrderBy(c => c.Id)
            .Select(c => new { c.Id, c.Alias })
            .FirstOrDefaultAsync(cancellationToken);
        if (category is null)
        {
            return new PostingRestrictionsResponseFailureModel
            {
                RestrictionType = PostingRestrictionType.CategoryNotFound,
            };
        }

        Guid? threadSalt = null;
        var threadIsClosed = false;
        var threadIsCyclic = false;
        var threadBumpLimit = 0;
        var threadPostCount = 0;

        if (requestModel.ThreadId is not null)
        {
            var thread = await _applicationDbContext.Threads
                .TagWithCallSite()
                .Include(t => t.Category)
                .Include(t => t.Posts)
                .Where(t => !t.Category.IsDeleted && !t.IsDeleted && t.Id == requestModel.ThreadId)
                .OrderBy(t => t.Id)
                .Select(t => new
                {
                    t.Id,
                    t.Salt,
                    t.IsClosed,
                    t.IsCyclic,
                    BumpLimit = t.BumpLimit > 0 ? t.BumpLimit : t.Category.DefaultBumpLimit,
                    PostCount = t.Posts.Count,
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (thread is null)
            {
                return new PostingRestrictionsResponseFailureModel
                {
                    RestrictionType = PostingRestrictionType.ThreadNotFound,
                };
            }

            var user = _userContext.GetUser();

            if (thread.IsClosed
                && (user == null
                    || (!user.ModeratedCategories.Contains(category.Id)
                        && !user.Roles.Contains(Defaults.AdministratorRoleName))))
            {
                return new PostingRestrictionsResponseFailureModel
                {
                    RestrictionType = PostingRestrictionType.ThreadClosed,
                };
            }

            threadSalt = thread.Salt;
            threadIsClosed = thread.IsClosed;
            threadIsCyclic = thread.IsCyclic;
            threadBumpLimit = thread.BumpLimit;
            threadPostCount = thread.PostCount;
        }

        var ban = await FindActiveBanAsync(new ActiveBanFilter
        {
            UserIpAddress = userIp,
            CategoryAlias = requestModel.CategoryAlias,
            ThreadId = requestModel.ThreadId,
        }, cancellationToken);

        if (ban is not null)
        {
            return new PostingRestrictionsResponseBanModel
            {
                RestrictionType = PostingRestrictionType.IpAddressBanned,
                RestrictionReason = ban.Reason,
                RestrictionEndsAt = ban.EndsAt,
            };
        }

        return new PostingRestrictionsResponseSuccessModel
        {
            RestrictionType = PostingRestrictionType.NoRestriction,
            ThreadSalt = threadSalt,
            IsClosed = threadIsClosed,
            IsCyclic = threadIsCyclic,
            BumpLimit = threadBumpLimit,
            PostCount = threadPostCount,
        };
    }

    public async Task<PagedResult<BanDetailsModel>> ListBansPaginatedAsync(
        BanPagingFilter filter,
        CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.BanSource.StartActivity();

        var query = _applicationDbContext.Bans
            .TagWithCallSite()
            .AsQueryable();

        query = filter.IncludeDeleted
            ? query.IgnoreQueryFilters()
            : query.Where(ban => !ban.IsDeleted);

        if (filter.CreatedNotBefore != null)
        {
            query = query.Where(ban => ban.CreatedAt >= filter.CreatedNotBefore);
        }

        if (filter.CreatedNotAfter != null)
        {
            query = query.Where(ban => ban.CreatedAt <= filter.CreatedNotAfter);
        }

        if (filter.EndsNotBefore != null)
        {
            query = query.Where(ban => ban.EndsAt >= filter.EndsNotBefore);
        }

        if (filter.EndsNotAfter != null)
        {
            query = query.Where(ban => ban.EndsAt <= filter.EndsNotAfter);
        }

        if (filter.IpAddress != null)
        {
            var filterIpAddress = filter.IpAddress.GetAddressBytes();
            query = query.Where(ban => ban.BannedIpAddress == filterIpAddress
                                       || (ban.BannedCidrLowerIpAddress != null
                                           && ban.BannedCidrUpperIpAddress != null
                                           && ban.BannedCidrLowerIpAddress.Compare(filterIpAddress) <= 0
                                           && ban.BannedCidrUpperIpAddress.Compare(filterIpAddress) >= 0));
        }

        if (!string.IsNullOrEmpty(filter.CountryIsoCode))
        {
            query = query.Where(ban => ban.CountryIsoCode == filter.CountryIsoCode);
        }

        if (filter.AutonomousSystemNumber != null)
        {
            query = query.Where(ban => ban.AutonomousSystemNumber == filter.AutonomousSystemNumber);
        }

        if (!string.IsNullOrEmpty(filter.AutonomousSystemOrganization))
        {
            query = query.Where(ban => ban.AutonomousSystemOrganization != null
                                       && ban.AutonomousSystemOrganization.Contains(filter.AutonomousSystemOrganization));
        }

        if (filter.RelatedPostId != null)
        {
            query = query.Where(ban => ban.RelatedPostId == filter.RelatedPostId);
        }

        if (filter.CategoryId != null)
        {
            query = query.Where(ban => ban.CategoryId == filter.CategoryId);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var data = await query
            .ApplyOrderByAndPaging(filter, x => x.CreatedAt)
            .Select(ban => new BanDetailsModel
            {
                Id = ban.Id,
                IsDeleted = ban.IsDeleted,
                CreatedAt = ban.CreatedAt,
                ModifiedAt = ban.ModifiedAt,
                EndsAt = ban.EndsAt,
                IpAddressType = ban.IpAddressType,
                BannedIpAddress = ban.BannedIpAddress,
                BannedCidrLowerIpAddress = ban.BannedCidrLowerIpAddress,
                BannedCidrUpperIpAddress = ban.BannedCidrUpperIpAddress,
                CountryIsoCode = ban.CountryIsoCode,
                AutonomousSystemNumber = ban.AutonomousSystemNumber,
                AutonomousSystemOrganization = ban.AutonomousSystemOrganization,
                Reason = ban.Reason,
                CategoryAlias = ban.Category != null ? ban.Category.Alias : null,
                RelatedThreadId = ban.RelatedPost != null ? ban.RelatedPost.ThreadId : null,
                RelatedPostId = ban.RelatedPostId,
                CategoryId = ban.CategoryId,
                CreatedById = ban.CreatedById,
                ModifiedById = ban.ModifiedById,
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<BanDetailsModel>(data, filter, totalCount);
    }

    public async Task<BanDetailsModel?> GetBanAsync(int banId, CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.BanSource.StartActivity();

        var ban = await _applicationDbContext.Bans
            .TagWithCallSite()
            .Where(ban => ban.Id == banId)
            .Select(ban => new BanDetailsModel
            {
                Id = ban.Id,
                IsDeleted = ban.IsDeleted,
                CreatedAt = ban.CreatedAt,
                ModifiedAt = ban.ModifiedAt,
                EndsAt = ban.EndsAt,
                IpAddressType = ban.IpAddressType,
                BannedIpAddress = ban.BannedIpAddress,
                BannedCidrLowerIpAddress = ban.BannedCidrLowerIpAddress,
                BannedCidrUpperIpAddress = ban.BannedCidrUpperIpAddress,
                CountryIsoCode = ban.CountryIsoCode,
                AutonomousSystemNumber = ban.AutonomousSystemNumber,
                AutonomousSystemOrganization = ban.AutonomousSystemOrganization,
                Reason = ban.Reason,
                CategoryAlias = ban.Category != null ? ban.Category.Alias : null,
                RelatedThreadId = ban.RelatedPost != null ? ban.RelatedPost.ThreadId : null,
                RelatedPostId = ban.RelatedPostId,
                CategoryId = ban.CategoryId,
                CreatedById = ban.CreatedById,
                ModifiedById = ban.ModifiedById,
            })
            .OrderBy(ban => ban.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return ban;
    }

    public async Task<BanCreateResultModel> CreateBanAsync(
        BanCreateRequestModel requestModel,
        CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.BanSource.StartActivity();

        var user = _userContext.GetRequiredUser();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var relatedPostId = requestModel.RelatedPostId;
        var isBannedBySubnet = requestModel is { BannedCidrLowerIpAddress: not null, BannedCidrUpperIpAddress: not null };

        // user can't be banned twice for one post
        if (relatedPostId != null)
        {
            var banExists = await _applicationDbContext.Bans
                .TagWithCallSite()
                .AnyAsync(ban => !ban.IsDeleted && ban.RelatedPostId == relatedPostId, cancellationToken);
            if (banExists)
            {
                return new DomainError
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    ErrorMessage = "User was already banned for this post",
                };
            }
        }

        var categoryId = await _applicationDbContext.Categories
            .TagWithCallSite()
            .Where(c => c.Alias == requestModel.CategoryAlias)
            .Select(c => c.Id)
            .OrderBy(id => id)
            .FirstOrDefaultAsync(cancellationToken);

        var ban = new Ban
        {
            CreatedAt = utcNow,
            EndsAt = requestModel.EndsAt,
            IpAddressType = requestModel.IpAddressType,
            BannedIpAddress = requestModel.BannedIpAddress,
            BannedCidrLowerIpAddress = requestModel.BannedCidrLowerIpAddress,
            BannedCidrUpperIpAddress = requestModel.BannedCidrUpperIpAddress,
            CountryIsoCode = requestModel.CountryIsoCode,
            AutonomousSystemNumber = requestModel.AutonomousSystemNumber,
            AutonomousSystemOrganization = requestModel.AutonomousSystemOrganization,
            Reason = requestModel.Reason,
            RelatedPostId = requestModel.RelatedPostId,
            CategoryId = requestModel.BanInAllCategories ? null : categoryId,
            CreatedById = user.Id,
        };

        List<Post> postsToBeDeleted = [];

        if (requestModel is { AdditionalAction: BanAdditionalAction.DeletePost, RelatedPostId: not null })
        {
            postsToBeDeleted = await _applicationDbContext.Posts
                .Where(p => p.ThreadId == requestModel.RelatedThreadId
                            && p.Id == requestModel.RelatedPostId)
                .ToListAsync(cancellationToken);
        }
        else if (requestModel.AdditionalAction == BanAdditionalAction.DeleteAllPostsInThread && !isBannedBySubnet)
        {
            postsToBeDeleted = await _applicationDbContext.Posts
                .Where(p => p.ThreadId == requestModel.RelatedThreadId
                            && (p.Id == requestModel.RelatedPostId
                                || p.UserIpAddress == requestModel.BannedIpAddress))
                .ToListAsync(cancellationToken);
        }
        else if (requestModel.AdditionalAction == BanAdditionalAction.DeleteAllPostsInThread && isBannedBySubnet)
        {
            postsToBeDeleted = await _applicationDbContext.Posts
                .Where(p => p.ThreadId == requestModel.RelatedThreadId
                            && (p.Id == requestModel.RelatedPostId
                                || (ban.BannedCidrLowerIpAddress != null
                                   && ban.BannedCidrUpperIpAddress != null
                                   && p.UserIpAddress != null
                                   && ban.BannedCidrLowerIpAddress.Compare(p.UserIpAddress) <= 0
                                   && ban.BannedCidrUpperIpAddress.Compare(p.UserIpAddress) >= 0)))
                .ToListAsync(cancellationToken);
        }
        else if (requestModel.AdditionalAction == BanAdditionalAction.DeleteAllPostsInCategory && !isBannedBySubnet)
        {
            postsToBeDeleted = await _applicationDbContext.Posts
                .Where(p => p.Thread.CategoryId == categoryId
                            && (p.Id == requestModel.RelatedPostId
                                || p.UserIpAddress == requestModel.BannedIpAddress))
                .ToListAsync(cancellationToken);
        }
        else if (requestModel.AdditionalAction == BanAdditionalAction.DeleteAllPostsInCategory && isBannedBySubnet)
        {
            postsToBeDeleted = await _applicationDbContext.Posts
                .Where(p => p.Thread.CategoryId == categoryId
                            && (p.Id == requestModel.RelatedPostId
                                || (ban.BannedCidrLowerIpAddress != null
                                   && ban.BannedCidrUpperIpAddress != null
                                   && p.UserIpAddress != null
                                   && ban.BannedCidrLowerIpAddress.Compare(p.UserIpAddress) <= 0
                                   && ban.BannedCidrUpperIpAddress.Compare(p.UserIpAddress) >= 0)))
                .ToListAsync(cancellationToken);
        }
        else if (requestModel.AdditionalAction == BanAdditionalAction.DeleteAllPosts && !isBannedBySubnet)
        {
            postsToBeDeleted = await _applicationDbContext.Posts
                .Where(p => p.UserIpAddress == requestModel.BannedIpAddress)
                .ToListAsync(cancellationToken);
        }
        else if (requestModel.AdditionalAction == BanAdditionalAction.DeleteAllPosts && isBannedBySubnet)
        {
            postsToBeDeleted = await _applicationDbContext.Posts
                .Where(p => ban.BannedCidrLowerIpAddress != null
                            && ban.BannedCidrUpperIpAddress != null
                            && p.UserIpAddress != null
                            && ban.BannedCidrLowerIpAddress.Compare(p.UserIpAddress) <= 0
                            && ban.BannedCidrUpperIpAddress.Compare(p.UserIpAddress) >= 0)
                .ToListAsync(cancellationToken);
        }

        foreach (var postToBeDeleted in postsToBeDeleted)
        {
            postToBeDeleted.IsDeleted = true;
        }

        await _applicationDbContext.Bans.AddAsync(ban, cancellationToken);
        await _applicationDbContext.SaveChangesAsync(cancellationToken);

        return new BanCreateResultSuccessModel { BanId = ban.Id };
    }

    public async Task SetBanDeletedAsync(int banId, bool isDeleted, CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.BanSource.StartActivity();

        var user = _userContext.GetRequiredUser();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var ban = await _applicationDbContext.Bans
            .TagWithCallSite()
            .OrderBy(ban => ban.Id)
            .FirstAsync(ban => ban.Id == banId, cancellationToken);

        ban.IsDeleted = isDeleted;
        ban.ModifiedAt = utcNow;
        ban.ModifiedById = user.Id;

        await _applicationDbContext.SaveChangesAsync(cancellationToken);
    }
}
