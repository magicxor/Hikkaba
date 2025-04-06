using Hikkaba.Web.ViewModels.BansViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Extensions;

internal static class BanViewModelExtensions
{
    public static string? GetUri(this BanViewModel banViewModel, IUrlHelper urlHelper)
    {
        return urlHelper.Action(
            "Details",
            "Threads",
            new
            {
                categoryAlias = banViewModel.CategoryAlias,
                threadId = banViewModel.RelatedThreadId,
            }) + "#" + banViewModel.RelatedPostId;
    }
}
