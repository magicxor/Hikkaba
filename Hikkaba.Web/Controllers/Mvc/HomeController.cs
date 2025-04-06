using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Application.Contracts;
using Hikkaba.Web.Mappings;
using Hikkaba.Web.ViewModels.HomeViewModels;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc;

internal sealed class HomeController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IPostService _postService;

    public HomeController(
        ICategoryService categoryService,
        IPostService postService)
    {
        _categoryService = categoryService;
        _postService = postService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var postPagingFilter = new PostPagingFilter
        {
            PageSize = 10,
            PageNumber = 1,
            OrderBy = [new OrderByItem { Field = nameof(Post.CreatedAt), Direction = OrderByDirection.Desc }],
        };
        var posts = await _postService.ListPostsPaginatedAsync(postPagingFilter, cancellationToken);

        var categoryFilter = new CategoryFilter
        {
            OrderBy = [new OrderByItem { Field = nameof(Category.Alias), Direction = OrderByDirection.Asc }],
        };
        var categories = await _categoryService.ListAsync(categoryFilter, cancellationToken);

        var homeIndexViewModel = new HomeIndexViewModel
        {
            Categories = categories.ToViewModels(),
            Posts = new PagedResult<PostDetailsViewModel>(posts.Data.ToViewModels(), postPagingFilter),
        };
        return View(homeIndexViewModel);
    }
}
