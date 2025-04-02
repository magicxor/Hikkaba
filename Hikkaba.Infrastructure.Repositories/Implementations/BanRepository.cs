using System.Net;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Models.Ban.PostingRestrictions;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Infrastructure.Repositories.Telemetry;
using Hikkaba.Paging.Extensions;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Enums;
using Hikkaba.Shared.Exceptions;
using Hikkaba.Shared.Extensions;
using Hikkaba.Shared.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public sealed class BanRepository : IBanRepository
{
    private readonly ILogger<BanRepository> _logger;
    private readonly IOptions<HikkabaConfiguration> _settings;
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly TimeProvider _timeProvider;
    private readonly IUserContext _userContext;

    public BanRepository(
        ILogger<BanRepository> logger,
        IOptions<HikkabaConfiguration> settings,
        ApplicationDbContext applicationDbContext,
        TimeProvider timeProvider,
        IUserContext userContext)
    {
        _logger = logger;
        _settings = settings;
        _applicationDbContext = applicationDbContext;
        _timeProvider = timeProvider;
        _userContext = userContext;
    }

    public async Task<BanPreviewModel?> FindActiveBan(long? threadId, string? categoryAlias, string userIpAddress)
    {
        using var activity = RepositoriesTelemetry.BanSource.StartActivity();

        var userIp = IPAddress.Parse(userIpAddress).GetAddressBytes();
        return await FindActiveBan(threadId, categoryAlias, userIp);
    }

    public async Task<BanPreviewModel?> FindActiveBan(long? threadId, string? categoryAlias, byte[] userIpAddress)
    {
        using var activity = RepositoriesTelemetry.BanSource.StartActivity();

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var userIp = userIpAddress;

        var activeBan = await _applicationDbContext.Bans
            .TagWithCallSite()
            .Where(
                ban =>
                    (ban.Category == null
                     || (!string.IsNullOrEmpty(categoryAlias) && ban.Category.Alias == categoryAlias)
                     || (threadId != null && ban.Category.Threads.Any(thread => thread.Id == threadId)))
                    && !ban.IsDeleted
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
            .FirstOrDefaultAsync();

        return activeBan;
    }

    public async Task<PostingRestrictionsResponseModel> GetPostingRestrictionStatusAsync(PostingRestrictionsRequestModel restrictionsRequestModel)
    {
        using var activity = RepositoriesTelemetry.BanSource.StartActivity();

        var userIp = restrictionsRequestModel.UserIpAddress;
        if (userIp is null)
        {
            return new PostingRestrictionsResponseFailureModel
            {
                RestrictionType = PostingRestrictionType.IpAddressNotFound,
            };
        }

        var category = await _applicationDbContext.Categories
            .TagWithCallSite()
            .Where(c => !c.IsDeleted && c.Alias == restrictionsRequestModel.CategoryAlias)
            .OrderBy(c => c.Id)
            .Select(c => new { c.Id, c.Alias })
            .FirstOrDefaultAsync();
        if (category is null)
        {
            return new PostingRestrictionsResponseFailureModel
            {
                RestrictionType = PostingRestrictionType.CategoryNotFound,
            };
        }

        Guid? threadSalt = null;
        var threadIsCyclic = false;
        var threadBumpLimit = 0;
        var threadPostCount = 0;

        if (restrictionsRequestModel.ThreadId is not null)
        {
            var thread = await _applicationDbContext.Threads
                .TagWithCallSite()
                .Include(t => t.Category)
                .Include(t => t.Posts)
                .Where(t => !t.Category.IsDeleted && !t.IsDeleted && t.Id == restrictionsRequestModel.ThreadId)
                .OrderBy(t => t.Id)
                .Select(t => new
                {
                    t.Id,
                    t.IsClosed,
                    t.Salt,
                    t.IsCyclic,
                    BumpLimit = t.BumpLimit > 0 ? t.BumpLimit : t.Category.DefaultBumpLimit,
                    PostCount = t.Posts.Count(p => !p.IsDeleted),
                })
                .FirstOrDefaultAsync();
            if (thread is null)
            {
                return new PostingRestrictionsResponseFailureModel
                {
                    RestrictionType = PostingRestrictionType.ThreadNotFound,
                };
            }

            if (thread.IsClosed)
            {
                return new PostingRestrictionsResponseFailureModel
                {
                    RestrictionType = PostingRestrictionType.ThreadClosed,
                };
            }

            threadSalt = thread.Salt;
            threadIsCyclic = thread.IsCyclic;
            threadBumpLimit = thread.BumpLimit;
            threadPostCount = thread.PostCount;
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var utcFiveMinutesAgo = utcNow.AddMinutes(-5);
        var postsFromIpWithin5MinutesCount = await _applicationDbContext.Posts
            .TagWithCallSite()
            .IgnoreQueryFilters()
            .Where(p => p.UserIpAddress == userIp && p.CreatedAt >= utcFiveMinutesAgo)
            .CountAsync();
        if (postsFromIpWithin5MinutesCount >= _settings.Value.MaxPostsFromIpWithin5Minutes)
        {
            return new PostingRestrictionsResponseFailureModel
            {
                RestrictionType = PostingRestrictionType.RateLimitExceeded,
            };
        }

        var ban = await FindActiveBan(restrictionsRequestModel.ThreadId, restrictionsRequestModel.CategoryAlias, userIp);
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
            IsCyclic = threadIsCyclic,
            BumpLimit = threadBumpLimit,
            PostCount = threadPostCount,
        };
    }

    public async Task<PagedResult<BanDetailsModel>> ListBansPaginatedAsync(BanPagingFilter banFilter)
    {
        using var activity = RepositoriesTelemetry.BanSource.StartActivity();

        var query = _applicationDbContext.Bans
            .TagWithCallSite()
            .AsQueryable();

        query = banFilter.IncludeDeleted
            ? query.IgnoreQueryFilters()
            : query.Where(ban => !ban.IsDeleted);

        if (banFilter.CreatedNotBefore != null)
        {
            query = query.Where(ban => ban.CreatedAt >= banFilter.CreatedNotBefore);
        }

        if (banFilter.CreatedNotAfter != null)
        {
            query = query.Where(ban => ban.CreatedAt <= banFilter.CreatedNotAfter);
        }

        if (banFilter.EndsNotBefore != null)
        {
            query = query.Where(ban => ban.EndsAt >= banFilter.EndsNotBefore);
        }

        if (banFilter.EndsNotAfter != null)
        {
            query = query.Where(ban => ban.EndsAt <= banFilter.EndsNotAfter);
        }

        if (banFilter.IpAddress != null)
        {
            var filterIpAddress = banFilter.IpAddress.GetAddressBytes();
            query = query.Where(ban => ban.BannedIpAddress == filterIpAddress
                                       || (ban.BannedCidrLowerIpAddress != null
                                           && ban.BannedCidrUpperIpAddress != null
                                           && ban.BannedCidrLowerIpAddress.Compare(filterIpAddress) <= 0
                                           && ban.BannedCidrUpperIpAddress.Compare(filterIpAddress) >= 0));
        }

        if (!string.IsNullOrEmpty(banFilter.CountryIsoCode))
        {
            query = query.Where(ban => ban.CountryIsoCode == banFilter.CountryIsoCode);
        }

        if (banFilter.AutonomousSystemNumber != null)
        {
            query = query.Where(ban => ban.AutonomousSystemNumber == banFilter.AutonomousSystemNumber);
        }

        if (!string.IsNullOrEmpty(banFilter.AutonomousSystemOrganization))
        {
            query = query.Where(ban => ban.AutonomousSystemOrganization != null
                                       && ban.AutonomousSystemOrganization.Contains(banFilter.AutonomousSystemOrganization));
        }

        if (banFilter.RelatedPostId != null)
        {
            query = query.Where(ban => ban.RelatedPostId == banFilter.RelatedPostId);
        }

        if (banFilter.CategoryId != null)
        {
            query = query.Where(ban => ban.CategoryId == banFilter.CategoryId);
        }

        query = query.ApplyOrderByAndPaging(banFilter, x => x.CreatedAt);

        var totalCount = await query.CountAsync();

        var data = await query.Select(ban => new BanDetailsModel
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
                RelatedPostId = ban.RelatedPostId,
                CategoryId = ban.CategoryId,
                CreatedById = ban.CreatedById,
                ModifiedById = ban.ModifiedById,
            })
            .ToListAsync();

        return new PagedResult<BanDetailsModel>(data, banFilter, totalCount);
    }

    public async Task<BanDetailsModel?> GetBanAsync(int banId)
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
                RelatedPostId = ban.RelatedPostId,
                CategoryId = ban.CategoryId,
                CreatedById = ban.CreatedById,
                ModifiedById = ban.ModifiedById,
            })
            .OrderBy(ban => ban.Id)
            .FirstOrDefaultAsync();

        return ban;
    }

    public async Task<int> CreateBanAsync(BanCreateRequestModel banCreateRequest)
    {
        using var activity = RepositoriesTelemetry.BanSource.StartActivity();

        var user = _userContext.GetRequiredUser();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var relatedPostId = banCreateRequest.RelatedPostId;

        // user can't be banned twice for one post
        if (relatedPostId != null)
        {
            var banExists = await _applicationDbContext.Bans
                .TagWithCallSite()
                .AnyAsync(ban => !ban.IsDeleted && ban.RelatedPostId == relatedPostId);
            if (banExists)
            {
                throw new HikkabaDomainException("User was already banned for this post");
            }
        }

        var ban = new Ban
        {
            CreatedAt = utcNow,
            EndsAt = banCreateRequest.EndsAt,
            IpAddressType = banCreateRequest.IpAddressType,
            BannedIpAddress = banCreateRequest.BannedIpAddress,
            BannedCidrLowerIpAddress = banCreateRequest.BannedCidrLowerIpAddress,
            BannedCidrUpperIpAddress = banCreateRequest.BannedCidrUpperIpAddress,
            CountryIsoCode = banCreateRequest.CountryIsoCode,
            AutonomousSystemNumber = banCreateRequest.AutonomousSystemNumber,
            AutonomousSystemOrganization = banCreateRequest.AutonomousSystemOrganization,
            Reason = banCreateRequest.Reason,
            RelatedPostId = banCreateRequest.RelatedPostId,
            CategoryId = banCreateRequest.CategoryId,
            CreatedById = user.Id,
        };

        await _applicationDbContext.Bans.AddAsync(ban);
        await _applicationDbContext.SaveChangesAsync();

        return ban.Id;
    }

    public async Task SetBanDeletedAsync(int banId, bool isDeleted)
    {
        using var activity = RepositoriesTelemetry.BanSource.StartActivity();

        var user = _userContext.GetRequiredUser();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        await _applicationDbContext.Bans
            .TagWithCallSite()
            .Where(ban => ban.Id == banId)
            .ExecuteUpdateAsync(setProp =>
                setProp
                    .SetProperty(ban => ban.IsDeleted, isDeleted)
                    .SetProperty(ban => ban.ModifiedAt, utcNow)
                    .SetProperty(ban => ban.ModifiedById, user.Id));
    }
}
