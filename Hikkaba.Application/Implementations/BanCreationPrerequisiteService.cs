using System.Net;
using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Infrastructure.Models.Post;

namespace Hikkaba.Application.Implementations;

public class BanCreationPrerequisiteService : IBanCreationPrerequisiteService
{
    private readonly IPostService _postService;
    private readonly IBanService _banService;
    private readonly IGeoIpService _geoIpService;

    public BanCreationPrerequisiteService(
        IPostService postService,
        IBanService banService,
        IGeoIpService geoIpService)
    {
        _postService = postService;
        _banService = banService;
        _geoIpService = geoIpService;
    }

    public async Task<BanCreationPrerequisites> GetPrerequisitesAsync(long? postId, long threadId, CancellationToken cancellationToken)
    {
        var threadPosts = await _postService.ListThreadPostsAsync(new ThreadPostsFilter
        {
            PostId = postId,
            ThreadId = threadId,
            IncludeDeleted = true,
        }, cancellationToken);

        if (!threadPosts.Any())
        {
            return new BanCreationPrerequisites { Status = BanCreationPrerequisiteStatus.PostNotFound };
        }

        var threadPost = threadPosts[0];

        if (threadPost.UserIpAddress == null)
        {
            return new BanCreationPrerequisites { Status = BanCreationPrerequisiteStatus.IpAddressNull };
        }

        var activeBan = await _banService.FindActiveBan(new ActiveBanFilter
        {
            UserIpAddress = threadPost.UserIpAddress,
            CategoryAlias = threadPost.CategoryAlias,
            ThreadId = threadPost.ThreadId,
        }, cancellationToken);

        if (activeBan != null)
        {
            return new BanCreationPrerequisites { Status = BanCreationPrerequisiteStatus.ActiveBanFound, ActiveBanId = activeBan.Id };
        }

        // Use IP from the fetched post
        var ipAddressInfo = _geoIpService.GetIpAddressInfo(new IPAddress(threadPost.UserIpAddress));

        return new BanCreationPrerequisites
        {
            Post = threadPost,
            IpAddressInfo = ipAddressInfo,
            Status = BanCreationPrerequisiteStatus.Success
        };
    }
}
