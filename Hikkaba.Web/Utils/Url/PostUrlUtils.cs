using Hikkaba.Web.ViewModels.PostsViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Extensions;

internal static class PostUrlUtils
{
    public static string? GetThreadDetailsPostUri(PostDetailsViewModel postDetailsViewModel, IUrlHelper urlHelper)
    {
        return urlHelper.RouteUrl(
            "ThreadDetails",
            new
            {
                categoryAlias = postDetailsViewModel.CategoryAlias,
                threadId = postDetailsViewModel.ThreadId,
            },
            protocol: null,
            host: null,
            fragment: $"{postDetailsViewModel.Id}");
    }
}
