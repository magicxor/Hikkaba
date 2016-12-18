using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Service.Base;
using Hikkaba.Web.ViewModels.PostsViewModels;

namespace Hikkaba.Web.ViewModels
{
    public class SearchResultViewModel
    {
        public string Query { get; set; }
        public BasePagedList<PostDetailsViewModel> Posts { get; set; }
    }
}
