using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Shared.Constants;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Paging.Enums;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Hikkaba.Web.ViewModels.ThreadsViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Hikkaba.Paging.Models;
using Hikkaba.Application.Contracts;
using Hikkaba.Web.Mappings;

namespace Hikkaba.Web.Controllers.Mvc;

[Authorize(Roles = Defaults.AdministratorRoleName)]
public class CategoriesController : BaseMvcController
{
    private readonly IBoardService _boardService;
    private readonly ICategoryService _categoryService;
    private readonly IThreadService _threadService;

    public CategoriesController(
        UserManager<ApplicationUser> userManager,
        IBoardService boardService,
        ICategoryService categoryService,
        IThreadService threadService)
        : base(userManager)
    {
        _boardService = boardService;
        _categoryService = categoryService;
        _threadService = threadService;
    }

    [AllowAnonymous]
    [Route("{categoryAlias}")]
    public async Task<IActionResult> Details(
        string categoryAlias,
        int page = 1,
        int size = 10,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryService.GetAsync(categoryAlias, false);
        if (category is null)
        {
            // todo: add 404 page
            return NotFound();
        }

        var filter = new ThreadPreviewFilter
        {
            CategoryAlias = categoryAlias,
            PageNumber = page,
            PageSize = size,
            OrderBy = [
                new OrderByItem { Field = nameof(ThreadPreviewModel.IsPinned), Direction = OrderByDirection.Desc },
                new OrderByItem { Field = nameof(ThreadPreviewModel.LastPostCreatedAt), Direction = OrderByDirection.Desc },
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

    /*
    [Route("Categories/Create")]
    public IActionResult Create()
    {
        return View();
    }

    [Route("Categories/Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryCreateViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var board = await _boardService.GetBoardAsync();
            var dto = _mapper.Map<CategoryDto>(viewModel);
            dto.BoardId = board.Id;
            var id = await _categoryService.CreateAsync(dto);
            return RedirectToAction("Index", "Administration");
        }
        else
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View(viewModel);
        }
    }

    [Route("Categories/{id}/Edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _categoryService.GetAsync(id);
        var viewModel = _mapper.Map<CategoryEditViewModel>(dto);
        return View(viewModel);
    }

    [Route("Categories/{id}/Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryEditViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var dto = await _categoryService.GetAsync(viewModel.Id);
            _mapper.Map(viewModel, dto);
            await _categoryService.EditAsync(dto);
            return RedirectToAction("Index", "Administration");
        }
        else
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View(viewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetIsDeleted(int id, bool isDeleted)
    {
        var categoryDto = await _categoryService.GetAsync(id);
        var isCurrentUserCategoryModerator = await _categoryToModeratorService
            .IsUserCategoryModeratorAsync(categoryDto.Id, User);
        if (isCurrentUserCategoryModerator)
        {
            await _categoryService.SetIsDeletedAsync(id, isDeleted);
            return RedirectToAction("Index", "Administration");
        }
        else
        {
            throw new HikkabaHttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
        }
    }

    [Route("Categories/{id}/AddModerator")]
    public async Task<IActionResult> AddModerator(int id)
    {
        var categoryModeratorsDto = await _categoryService.ListCategoryModeratorsAsync(new CategoryModeratorsFilter
        {
            CategoryId = id,
            OrderBy = [nameof(ApplicationUser.NormalizedUserName)],
        });

        return View(categoryModeratorsDto);
    }

    [Route("Categories/{id}/AddModerator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddModeratorConfirmed(int id, int moderatorId)
    {
        await _categoryToModeratorService.AddAsync(id, moderatorId);
        return RedirectToAction("Index", "Administration");
    }

    [Route("Categories/{id}/RemoveModerator")]
    public async Task<IActionResult> RemoveModerator(int id)
    {
        var categoryModeratorsDto = await _categoryService.ListCategoryModeratorsAsync(new CategoryModeratorsFilter
        {
            CategoryId = id,
            OrderBy = [nameof(ApplicationUser.NormalizedUserName)],
        });

        return View(categoryModeratorsDto);
    }

    [Route("Categories/{id}/RemoveModerator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveModeratorConfirmed(int id, int moderatorId)
    {
        await _categoryToModeratorService.DeleteAsync(id, moderatorId);
        return RedirectToAction("Index", "Administration");
    }
    */
}
