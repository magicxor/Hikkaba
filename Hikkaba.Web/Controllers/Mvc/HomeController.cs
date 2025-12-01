using System.Linq;
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc;

[AllowAnonymous]
public sealed class HomeController : Controller
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

    [HttpGet("", Name = "HomeIndex")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var postPagingFilter = new PostPagingFilter
        {
            PageSize = 10,
            PageNumber = 1,
            OrderBy = [new(nameof(Post.CreatedAt), OrderByDirection.Desc)],
        };
        var posts = await _postService.ListPostsAsync(postPagingFilter, cancellationToken);

        var categoryFilter = new CategoryFilter
        {
            OrderBy = [nameof(Category.Alias)],
        };
        var categories = await _categoryService.ListCategoriesAsync(categoryFilter, cancellationToken);
        var categoriesVm = categories.ToViewModels();
        var postsVm = posts.Data.ToViewModels()
            .Select(p => p with { ShowCategoryAlias = true })
            .ToList()
            .AsReadOnly();

        var homeIndexViewModel = new HomeIndexViewModel
        {
            Categories = categoriesVm,
            Posts = new PagedResult<PostDetailsViewModel>(postsVm, postPagingFilter),
        };
        return View(homeIndexViewModel);
    }
}
