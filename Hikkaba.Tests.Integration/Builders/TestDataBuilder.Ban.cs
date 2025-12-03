using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Hikkaba.Data.Entities;
using Hikkaba.Shared.Enums;

namespace Hikkaba.Tests.Integration.Builders;

internal sealed partial class TestDataBuilder
{
    private readonly List<Ban> _bans = [];
    private Ban? _lastBan;

    public Ban LastBan => _lastBan ?? throw new InvalidOperationException("No ban created yet.");
    public int LastBanId => _lastBan?.Id ?? throw new InvalidOperationException("No ban created yet.");

    public TestDataBuilder WithExactBan(
        string ipAddress,
        string reason,
        bool isDeleted = false,
        bool isExpired = false,
        bool inCategory = false,
        TimeSpan? createdAtOffset = null,
        TimeSpan? endsAtOffset = null,
        string? countryIsoCode = null,
        int? autonomousSystemNumber = null,
        string? autonomousSystemOrganization = null,
        long? relatedPostId = null)
    {
        EnsureAdminExists();
        if (inCategory)
        {
            EnsureCategoryExists();
        }

        var ip = IPAddress.Parse(ipAddress);
        var ipType = ip.AddressFamily == AddressFamily.InterNetwork
            ? IpAddressType.IpV4
            : IpAddressType.IpV6;

        var utcNow = TimeProvider.GetUtcNow().UtcDateTime;
        DateTime createdAt;
        DateTime endsAt;

        if (createdAtOffset.HasValue || endsAtOffset.HasValue)
        {
            createdAt = utcNow.Add(createdAtOffset ?? TimeSpan.Zero);
            endsAt = utcNow.Add(endsAtOffset ?? TimeSpan.FromDays(365 * 99));
        }
        else
        {
            createdAt = isExpired ? utcNow.AddDays(-30) : utcNow;
            endsAt = isExpired ? utcNow.AddDays(-1) : utcNow.AddYears(99);
        }

        var ban = new Ban
        {
            IsDeleted = isDeleted,
            CreatedAt = createdAt,
            EndsAt = endsAt,
            IpAddressType = ipType,
            BannedIpAddress = ip.GetAddressBytes(),
            Reason = reason,
            Category = inCategory ? LastCategory : null,
            CreatedBy = Admin,
            CountryIsoCode = countryIsoCode,
            AutonomousSystemNumber = autonomousSystemNumber,
            AutonomousSystemOrganization = autonomousSystemOrganization,
            RelatedPostId = relatedPostId,
        };
        _dbContext.Bans.Add(ban);
        _bans.Add(ban);
        _lastBan = ban;
        return this;
    }

    public TestDataBuilder WithBanRelatedToLastPost()
    {
        EnsureLastPostExists();
        if (_lastBan == null)
        {
            throw new InvalidOperationException("Ban must be created first. Call WithExactBan() or WithRangeBan().");
        }

        _lastBan.RelatedPostId = LastPostId;
        return this;
    }

    public TestDataBuilder WithRangeBan(
        string bannedIpAddress,
        string lowerIpAddress,
        string upperIpAddress,
        string reason)
    {
        EnsureAdminExists();

        var ip = IPAddress.Parse(bannedIpAddress);
        var ipType = ip.AddressFamily == AddressFamily.InterNetwork
            ? IpAddressType.IpV4
            : IpAddressType.IpV6;

        var ban = new Ban
        {
            CreatedAt = TimeProvider.GetUtcNow().UtcDateTime,
            EndsAt = TimeProvider.GetUtcNow().UtcDateTime.AddYears(99),
            IpAddressType = ipType,
            BannedIpAddress = ip.GetAddressBytes(),
            BannedCidrLowerIpAddress = IPAddress.Parse(lowerIpAddress).GetAddressBytes(),
            BannedCidrUpperIpAddress = IPAddress.Parse(upperIpAddress).GetAddressBytes(),
            Reason = reason,
            CreatedBy = Admin,
        };
        _dbContext.Bans.Add(ban);
        _bans.Add(ban);
        _lastBan = ban;
        return this;
    }
}
