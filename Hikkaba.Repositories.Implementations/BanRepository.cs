using System.Net;
using Hikkaba.Common.Exceptions;
using Hikkaba.Common.Extensions;
using Hikkaba.Common.Services.Contracts;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Paging.Extensions;
using Hikkaba.Paging.Models;
using Hikkaba.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Repositories.Implementations;

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

    public async Task<BanPreviewRm?> FindActiveBan(long? threadId, string? categoryAlias, string userIpAddress)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var userIp = IPAddress.Parse(userIpAddress).GetAddressBytes();

        var activeBan = await _applicationDbContext.Bans
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
            .Select(ban => new BanPreviewRm
            {
                Id = ban.Id,
                EndsAt = ban.EndsAt,
                Reason = ban.Reason,
            })
            .FirstOrDefaultAsync();

        return activeBan;
    }

    public async Task<PagedResult<BanRm>> ListBansPaginatedAsync(BanPagingFilter banFilter)
    {
        var query = _applicationDbContext.Bans
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

        var total = await query.CountAsync();

        var data = await query.Select(ban => new BanRm
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

        return new PagedResult<BanRm>(data, banFilter, total);
    }

    public async Task<BanRm?> GetBanAsync(int banId)
    {
        var ban = await _applicationDbContext.Bans
            .Where(ban => ban.Id == banId)
            .Select(ban => new BanRm
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
            .FirstOrDefaultAsync();

        return ban;
    }

    public async Task<int> CreateBanAsync(BanCreateRequestRm banCreateRequest)
    {
        var user = _userContext.GetRequiredUser();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var relatedPostId = banCreateRequest.RelatedPostId;

        // user can't be banned twice for one post
        if (relatedPostId != null)
        {
            var banExists = await _applicationDbContext.Bans
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
        var user = _userContext.GetRequiredUser();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        await _applicationDbContext.Bans
            .Where(ban => ban.Id == banId)
            .ExecuteUpdateAsync(setProp =>
                setProp
                    .SetProperty(ban => ban.IsDeleted, isDeleted)
                    .SetProperty(ban => ban.ModifiedAt, utcNow)
                    .SetProperty(ban => ban.ModifiedById, user.Id));
    }
}
