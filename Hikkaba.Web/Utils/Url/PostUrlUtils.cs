using Hikkaba.Web.Services.Contracts;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.Utils.Url;

public static class PostUrlUtils
{
    public static string? GetThreadDetailsPostUri(PostDetailsViewModel postDetailsViewModel, ILinkBuilder urlHelper)
    {
        return urlHelper.RouteUrl(
            "ThreadDetails",
            new
            {
                categoryAlias = postDetailsViewModel.CategoryAlias,
                threadId = postDetailsViewModel.ThreadId,
            },
            fragment: new FragmentString($"#{postDetailsViewModel.Id}"));
    }
}
