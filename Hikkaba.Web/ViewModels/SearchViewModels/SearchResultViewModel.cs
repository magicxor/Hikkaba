using Hikkaba.Services.Base.Generic;
using Hikkaba.Web.ViewModels.PostsViewModels;

namespace Hikkaba.Web.ViewModels.SearchViewModels
{
    public class SearchResultViewModel
    {
        public string Query { get; set; }
        public BasePagedList<PostDetailsViewModel> Posts { get; set; }
    }
}
