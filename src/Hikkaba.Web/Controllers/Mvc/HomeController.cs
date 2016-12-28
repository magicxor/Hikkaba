using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Dto;
using Hikkaba.Service;
using Hikkaba.Service.Base;
using Hikkaba.Web.Filters;
using Hikkaba.Web.Utils;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Hikkaba.Web.ViewModels.HomeViewModels;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Hikkaba.Web.ViewModels.SearchViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// todo: add posts numeration
// todo: merge empty lines to one line
// todo: enlarge video on click
// todo: enlarge post form
// todo: in-memory cache

namespace Hikkaba.Web.Controllers.Mvc
{
    [TypeFilter(typeof(ExceptionLoggingFilter))]
    public class HomeController : Controller
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICategoryService _categoryService;
        private readonly IThreadService _threadService;
        private readonly IPostService _postService;

        public HomeController(
            ILogger<HomeController> logger,
            IMapper mapper,
            ICategoryService categoryService, 
            IThreadService threadService, 
            IPostService postService)
        {
            _logger = logger;
            _mapper = mapper;
            _categoryService = categoryService;
            _threadService = threadService;
            _postService = postService;
        }

        public async Task<IActionResult> Index()
        {
            var page = new PageDto();
            var latestPostsDtoList = await _postService
                                        .PagedListAsync(
                                            post => (!post.IsDeleted) && (!post.Thread.IsDeleted) && (!post.Thread.Category.IsHidden),
                                            post => post.Created,
                                            true,
                                            page);
            var latestPostDetailsViewModels = _mapper.Map<List<PostDetailsViewModel>>(latestPostsDtoList.CurrentPageItems);
            foreach (var latestPostDetailsViewModel in latestPostDetailsViewModels)
            {
                var threadDto = await _threadService.GetAsync(latestPostDetailsViewModel.ThreadId);
                var categoryDto = await _categoryService.GetAsync(threadDto.CategoryId);
                latestPostDetailsViewModel.ThreadShowThreadLocalUserHash = threadDto.ShowThreadLocalUserHash;
                latestPostDetailsViewModel.CategoryAlias = categoryDto.Alias;
                latestPostDetailsViewModel.CategoryId = categoryDto.Id;
            }
            var categoriesDtoList = await _categoryService.ListAsync(category => !category.IsHidden && !category.IsDeleted, category => category.Alias);
            var categoryViewModels = _mapper.Map<List<CategoryViewModel>>(categoriesDtoList);
            var homeIndexViewModel = new HomeIndexViewModel()
            {
                Categories = categoryViewModels,
                Posts = new BasePagedList<PostDetailsViewModel>()
                {
                    CurrentPage = page,
                    CurrentPageItems = latestPostDetailsViewModels,
                    TotalItemsCount = latestPostsDtoList.TotalItemsCount,
                },
            };
            return View(homeIndexViewModel);
        }
    }
}
