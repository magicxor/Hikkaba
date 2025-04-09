using Hikkaba.Web.ViewModels.BansViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Extensions;

internal static class BanUrlUtils
{
    public static string? GetThreadDetailsPostUri(BanViewModel banViewModel, IUrlHelper urlHelper)
    {
        return urlHelper.RouteUrl(
            "ThreadDetails",
            new
            {
                categoryAlias = banViewModel.CategoryAlias,
                threadId = banViewModel.RelatedThreadId,
            },
            protocol: null,
            host: null,
            fragment: $"{banViewModel.RelatedPostId}");
    }
}
