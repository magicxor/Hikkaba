using Hikkaba.Web.Services.Contracts;
using Hikkaba.Web.ViewModels.ThreadsViewModels;

namespace Hikkaba.Web.Utils.Url;

public static class ThreadUrlUtils
{
    public static string? GetThreadDetailsUri(ThreadDetailsViewModel threadDetailsViewModel, ILinkBuilder urlHelper)
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
