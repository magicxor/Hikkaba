using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.User;
using Hikkaba.Shared.Constants;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.Mappings;
using Hikkaba.Web.Utils;
using Hikkaba.Web.ViewModels.UserViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc.Admin;

[Authorize(Roles = Defaults.AdministratorRoleName)]
[Route("admin/users")]
public class UserAdminController : BaseMvcController
{
    private readonly IUserService _userService;

    public UserAdminController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("", Name = "UserIndex")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var filter = new UserFilter
        {
            OrderBy = [nameof(UserDetailsModel.UserName)],
            IncludeDeleted = true,
        };
        var users = await _userService.ListUsersAsync(filter, cancellationToken);
        var vm = users.ToViewModels();

        return View(vm);
    }

    [HttpGet("create", Name = "UserCreate")]
    public IActionResult Create()
    {
        var viewModel = new UserCreateViewModel
        {
            Email = string.Empty,
            UserName = string.Empty,
            Password = string.Empty,
        };
        return View(viewModel);
    }

    [HttpPost("create", Name = "UserCreateConfirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateConfirm(
        [Required] [FromForm] UserCreateViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View("Create", viewModel);
        }

        var requestModel = viewModel.ToCreateModel();
        var result = await _userService.CreateUserAsync(requestModel, cancellationToken);

        return result.Match(
            _ => RedirectToRoute("UserIndex"),
            err => CustomErrorPage(err.StatusCode, err.ErrorMessage, GetLocalReferrerOrNull()));
    }

    [HttpGet("{userId:int}/edit", Name = "UserEdit")]
    public async Task<IActionResult> Edit(
        [Required] [FromRoute] int userId,
        CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserAsync(userId, cancellationToken);
        if (user is null)
        {
            return CustomErrorPage(
                StatusCodes.Status404NotFound,
                "The requested user was not found.",
                GetLocalReferrerOrRoute("HomeIndex"));
        }

        var viewModel = user.ToEditViewModel();
        return View(viewModel);
    }

    [HttpPost("{userId:int}/edit", Name = "UserEditConfirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditConfirm(
        [Required] [FromRoute] int userId,
        [Required] [FromForm] UserEditViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View("Edit", viewModel);
        }

        var requestModel = viewModel.ToEditModel();
        var result = await _userService.EditUserAsync(requestModel, cancellationToken);

        return result.Match(
            _ => RedirectToRoute("UserIndex"),
            err => CustomErrorPage(err.StatusCode, err.ErrorMessage, GetLocalReferrerOrNull()));
    }

    [HttpPost("{userId:int}/set-deleted", Name = "UserSetDeleted")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDeleted(
        [Required] [FromRoute] int userId,
        [Required] [FromForm] bool isDeleted,
        CancellationToken cancellationToken)
    {
        await _userService.SetUserDeletedAsync(userId, isDeleted, cancellationToken);
        return RedirectToRoute("UserIndex");
    }
}
