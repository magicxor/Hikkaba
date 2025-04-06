using Hikkaba.Paging.Models;

using Hikkaba.Web.ViewModels.PostsViewModels;

namespace Hikkaba.Web.ViewModels.SearchViewModels;

public class PostSearchResultViewModel
{
    public required string Query { get; set; }
    public required PagedResult<PostDetailsViewModel> Posts { get; set; }
}
