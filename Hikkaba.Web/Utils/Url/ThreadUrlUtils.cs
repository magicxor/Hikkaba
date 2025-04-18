using Hikkaba.Web.ViewModels.ThreadsViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Utils.Url;

public static class ThreadUrlUtils
{
    public static string? GetThreadDetailsUri(ThreadDetailsViewModel threadDetailsViewModel, IUrlHelper urlHelper)
    {
        return urlHelper.RouteUrl(
            "ThreadDetails",
            new
            {
                categoryAlias = threadDetailsViewModel.CategoryAlias,
                threadId = threadDetailsViewModel.Id,
            });
    }
}
