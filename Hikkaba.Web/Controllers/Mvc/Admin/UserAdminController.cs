using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Role;
using Hikkaba.Infrastructure.Models.User;
using Hikkaba.Shared.Constants;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.Mappings;
using Hikkaba.Web.Utils;
using Hikkaba.Web.ViewModels.UserViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hikkaba.Web.Controllers.Mvc.Admin;

[Authorize(Roles = Defaults.AdministratorRoleName)]
[Route("admin/users")]
public sealed class UserAdminController : BaseMvcController
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;

    public UserAdminController(
        IUserService userService,
        IRoleService roleService)
    {
        _userService = userService;
        _roleService = roleService;
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
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var filter = new RoleFilter
        {
            OrderBy = [nameof(RoleModel.Name)],
        };
        var roles = await _roleService.ListRolesAsync(filter, cancellationToken);
        var rolesSelectList = roles.Select(r =>
            new SelectListItem(r.Name, r.Id.ToString(CultureInfo.InvariantCulture), false))
            .ToList()
            .AsReadOnly();

        var viewModel = new UserCreateViewModel
        {
            Email = string.Empty,
            UserName = string.Empty,
            Password = string.Empty,
            UserRoleIds = [],
            AllExistingRoles = rolesSelectList,
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
            var filter = new RoleFilter
            {
                OrderBy = [nameof(RoleModel.Name)],
            };
            var roles = await _roleService.ListRolesAsync(filter, cancellationToken);
            var rolesSelectList = roles.Select(r =>
                    new SelectListItem(r.Name, r.Id.ToString(CultureInfo.InvariantCulture), false))
                .ToList()
                .AsReadOnly();

            viewModel.AllExistingRoles = rolesSelectList;

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
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.ModelErrorsToString();
            ViewBag.ErrorMessage = errorMessage;
            return CustomErrorPage(StatusCodes.Status400BadRequest, errorMessage, GetLocalReferrerOrNull());
        }

        var user = await _userService.GetUserAsync(userId, cancellationToken);
        if (user is null)
        {
            return CustomErrorPage(
                StatusCodes.Status404NotFound,
                "The requested user was not found.",
                GetLocalReferrerOrRoute("HomeIndex"));
        }

        var filter = new RoleFilter
        {
            OrderBy = [nameof(RoleModel.Name)],
        };
        var roles = await _roleService.ListRolesAsync(filter, cancellationToken);
        var rolesSelectList = roles.Select(r =>
                new SelectListItem(r.Name, r.Id.ToString(CultureInfo.InvariantCulture), false))
            .ToList()
            .AsReadOnly();

        var viewModel = user.ToEditViewModel();
        viewModel.AllExistingRoles = rolesSelectList;

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
            var filter = new RoleFilter
            {
                OrderBy = [nameof(RoleModel.Name)],
            };
            var roles = await _roleService.ListRolesAsync(filter, cancellationToken);
            var rolesSelectList = roles.Select(r =>
                    new SelectListItem(r.Name, r.Id.ToString(CultureInfo.InvariantCulture), false))
                .ToList()
                .AsReadOnly();

            viewModel.AllExistingRoles = rolesSelectList;

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
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.ModelErrorsToString();
            ViewBag.ErrorMessage = errorMessage;
            return CustomErrorPage(StatusCodes.Status400BadRequest, errorMessage, GetLocalReferrerOrNull());
        }

        await _userService.SetUserDeletedAsync(userId, isDeleted, cancellationToken);
        return RedirectToRoute("UserIndex");
    }
}
