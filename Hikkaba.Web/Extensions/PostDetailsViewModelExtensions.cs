using Hikkaba.Web.ViewModels.PostsViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Extensions;

internal static class PostDetailsViewModelExtensions
{
    public static string GetUri(this PostDetailsViewModel postDetailsViewModel, IUrlHelper urlHelper)
    {
        return urlHelper.Action(
            "Details",
            "Threads",
            new
            {
                categoryAlias = postDetailsViewModel.CategoryAlias,
                threadId = postDetailsViewModel.ThreadId,
            }) + "#" + postDetailsViewModel.Id;
    }
}
