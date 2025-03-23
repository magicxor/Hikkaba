using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Services.Contracts;
using Hikkaba.Web.Mappings;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Hikkaba.Web.ViewModels.HomeViewModels;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.Controllers.Mvc;

public class HomeController : Controller
{
    private readonly ILogger _logger;
    private readonly ICategoryService _categoryService;
    private readonly IThreadService _threadService;
    private readonly IPostService _postService;

    public HomeController(
        ILogger<HomeController> logger,
        ICategoryService categoryService,
        IThreadService threadService,
        IPostService postService)
    {
        _logger = logger;
        _categoryService = categoryService;
        _threadService = threadService;
        _postService = postService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var postPagingFilter = new PostPagingFilter
        {
            PageSize = 1,
            PageNumber = 10,
            OrderBy = [new OrderByItem { Field = nameof(Post.CreatedAt), Direction = OrderByDirection.Desc }],
        };
        var posts = await _postService.ListPostsPaginatedAsync(postPagingFilter, cancellationToken);

        var categoryFilter = new CategoryFilter
        {
            OrderBy = [new OrderByItem { Field = nameof(Category.Alias), Direction = OrderByDirection.Asc }],
        };
        var categories = await _categoryService.ListAsync(categoryFilter);

        var homeIndexViewModel = new HomeIndexViewModel
        {
            Categories = categories.ToViewModels(),
            Posts = new PagedResult<PostDetailsViewModel>(posts.Data.ToViewModels(), postPagingFilter),
        };
        return View(homeIndexViewModel);
    }
}
