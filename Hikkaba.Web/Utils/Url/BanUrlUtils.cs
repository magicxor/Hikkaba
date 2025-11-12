using Hikkaba.Web.Services.Contracts;
using Hikkaba.Web.ViewModels.BansViewModels;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.Utils.Url;

public static class BanUrlUtils
{
    public static string? GetThreadDetailsPostUri(BanViewModel banViewModel, ILinkBuilder urlHelper)
    {
        return urlHelper.RouteUrl(
            "ThreadDetails",
            new
            {
                categoryAlias = banViewModel.CategoryAlias,
                threadId = banViewModel.RelatedThreadId,
            },
            fragment: new FragmentString($"#{banViewModel.RelatedPostId}"));
    }
}
