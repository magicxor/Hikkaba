using Hikkaba.Web.ViewModels.ThreadsViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Extensions;

public static class ThreadDetailsViewModelExtensions
{
    public static string GetUri(this ThreadDetailsViewModel threadDetailsViewModel, IUrlHelper urlHelper)
    {
        return urlHelper.Action("Details", "Threads",
            new
            {
                categoryAlias = threadDetailsViewModel.CategoryAlias,
                threadId = threadDetailsViewModel.Id,
            });
    }
}