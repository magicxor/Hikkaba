using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Paging.Enums;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Hikkaba.Web.ViewModels.ThreadsViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hikkaba.Paging.Models;
using Hikkaba.Application.Contracts;
using Hikkaba.Shared.Constants;
using Hikkaba.Web.Mappings;
using Hikkaba.Web.Utils;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.Controllers.Mvc;

[AllowAnonymous]
[Route("categories")]
public sealed class CategoryController : BaseMvcController
{
    private readonly ICategoryService _categoryService;
    private readonly IThreadService _threadService;

    public CategoryController(
        ICategoryService categoryService,
        IThreadService threadService)
    {
        _categoryService = categoryService;
        _threadService = threadService;
    }

    [HttpGet("/{categoryAlias}", Name = "CategoryDetails")]
    public async Task<IActionResult> Details(
        [Required] [FromRoute] [MaxLength(Defaults.MaxCategoryAliasLength)] string categoryAlias,
        [FromQuery] [Range(1, int.MaxValue)] int page = 1,
        [FromQuery] [Range(1, 100)] int size = 10,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.ModelErrorsToString();
            ViewBag.ErrorMessage = errorMessage;
            return CustomErrorPage(StatusCodes.Status400BadRequest, errorMessage, GetLocalReferrerOrNull());
        }

        var category = await _categoryService.GetCategoryAsync(categoryAlias, false, cancellationToken);
        if (category is null)
        {
            var returnUrl = GetLocalReferrerOrRoute("HomeIndex");
            return CustomErrorPage(
                StatusCodes.Status404NotFound,
                "The requested category was not found.",
                returnUrl);
        }

        var filter = new ThreadPreviewFilter
        {
            CategoryAlias = categoryAlias,
            PageNumber = page,
            PageSize = size,
            OrderBy =
            [
                new OrderByItem { Field = nameof(ThreadPreviewModel.IsPinned), Direction = OrderByDirection.Desc },
                new OrderByItem { Field = nameof(ThreadPreviewModel.LastBumpAt), Direction = OrderByDirection.Desc },
                new OrderByItem { Field = nameof(ThreadPreviewModel.Id), Direction = OrderByDirection.Desc },
            ],
        };
        var threads = await _threadService.ListThreadPreviewsPaginatedAsync(filter, cancellationToken);

        var categoryDetailsViewModel = new CategoryThreadsViewModel
        {
            Category = category.ToViewModel(),
            Threads = new PagedResult<ThreadDetailsViewModel>(threads.Data.ToViewModels(), filter, threads.TotalItemCount),
        };
        return View(categoryDetailsViewModel);
    }
}
