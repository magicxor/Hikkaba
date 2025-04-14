using System.Net;
using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Models.Post;

namespace Hikkaba.Application.Implementations;

public sealed class BanCreationPrerequisiteService : IBanCreationPrerequisiteService
{
    private readonly IPostService _postService;
    private readonly IBanService _banService;
    private readonly IGeoIpService _geoIpService;
    private readonly IIpAddressCalculator _ipAddressCalculator;

    public BanCreationPrerequisiteService(
        IPostService postService,
        IBanService banService,
        IGeoIpService geoIpService,
        IIpAddressCalculator ipAddressCalculator)
    {
        _postService = postService;
        _banService = banService;
        _geoIpService = geoIpService;
        _ipAddressCalculator = ipAddressCalculator;
    }

    public async Task<BanCreationPrerequisitesModel> GetPrerequisitesAsync(long? postId, long threadId, CancellationToken cancellationToken)
    {
        var threadPosts = await _postService.ListThreadPostsAsync(new ThreadPostsFilter
        {
            PostId = postId,
            ThreadId = threadId,
            IncludeDeleted = true,
        }, cancellationToken);

        if (!threadPosts.Any())
        {
            return new BanCreationPrerequisitesModel { Status = BanCreationPrerequisiteStatus.PostNotFound };
        }

        var threadPost = threadPosts[0];

        if (threadPost.UserIpAddress == null)
        {
            return new BanCreationPrerequisitesModel { Status = BanCreationPrerequisiteStatus.IpAddressNull };
        }

        var userIpAddress = new IPAddress(threadPost.UserIpAddress);

        if (_ipAddressCalculator.IsPrivate(userIpAddress))
        {
            return new BanCreationPrerequisitesModel { Status = BanCreationPrerequisiteStatus.IpAddressIsLocalOrPrivate };
        }

        var activeBan = await _banService.FindActiveBan(new ActiveBanFilter
        {
            UserIpAddress = threadPost.UserIpAddress,
            CategoryAlias = threadPost.CategoryAlias,
            ThreadId = threadPost.ThreadId,
        }, cancellationToken);

        if (activeBan != null)
        {
            return new BanCreationPrerequisitesModel { Status = BanCreationPrerequisiteStatus.ActiveBanFound, ActiveBanId = activeBan.Id };
        }

        // Use IP from the fetched post
        var ipAddressInfo = _geoIpService.GetIpAddressInfo(userIpAddress);

        return new BanCreationPrerequisitesModel
        {
            Post = threadPost,
            IpAddressInfo = ipAddressInfo,
            Status = BanCreationPrerequisiteStatus.Success,
        };
    }
}
