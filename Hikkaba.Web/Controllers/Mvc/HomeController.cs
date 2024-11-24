using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Models.Dto;
using Hikkaba.Models.Enums;
using Hikkaba.Services;
using Hikkaba.Services.Base.Generic;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Hikkaba.Web.ViewModels.HomeViewModels;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.Controllers.Mvc;

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
                AdditionalRecordType.None,
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
        var categoryViewModels = _mapper.Map<List<CategoryDetailsViewModel>>(categoriesDtoList);
        var homeIndexViewModel = new HomeIndexViewModel
        {
            Categories = categoryViewModels,
            Posts = new BasePagedList<PostDetailsViewModel>
            {
                CurrentPage = page,
                CurrentPageItems = latestPostDetailsViewModels,
                TotalItemsCount = latestPostsDtoList.TotalItemsCount,
            },
        };
        return View(homeIndexViewModel);
    }
}