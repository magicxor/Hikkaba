using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Infrastructure.Models.User;
using Hikkaba.Shared.Constants;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.Mappings;
using Hikkaba.Web.Utils;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hikkaba.Web.Controllers.Mvc.Admin;

[Authorize(Roles = Defaults.AdministratorRoleName)]
[Route("admin/categories")]
public class CategoryAdminController : BaseMvcController
{
    private readonly ICategoryService _categoryService;
    private readonly IUserService _userService;

    public CategoryAdminController(
        ICategoryService categoryService,
        IUserService userService)
    {
        _categoryService = categoryService;
        _userService = userService;
    }

    [HttpGet("", Name = "CategoryIndex")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var filter = new CategoryFilter
        {
            OrderBy = [nameof(CategoryDetailsViewModel.Alias)],
            IncludeHidden = true,
            IncludeDeleted = true,
        };
        var categories = await _categoryService.ListCategoriesAsync(filter, cancellationToken);
        var viewModels = categories.ToViewModels();
        return View(viewModels);
    }

    [HttpGet("{categoryAlias}/moderators", Name = "CategorySetModerators")]
    public async Task<IActionResult> SetModerators(
        [Required] [FromRoute] string categoryAlias,
        CancellationToken cancellationToken)
    {
        var filter = new CategoryModeratorFilter
        {
            OrderBy = [nameof(UserDetailsModel.UserName)],
            IncludeDeleted = false,
            CategoryAlias = categoryAlias,
        };
        var users = await _userService.ListCategoryModerators(filter, cancellationToken);
        var currentModeratorIds = users
            .Where(u => u.IsCategoryModerator)
            .Select(u => u.Id)
            .ToList()
            .AsReadOnly();
        var userSelectList = users
            .Select(u =>
                new SelectListItem(u.UserName, u.Id.ToString(CultureInfo.InvariantCulture), u.IsCategoryModerator))
            .ToList()
            .AsReadOnly();

        var vm = new SetModeratorsViewModel
        {
            CategoryAlias = categoryAlias,
            ModeratorIds = currentModeratorIds,
            Users = userSelectList,
        };

        return View(vm);
    }

    [HttpPost("{categoryAlias}/moderators", Name = "CategorySetModeratorsConfirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetModeratorsConfirm(
        [Required] [FromRoute] string categoryAlias,
        [Required] [FromForm] SetModeratorsRequestViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.ModelErrorsToString();
            ViewBag.ErrorMessage = errorMessage;
            return CustomErrorPage(StatusCodes.Status500InternalServerError, errorMessage, GetLocalReferrerOrNull());
        }

        await _categoryService.SetCategoryModeratorsAsync(
            categoryAlias,
            viewModel.ModeratorIds,
            cancellationToken);

        return RedirectToRoute("CategoryIndex");
    }

    [HttpGet("create", Name = "CategoryCreate")]
    public IActionResult Create()
    {
        var viewModel = new CategoryCreateViewModel
        {
            Alias = string.Empty,
            Name = string.Empty,
            IsHidden = false,
            DefaultBumpLimit = Defaults.DefaultBumpLimit,
            ShowThreadLocalUserHash = false,
            ShowCountry = false,
            ShowOs = false,
            ShowBrowser = false,
            MaxThreadCount = Defaults.MaxThreadCountInCategory,
        };
        return View(viewModel);
    }

    [HttpPost("create", Name = "CategoryCreateConfirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateConfirm(
        [Required] [FromForm] CategoryCreateViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View("Create", viewModel);
        }
        var requestModel = viewModel.ToModel();
        await _categoryService.CreateCategoryAsync(requestModel, cancellationToken);
        return RedirectToRoute("CategoryIndex");
    }

    [HttpGet("{categoryAlias}/edit", Name = "CategoryEdit")]
    public async Task<IActionResult> Edit(
        [Required] [FromRoute] string categoryAlias,
        CancellationToken cancellationToken)
    {
        var category = await _categoryService.GetCategoryAsync(categoryAlias, true, cancellationToken);
        if (category is null)
        {
            return CustomErrorPage(
                StatusCodes.Status404NotFound,
                "The requested category was not found.",
                GetLocalReferrerOrRoute("HomeIndex"));
        }

        var viewModel = category.ToEditViewModel();
        return View(viewModel);
    }

    [HttpPost("{categoryAlias}/edit", Name = "CategoryEditConfirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditConfirm(
        [Required] [FromRoute] string categoryAlias,
        [Required] [FromForm] CategoryEditViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View("Edit", viewModel);
        }

        var requestModel = viewModel.ToModel();
        await _categoryService.EditCategoryAsync(requestModel, cancellationToken);
        return RedirectToRoute("CategoryIndex");
    }

    [HttpPost("{categoryAlias}/set-deleted", Name = "CategorySetDeleted")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDeleted(
        [Required] [FromRoute] string categoryAlias,
        [Required] [FromForm] bool isDeleted,
        CancellationToken cancellationToken)
    {
        await _categoryService.SetCategoryDeletedAsync(categoryAlias, isDeleted, cancellationToken);
        return RedirectToRoute("CategoryIndex");
    }
}
